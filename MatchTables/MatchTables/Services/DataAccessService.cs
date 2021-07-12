using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
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

            //if (viewModel.Modified.Count > 0)
            //    await UpdateDataToDb(viewModel.TargetedTable, viewModel.Modified, viewModel.Exising);
           
        }

        public async Task InsertDataToDb(string tableName, List<Dictionary<string, object>> data)
        {
            try
            {
                //var query = $"INSERT INTO {tableName} (SocialSecurityNumber, FirstName, LastName, Department)" +
                //                                 "VALUES (@SocialSecurityNumber, @FirstName, @LastName, @Department)";

                foreach(var d in data)
                {
                    var column = string.Join(",", d.Select(x => x.Key));
                    var columnValues = string.Join(",", d.Select(x => $"@{x.Key}"));

                    var query = $"INSERT INTO {tableName} ({column}) VALUES ({columnValues})";
                    var result = await _dbHelper.ExecuteNonQueryWithDataAsync(query, d);
                }

                Console.WriteLine("Added employees");
                //PrintDetails(data);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        //public async Task UpdateDataToDb(string tableName, List<Customer> data, List<Customer> exising)
        //{
        //    try
        //    {
        //        var query = $"Update {tableName} SET FirstName = @FirstName, LastName = @LastName, Department = @Department Where SocialSecurityNumber = @SocialSecurityNumber";
        //        var result = await _dbHelper.ExecuteNonQueryWithDataAsync(query, data);
        //        Console.WriteLine("Changes");
        //        PrintDetails(data, exising);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //        throw;
        //    }
        //}
        public async Task DeleteDataFromDb(string tableName, List<Dictionary<string, object>> data)
        {
            try
            {

                string columnName =""; 
                List<string> primaryKeys = new List<string>(); 
                foreach(var d in data)
                {
                    columnName = d.FirstOrDefault().Key;
                    primaryKeys.Add((string)d.FirstOrDefault().Value);
                }

                var query = $"DELETE FROM {tableName} WHERE {columnName} IN (" + string.Join(",", primaryKeys.Select(x => x)) + ")";
                await _dbHelper.ExecuteNonQueryAsync(query);

                Console.WriteLine("Removed employees");
                //PrintDetails(data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        #region Private methods
        
        //private void PrintDetails(List<Customer> data)
        //{
        //    foreach (var d in data)
        //        Console.WriteLine($"{d.SocialSecurityNumber} ({d.FirstName} {d.LastName})");
        //}
        //private void PrintDetails(List<Customer> data, List<Customer> existing)
        //{
        //    if (data.Count == existing.Count)
        //    {
        //        for (int i = 0; i < data.Count; i++) 
        //        {
        //            if (data[i].FirstName != existing[i].FirstName) 
        //                Console.WriteLine($"{data[i].SocialSecurityNumber}  Firstname has changed from {existing[i].FirstName} to  {data[i].FirstName}");
                    
        //            if (data[i].LastName != existing[i].LastName)
        //                Console.WriteLine($"{data[i].SocialSecurityNumber}  Lastname has changed from {existing[i].LastName} to  {data[i].LastName}");

        //            if (data[i].Department != existing[i].Department)
        //                Console.WriteLine($"{data[i].SocialSecurityNumber}  Department has changed from {existing[i].Department} to  {data[i].Department}");


        //        }    
        //    }
     
        //}

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
