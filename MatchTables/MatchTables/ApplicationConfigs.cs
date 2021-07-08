using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace MatchTables
{
    public class ApplicationConfigs
    {
        public static string ConnectionString()
        {
            return ReadSetting(AppConst.DbKeyName);
        }

        static string ReadSetting(string key)
        {
            try
            {
                var builder = new ConfigurationBuilder().AddNewtonsoftJsonFile("appsetting.json", optional: false);
                var configuration = builder.Build();
                string str = configuration.GetConnectionString("DefaultConnection");
                return str;
            }
            catch (Exception ex)
            {
                return "Error reading app settings";
            }
        }
    }
}
