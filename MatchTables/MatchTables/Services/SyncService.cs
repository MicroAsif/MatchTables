using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatchTables.Interfaces;
using MatchTables.Models;
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

                var syncViewModel = CompareResult(sourceData, targetData);
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
        private SyncViewModel CompareResult(List<Customer> sourceData, List<Customer> targetData)
        {
            var originalIds = sourceData.ToArray().ToDictionary(o => o.SocialSecurityNumber, o => o);
            var newIds = targetData.ToArray().ToDictionary(o => o.SocialSecurityNumber, o => o);

            var added = new List<Customer>();
            var modified = new List<Customer>();

            foreach (var row in sourceData)
            {
                if (!newIds.ContainsKey(row.SocialSecurityNumber))
                    added.Add(row);
                else
                {
                    var otherRow = newIds[row.SocialSecurityNumber];
                    if (!otherRow.AreEqual(row))
                    {
                        modified.Add(row);
                        //modified.Add(otherRow);
                    }
                }
            }
            var deleted = targetData.Where(t => !originalIds.ContainsKey(t.SocialSecurityNumber)).ToList();
            var syncViewModel = new SyncViewModel { Added = added, Modified = modified, Deleted = deleted };
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
