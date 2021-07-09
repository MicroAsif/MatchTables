using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MatchTables.Interfaces;
using MatchTables.Models;
using MatchTables.ViewModel;
using Microsoft.Extensions.Logging;

namespace MatchTables.Services
{
    public class DataAccessService : IDataAccessService
    {
        private readonly ILogger<DataAccessService> _logger;

        public DataAccessService(ILogger<DataAccessService> logger)
        {
            _logger = logger;
        }
        public IEnumerable<string> GetColumnNames(string tableName)
        {
            var result = new List<string>();
            using var sqlCon = GetConnection();
            var sqlCmd = sqlCon.CreateCommand();
            sqlCmd.CommandText = "select * from " + tableName + " where 1=0"; // No data wanted, only schema  
            sqlCmd.CommandType = CommandType.Text;
            var sqlDr = sqlCmd.ExecuteReader();
            var dataTable = sqlDr.GetSchemaTable();
            foreach (DataRow row in dataTable.Rows) result.Add(row.Field<string>("ColumnName"));
            return result;
        }
        public List<string> GetPrimaryKeyColumns(string tableName)
        {
            DbConnection connection = GetConnection();
            List<string> result = new List<string>();
            
            string[] restrictions = new string[] { null, null, tableName };
            DataTable table = connection.GetSchema("IndexColumns", restrictions);

            if (string.IsNullOrEmpty(tableName))
                throw new Exception("Table name must be set.");

            foreach (DataRow row in table.Rows)
            {
                result.Add(row["column_name"].ToString());
            }

            return result;
        }
        public async Task<List<Customer>> GetTableValues(string tableName)
        {
            var customerData = new List<Customer>();

            await using SqlConnection conn = GetConnection();
            string query = $"SELECT * FROM {tableName}";
            SqlCommand cmd = new SqlCommand(query, conn);

            SqlDataReader dr = await cmd.ExecuteReaderAsync();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    var customer = new Customer
                    {
                        SocialSecurityNumber = dr.GetString(0),
                        FirstName = dr.GetString(1),
                        LastName = dr.GetString(2),
                        Department = dr.GetString(3)
                    };
                    customerData.Add(customer);
                }
            }
            else
            {
                Console.WriteLine("No data found.");
            }
            await dr.CloseAsync();
            await conn.CloseAsync();
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
                await using SqlConnection conn = GetConnection();
                SqlCommand cmd = new SqlCommand($"INSERT INTO {tableName} (SocialSecurityNumber, FirstName, LastName, Department)" +
                                                "VALUES (@SocialSecurityNumber, @FirstName, @LastName, @Department)")
                    { CommandType = CommandType.Text, Connection = conn };
                cmd.Parameters.Add("@SocialSecurityNumber", SqlDbType.VarChar);
                cmd.Parameters.Add("@FirstName", SqlDbType.VarChar);
                cmd.Parameters.Add("@LastName", SqlDbType.VarChar);
                cmd.Parameters.Add("@Department", SqlDbType.VarChar);

                foreach (var item in data)
                {
                    cmd.Parameters[0].Value = item.SocialSecurityNumber;
                    cmd.Parameters[1].Value = item.FirstName;
                    cmd.Parameters[2].Value = item.LastName;
                    cmd.Parameters[3].Value = item.Department;
                    await cmd.ExecuteNonQueryAsync();
                }
                conn.Close();
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
                await using SqlConnection conn = GetConnection();
                SqlCommand cmd = new SqlCommand($"Update {tableName} SET FirstName = @FirstName, LastName = @LastName, Department = @Department Where SocialSecurityNumber = @SocialSecurityNumber", conn);
                cmd.Parameters.Add("@SocialSecurityNumber", SqlDbType.VarChar);
                cmd.Parameters.Add("@FirstName", SqlDbType.VarChar);
                cmd.Parameters.Add("@LastName", SqlDbType.VarChar);
                cmd.Parameters.Add("@Department", SqlDbType.VarChar);

                foreach (var item in data)
                {
                    cmd.Parameters[0].Value = item.SocialSecurityNumber;
                    cmd.Parameters[1].Value = item.FirstName;
                    cmd.Parameters[2].Value = item.LastName;
                    cmd.Parameters[3].Value = item.Department;
                    await cmd.ExecuteNonQueryAsync();
                }
                conn.Close();
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
                await using SqlConnection connection = GetConnection();
                var query = $"DELETE FROM {tableName} WHERE SocialSecurityNumber IN (" + string.Join(",", data.Select(x=>x.SocialSecurityNumber)) + ")";
                SqlCommand commandDelete = new SqlCommand(query, connection);
                await commandDelete.ExecuteNonQueryAsync();
                await connection.CloseAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        #region Private methods
        private SqlConnection GetConnection()
        {
            SqlConnection connection = new SqlConnection(ApplicationConfigs.ConnectionString());
            if (connection.State != ConnectionState.Open)
                connection.Open();
            return connection;
        }


        #endregion
    }
}
