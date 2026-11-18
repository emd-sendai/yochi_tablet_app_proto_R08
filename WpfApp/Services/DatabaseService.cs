using Microsoft.Data.Sqlite;
using System.Diagnostics;
using WpfApp.Interfaces;

namespace WpfApp.Services
{
    public class DatabaseService
    {
        private readonly IAppPathService _path;
        private readonly string _connectionString;

        public DatabaseService()
        {
            _path = new AppPathService();

            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = _path.DatabasePath,
                Mode = SqliteOpenMode.ReadWriteCreate
            };

            _connectionString = builder.ToString();
        }

        public async Task<SqliteConnection> CreateConnectionAsync()
        {
            var connection = new SqliteConnection(_connectionString);

            try
            {
                await connection.OpenAsync();

                // 外部キー制約の有効化
                using var command = connection.CreateCommand();
                command.CommandText = "PRAGMA foreign_keys = ON;";

                await command.ExecuteNonQueryAsync();

                return connection;
            }
            catch (Exception ex) 
            {
                await connection.DisposeAsync();
                Debug.WriteLine(ex);
                throw;
            }
        }
    }
}
