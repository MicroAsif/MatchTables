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
using MatchTables.Models;
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
        public async Task<List<Customer>> GetTableValues(string tableName)
        {
            var customerData = new List<Customer>();
            string query = $"SELECT * FROM {tableName}";

            var dt= await _dbHelper.ExecuteReaderAsync(query);
            customerData = ConvertDataTable<Customer>(dt);
            
            return customerData;
        }

        public async Task InsertUpdateDelete(SyncViewModel viewModel)
        {
            if (viewModel.Added.Count > 0)
                await InsertDataToDb(viewModel.TargetedTable, viewModel.Added);

            if (viewModel.Deleted.Count > 0)
                await DeleteDataFromDb(viewModel.TargetedTable, viewModel.Deleted);

            if (viewModel.Modified.Count > 0)
                await UpdateDataToDb(viewModel.TargetedTable, viewModel.Modified);
           
        }

        public async Task InsertDataToDb(string tableName, List<Customer> data)
        {
            try
            {
                var query = $"INSERT INTO {tableName} (SocialSecurityNumber, FirstName, LastName, Department)" +
                                                 "VALUES (@SocialSecurityNumber, @FirstName, @LastName, @Department)";

                var result = await _dbHelper.ExecuteNonQueryWithDataAsync(query, data);

                Console.WriteLine("Added employees");
                PrintDetails(data);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        public async Task UpdateDataToDb(string tableName, List<Customer> data)
        {
            try
            {
                var query = $"Update {tableName} SET FirstName = @FirstName, LastName = @LastName, Department = @Department Where SocialSecurityNumber = @SocialSecurityNumber";
                var result = await _dbHelper.ExecuteNonQueryWithDataAsync(query, data);
                Console.WriteLine("Changes");
                PrintDetails(data);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        public async Task DeleteDataFromDb(string tableName, List<Customer> data)
        {
            try
            {
                var query = $"DELETE FROM {tableName} WHERE SocialSecurityNumber IN (" + string.Join(",", data.Select(x => x.SocialSecurityNumber)) + ")";
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
        
        private void PrintDetails(List<Customer> data)
        {
            foreach (var d in data)
                Console.WriteLine($"{d.SocialSecurityNumber} ({d.FirstName} {d.LastName})");
        }

        private void CheckTableName(string table)
        {
            if (string.IsNullOrEmpty(table))
                throw new Exception("Table name must be set.");
        }

        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }

        #endregion
    }
}
