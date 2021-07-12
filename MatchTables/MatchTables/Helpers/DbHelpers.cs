using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace MatchTables.Helpers
{
    public  class DBHelper : IDbHelper
    {
        public async Task<DataTable> ExecuteReaderAsync(string query)
        {
            try
            {
                DataTable dt = new DataTable();
                using var connection = await GetConnection();
                SqlCommand command = new SqlCommand();
                SqlDataAdapter da;
                try
                {
                    command.Connection = connection;
                    command.CommandText = query;
                    da = new SqlDataAdapter(command);
                    da.Fill(dt);
                }
                finally
                {
                    if (connection != null)
                        await connection.CloseAsync();
                }
                return dt;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<DataTable> ExecuteReaderDatatableAsync(string query)
        {
            try
            {
                using var sqlCon = await GetConnection();
                var sqlCmd = sqlCon.CreateCommand();
                sqlCmd.CommandText = query;
                sqlCmd.CommandType = CommandType.Text;
                var sqlDr = await sqlCmd.ExecuteReaderAsync();
                var dataTable = sqlDr.GetSchemaTable();
                return dataTable;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<DataTable> ExecuteReaderDatatableAsync(string[] restrictions)
        {
            try
            {
                DbConnection connection = await GetConnection();
                List<string> result = new List<string>();
                DataTable table = connection.GetSchema("IndexColumns", restrictions);
                return table;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<int> ExecuteNonQueryWithDataAsync(string query, Dictionary<string, object> data)
        {
            try
            {
                await using SqlConnection conn = await GetConnection();
                SqlCommand cmd = new SqlCommand(query)
                { CommandType = CommandType.Text, Connection = conn };

                foreach (var item in data)
                {
                    cmd.Parameters.AddWithValue($"@{item.Key}", item.Value);

                }
                return await cmd.ExecuteNonQueryAsync();
                
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<int> ExecuteNonQueryAsync(string query)
        {
            try
            {
                await using SqlConnection connection = await GetConnection();
                SqlCommand commandDelete = new SqlCommand(query, connection);
                return await commandDelete.ExecuteNonQueryAsync();

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<SqlConnection> GetConnection()
        {
            SqlConnection connection = new SqlConnection(ApplicationConfigs.ConnectionString());
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();
            return connection;
        }
    }
}
