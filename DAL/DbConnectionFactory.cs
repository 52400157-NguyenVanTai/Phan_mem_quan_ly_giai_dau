using System;
using System.Configuration;
using System.Data.SqlClient;

namespace DAL
{
    public static class DbConnectionFactory
    {
        private const string ConnectionStringName = "MyDbConn";

        public static string ConnectionString
        {
            get
            {
                ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[ConnectionStringName];

                if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
                {
                    throw new InvalidOperationException("Không tìm thấy connection string MyDbConn trong Web.config.");
                }

                return settings.ConnectionString;
            }
        }

        public static SqlConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static bool CanConnect()
        {
            using (SqlConnection connection = CreateConnection())
            {
                connection.Open();
                return connection.State == System.Data.ConnectionState.Open;
            }
        }
    }
}
