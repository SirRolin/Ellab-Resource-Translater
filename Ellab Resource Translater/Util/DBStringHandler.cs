using Ellab_Resource_Translater.Enums;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Ellab_Resource_Translater.Util
{
    internal class DBStringHandler
    {
        public static DbConnection CreateDbConnection(string connectionString)
        {
            return DetectType(connectionString) switch {
                ConnType.MySql => new MySqlConnection(connectionString),
                ConnType.MSSql => new SqlConnection(connectionString),
                ConnType.PostgreSql => new NpgsqlConnection(connectionString),
                _ => throw new InvalidOperationException("Unknown or unsupported database type in connection string.")
            };
        }

        public static ConnType DetectType(string connectionString)
        {
            // In Case it's a Json Object gotten from exporting connection from VS
            connectionString = JsonExtractIfNeeded(connectionString);

            // Each Connection type have different setups
            if (connectionString.Contains("Server") && connectionString.Contains("Database") && connectionString.Contains("User ID") && connectionString.Contains("Password"))
                return ConnType.MySql;
            else if (connectionString.Contains("Data Source") || connectionString.Contains("Server") && connectionString.Contains("Initial Catalog"))
                return ConnType.MSSql;
            else if (connectionString.Contains("Host") && connectionString.Contains("Username") && connectionString.Contains("Password"))
                return ConnType.PostgreSql;
            else
                return ConnType.None;
        }

        public static string JsonExtractIfNeeded(string connectionString)
        {
            JsonSerializerSettings dontThrow = new()
            {
                Error = (s,a) => { a.ErrorContext.Handled = true; }
            };
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(connectionString, dontThrow);
            if (dict != null && dict.Count > 0)
                return dict.First().Value;

            dict = JsonConvert.DeserializeObject<Dictionary<string, string>>($"{{ {connectionString} }}", dontThrow);
            if (dict != null && dict.Count > 0)
                connectionString = dict.First().Value;

            return connectionString;
        }
    }
}
