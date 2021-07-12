using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace MatchTables.Helpers
{
    public interface IDbHelper
    {
        Task<DataTable> ExecuteReaderAsync(string query);
        Task<DataTable> ExecuteReaderDatatableAsync(string query);
        Task<DataTable> ExecuteReaderDatatableAsync(string[] restrictions);
        Task<int> ExecuteNonQueryWithDataAsync(string query, Dictionary<string, object> data);
        Task<int> ExecuteNonQueryAsync(string query);

    }
}
