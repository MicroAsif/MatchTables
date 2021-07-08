using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MatchTables.Interfaces;
using MatchTables.Models;

namespace MatchTables.Services
{
    public class DataAccessService : IDataAccessService
    {
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

            using SqlConnection conn = GetConnection();
            string query = $"SELECT * FROM {tableName}";
            SqlCommand cmd = new SqlCommand(query, conn);

            SqlDataReader dr = await cmd.ExecuteReaderAsync();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    var customer = new Customer
                    {
                        SocialSecurityNumber = dr.GetInt32(0),
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
            dr.Close();
            conn.Close();
            return customerData;
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
