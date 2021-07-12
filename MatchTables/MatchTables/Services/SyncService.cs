using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatchTables.Interfaces;
using MatchTables.ViewModel;
using Microsoft.Extensions.Logging;

namespace MatchTables.Services
{
    public class SyncService : ISyncService
    {
        private readonly IDataAccessService _dataAccessService;
        private readonly ILogger<SyncService> _logger;

        public SyncService(IDataAccessService dataAccessService, ILogger<SyncService> logger)
        {
            _dataAccessService = dataAccessService;
            _logger = logger;
        }

        public async Task StartSyncAsync(string source, string target, string primaryKey)
        {

            var isValid = await IsTableColumnsValid(source, target, primaryKey);
            if (isValid)
            {
                var sourceData = await _dataAccessService.GetTableValues(source);
                var targetData = await _dataAccessService.GetTableValues(target);

                var syncViewModel = CompareResult(sourceData, targetData, primaryKey);
                syncViewModel.TargetedTable = target;

                await _dataAccessService.InsertUpdateDelete(syncViewModel);
            }
            else
            {
                _logger.LogDebug("Failed to validate table");
                throw new Exception("Failed to validate table");
            }
        }



        #region private methods
        private SyncViewModel CompareResult(List<Dictionary<string, object>> sourceData, List<Dictionary<string, object>> targetData, string primaryKey)
        {
            var originalIds = sourceData;
            var newIds = targetData;

            var added = new List<Dictionary<string, object>>();
            var modified = new List<Dictionary<string, object>>();
            var existing = new List<Dictionary<string, object>>();
            var deleted = new List<Dictionary<string, object>>();


            foreach (var row in sourceData)
            {
                string key = (string)row[primaryKey];

                var otherRow = newIds.FirstOrDefault(d => (string)d[primaryKey] == key);
                if (otherRow == null)
                    added.Add(row); 
                else
                {
                    modified.Add(row);
                    var modifiled = row.Where(entry => otherRow[entry.Key] != entry.Value).ToDictionary(entry => entry.Key, entry => entry.Value);
                    
                    existing.Add(modifiled);
                    
                }
            }
           
            foreach(var row in targetData)
            {
                string key = (string)row[primaryKey];

                var otherRow = originalIds.FirstOrDefault(d => (string)d[primaryKey] == key);
                if (otherRow == null)
                    deleted.Add(row);
            }
            var syncViewModel = new SyncViewModel { Added = added, Modified = modified, Deleted = deleted, Exising = existing };
            return syncViewModel;
        }
        private async Task<bool> IsTableColumnsValid(string source, string target, string primaryKey)
        {
            var result = true;

            //two tables primay key check
            var sourceTablePKeys = await _dataAccessService.GetPrimaryKeyColumns(source);
            var targetTablePKeys = await _dataAccessService.GetPrimaryKeyColumns(target);
            
            if (!sourceTablePKeys.All(targetTablePKeys.Contains))
            {
                result = false;
                _logger.LogDebug("Two tables primary key is not same");
                throw new Exception("Two tables primary key is not same");
            }

            if (!(primaryKey.ToLower() == targetTablePKeys.FirstOrDefault().ToLower()))
            {
                result = false;
                _logger.LogDebug("Table primary key is not matched");
                throw new Exception("Table primary key is not matched");
            }

            //two tables columns  check
            var sourceTableColumns = await _dataAccessService.GetColumnNames(source);
            var targetTableColumns = await _dataAccessService.GetColumnNames(target);



            if (!sourceTableColumns.All(targetTableColumns.Contains))
            {
                result = false;
                _logger.LogDebug("Two tables columns are not same");
                throw new Exception("Two tables columns are  is not same");
            }

            return result;
        }
        #endregion

    }
}
