using System;
using System.Collections.Generic;
using System.Linq;
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

        private SyncViewModel CompareResult(List<Dictionary<string, object>> sourceData,
            List<Dictionary<string, object>> targetData, string primaryKey)
        {
            var originalIds = sourceData.Select(x => x[primaryKey].ToString()).ToList();
            var newIds = targetData.Select(x => x[primaryKey].ToString()).ToList();

            var added = new List<Dictionary<string, object>>();
            var modified = new List<Dictionary<string, object>>();
            var existing = new List<Dictionary<string, object>>();
            var deleted = new List<Dictionary<string, object>>();


            foreach (var row in sourceData)
            {
                var key = (string) row[primaryKey];
                if (!newIds.Contains(key))
                {
                    added.Add(row);
                }
                else
                {
                    var existingRow = targetData.FirstOrDefault(x => x[primaryKey].ToString() == key);
                    if (!AreDictionariesEqual(row, existingRow))
                    {
                        modified.Add(row);
                        existing.Add(existingRow);
                    }
                }
            }

            foreach (var row in targetData)
            {
                var key = (string) row[primaryKey];
                if (!originalIds.Contains(key))
                    deleted.Add(row);
            }

            var syncViewModel = new SyncViewModel
                {Added = added, Modified = modified, Deleted = deleted, Existing = existing};
            return syncViewModel;
        }

        private bool AreDictionariesEqual(Dictionary<string, object> dict1, Dictionary<string, object> dict2)
        {
            var dictonary1 = "";
            var dictonary2 = "";

            foreach (var d in dict1)
            {
                dictonary1 += d.Value.ToString();
                dictonary2 += dict2[d.Key].ToString();
            }

            if (dictonary1.Equals(dictonary2))
                return true;
            return false;
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