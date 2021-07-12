using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MatchTables.ViewModel;

namespace MatchTables.Interfaces
{
    public interface IDataAccessService
    {
        Task<IEnumerable<string>> GetColumnNames(string tableName);
        Task<List<string>> GetPrimaryKeyColumns(string tableName);
        Task<List<Dictionary<string, object>>> GetTableValues(string tableName);
        Task InsertUpdateDelete(SyncViewModel viewModel);
        Task DeleteDataFromDb(string tableName, List<Dictionary<string, object>> data);
    }
}
