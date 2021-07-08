using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MatchTables.Models;

namespace MatchTables.Interfaces
{
    public interface IDataAccessService
    {
        IEnumerable<string> GetColumnNames(string tableName);
        List<string> GetPrimaryKeyColumns(string tableName);
        Task<List<Customer>> GetTableValues(string tableName);
    }
}
