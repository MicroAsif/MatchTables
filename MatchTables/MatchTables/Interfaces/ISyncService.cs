using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MatchTables.Interfaces
{
    public interface ISyncService
    {
        Task StartSyncAsync(string source, string target, string primaryKey);
    }
}
