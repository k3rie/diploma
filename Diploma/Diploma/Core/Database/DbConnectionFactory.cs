using System.Configuration;
using System.Data;
using Npgsql;

namespace Diploma.Core.Database
{
    public static class DbConnectionFactory
    {
        private static readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public static IDbConnection Create()
        {
            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            return conn;
        }
    }
}