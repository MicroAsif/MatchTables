using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatchTables.Interfaces;
using MatchTables.Models;

namespace MatchTables.Services
{
    public class SyncService : ISyncService
    {
        private readonly IDataAccessService _dataAccessService;

        public SyncService(IDataAccessService dataAccessService)
        {
            _dataAccessService = dataAccessService;
        }

        public async Task StartSyncAsync(string source, string target, string primaryKey)
        {
            var sourceData = await _dataAccessService.GetTableValues(source);
            var targetData = await _dataAccessService.GetTableValues(target);


            var originalIds = sourceData.ToArray().ToDictionary(o => o.SocialSecurityNumber, o => o);
            var newIds = targetData.ToArray().ToDictionary(o => o.SocialSecurityNumber, o => o);

            var deleted = new List<Customer>();
            var modified = new List<Customer>();

            foreach (var row in sourceData)
            {
                if (!newIds.ContainsKey(row.SocialSecurityNumber))
                    deleted.Add(row);
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
            var added = targetData.Where(t => !originalIds.ContainsKey(t.SocialSecurityNumber)).ToList();


        }
    }
}
