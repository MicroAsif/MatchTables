using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MatchTables.Helpers;
using MatchTables.Interfaces;
using MatchTables.ViewModel;
using Microsoft.Extensions.Logging;

namespace MatchTables.Services
{
    public class DataAccessService : IDataAccessService
    {
        private readonly ILogger<DataAccessService> _logger;
        private readonly IDbHelper _dbHelper;

        public DataAccessService(ILogger<DataAccessService> logger, IDbHelper dbHelper)
        {
            _logger = logger;
            _dbHelper = dbHelper;
        }
        public async Task<IEnumerable<string>> GetColumnNames(string tableName)
        {
            CheckTableName(tableName);

            var result = new List<string>();
            var query = "select * from " + tableName + " where 1=0"; // No data wanted, only schema  
            var dataTable = await _dbHelper.ExecuteReaderDatatableAsync(query);
            foreach (DataRow row in dataTable.Rows) 
                result.Add(row.Field<string>("ColumnName"));

            return result;
        }
        public async Task<List<string>> GetPrimaryKeyColumns(string tableName)
        {
            CheckTableName(tableName);

            List<string> result = new List<string>();
            string[] restrictions = new string[] { null, null, tableName };
            DataTable table = await _dbHelper.ExecuteReaderDatatableAsync(restrictions);
            foreach (DataRow row in table.Rows)
            {
                result.Add(row["column_name"].ToString());
            }

            return result;
        }
        public async Task<List<Dictionary<string, object>>> GetTableValues(string tableName)
        {
            var customerData = new List<Dictionary<string, object>>();
            string query = $"SELECT * FROM {tableName}";

            var dt= await _dbHelper.ExecuteReaderAsync(query);
            customerData = ConvertDataTable(dt);
            
            return customerData;
        }
        public async Task InsertUpdateDelete(SyncViewModel viewModel)
        {
            if (viewModel.Added.Count > 0)
                await InsertDataToDb(viewModel.TargetedTable, viewModel.Added);

            if (viewModel.Deleted.Count > 0)
                await DeleteDataFromDb(viewModel.TargetedTable, viewModel.Deleted);

            if (viewModel.Modified.Count > 0)
                await UpdateDataToDb(viewModel.TargetedTable, viewModel.Modified, viewModel.Existing);
           
        }
        public async Task InsertDataToDb(string tableName, List<Dictionary<string, object>> data)
        {
            try
            {
                foreach (var d in data)
                {
                    var column = string.Join(",", d.Select(x => x.Key));
                    var columnValues = string.Join(",", d.Select(x => $"@{x.Key}"));

                    var query = $"INSERT INTO {tableName} ({column}) VALUES ({columnValues})";
                    var result = await _dbHelper.ExecuteNonQueryWithDataAsync(query, d);
                }

                Console.WriteLine("Added employees");
                PrintDetails(data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        public async Task UpdateDataToDb(string tableName, List<Dictionary<string, object>> data, List<Dictionary<string, object>> exising)
        {
            try
            {

                foreach (var d in data)
                {
                    var key = d.FirstOrDefault().Key;
                    var keyValue = d.FirstOrDefault().Value;

                    var column = string.Join(",", d.Select(x => x.Key + "=" + $"@{x.Key}"));
                    var query = $"Update {tableName} SET {column} Where {key} = {keyValue}";
                    var result = await _dbHelper.ExecuteNonQueryWithDataAsync(query, d);
                }
                Console.WriteLine("Changes");
                PrintDetails(data, exising);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        public async Task DeleteDataFromDb(string tableName, List<Dictionary<string, object>> data)
        {
            try
            {

                string columnName = "";
                List<string> primaryKeys = new List<string>();
                foreach (var d in data)
                {
                    columnName = d.FirstOrDefault().Key;
                    primaryKeys.Add((string)d.FirstOrDefault().Value);
                }

                var query = $"DELETE FROM {tableName} WHERE {columnName} IN (" + string.Join(",", primaryKeys.Select(x => x)) + ")";
                await _dbHelper.ExecuteNonQueryAsync(query);

                Console.WriteLine("Removed employees");
                PrintDetails(data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        #region Private methods

        private void PrintDetails(List<Dictionary<string, object>> data)
        {
            foreach (var d in data)
            {
                var keys = d.Select(x => x.Key).Take(3).ToList();
                string values = "";
                foreach (var k in keys)
                {
                    values += $"{d[k]} ";
                }
                Console.WriteLine(values);
            }

        }
        private void PrintDetails(List<Dictionary<string, object>> data, List<Dictionary<string, object>> existing)
        {
            if (data.Count == existing.Count)
            {
                var keys = data.Select(x=>x.Keys).SelectMany(c => c).Distinct().ToList();

                List<string> results = new List<string>();

                foreach (Dictionary<string, object> d in existing)
                {
                    string result = "";
                    string keyValue = (string)d[keys.First()];
                    result += $"{keyValue} ";
                    var dataRow = data.FirstOrDefault(x => x[keys.First()].ToString() == keyValue);
                    foreach (var o in d)
                    {
                        if (o.Value.ToString() != dataRow[o.Key].ToString())
                        {
                            result += $" {o.Key} has changed from {o.Value} to {dataRow[o.Key]}";
                            
                        }
                    }
                    results.Add(result);
                }

                foreach (var result in results)
                {
                    Console.WriteLine(result);
                }
            }
        }

        private void CheckTableName(string table)
        {
            if (string.IsNullOrEmpty(table))
                throw new Exception("Table name must be set.");
        }

        private List<Dictionary<string, object>> ConvertDataTable(DataTable dt)
        {
            var rows = new List<Dictionary<string, object>>();
            foreach (DataRow dataRow in dt.Rows)
            {
                var row = GetItem(dataRow);
                rows.Add(row);
            }
            return rows;
        }

        private  Dictionary<string, object> GetItem(DataRow dr)
        {
            var row = new Dictionary<string, object>();
            foreach (DataColumn column in dr.Table.Columns)
            {
                row.Add(column.ColumnName, dr[column.ColumnName]);
            }
            return row;
        }

        #endregion
    }
}
