using Microsoft.Data.Sqlite;
using System.Globalization;
using WpfApp.Common.Models;
using WpfApp.Common.Repositories.Interfaces;
using WpfApp.Services;

namespace WpfApp.Common.Repositories
{
    public class AdminPasswordRepository : IAdminPasswordRepository
    {

        // SQLパラメータ名
        private const string ParamId = "$Id";
        private const string ParamHash = "$PasswordHash";
        private const string ParamSalt = "$Salt";
        private const string ParamIterations = "$Iterations";
        private const string ParamCreatedAt = "$CreatedAt";
        private const string ParamUpdatedAt = "$UpdatedAt";
        // SQLカラム名
        private const string ColumnId = "Id";
        private const string ColumnHash = "PasswordHash";
        private const string ColumnSalt = "Salt";
        private const string ColumnIterations = "Iterations";
        private const string ColumnCreatedAt = "CreatedAt";
        private const string ColumnUpdatedAt = "UpdatedAt";
        // DateTime用データフォーマット
        private const string DataFormat = "yyyy-MM-dd HH:mm:ss.fff";

        private readonly DatabaseService _database;

        public AdminPasswordRepository(DatabaseService database)
        {
            _database = database;
        }

        public async Task<bool> ExistsAsync()
        {
            await using var connection = await _database.CreateConnectionAsync();

            string sql = """
                SELECT 1
                FROM AdminPassword
                WHERE Id = 1;
                """;
            await using var command = new SqliteCommand(sql, connection);
            var ret = await command.ExecuteScalarAsync();
            return Convert.ToInt32(ret) == 1;
        }

        public async Task<int> InsertAsync(PasswordData password)
        {
            await using var connection = await _database.CreateConnectionAsync();

            string sql = """
                INSERT INTO AdminPassword
                (
                    PasswordHash,
                    Salt,
                    Iterations,
                    CreatedAt,
                    UpdatedAt
                )
                VALUES
                (
                    $PasswordHash,
                    $Salt,
                    $Iterations,
                    $CreatedAt,
                    $UpdatedAt
                );
                """;

            await using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue(ParamHash, password.PasswordHash);
            command.Parameters.AddWithValue(ParamSalt, password.Salt);
            command.Parameters.AddWithValue(ParamIterations, password.Iterations);
            command.Parameters.AddWithValue(ParamCreatedAt, password.CreatedAt.ToString(DataFormat, CultureInfo.InvariantCulture));
            command.Parameters.AddWithValue(ParamUpdatedAt, password.UpdatedAt.ToString(DataFormat, CultureInfo.InvariantCulture));
            var ret = await command.ExecuteNonQueryAsync();
            return ret;
        }

        public async Task<int> UpdateAsync(PasswordData password)
        {
            await using var connection = await _database.CreateConnectionAsync();

            string sql = """
                UPDATE AdminPassword
                SET
                    PasswordHash = $PasswordHash,
                    Salt = $Salt,
                    Iterations = $Iterations,
                    UpdatedAt = $UpdatedAt
                WHERE
                    Id = 1;
                """;

            await using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue(ParamHash, password.PasswordHash);
            command.Parameters.AddWithValue(ParamSalt, password.Salt);
            command.Parameters.AddWithValue(ParamIterations, password.Iterations);
            command.Parameters.AddWithValue(ParamUpdatedAt, password.UpdatedAt.ToString(DataFormat, CultureInfo.InvariantCulture));
            var ret = await command.ExecuteNonQueryAsync();
            return ret;
        }

        public async Task<int> DeleteAsync()
        {
            await using var connection = await _database.CreateConnectionAsync();

            string sql = """
                DELETE FROM
                    AdminPassword
                WHERE
                    Id = 1;
                """;

            await using var command = new SqliteCommand(sql, connection);
            var ret = await command.ExecuteNonQueryAsync();
            return ret;
        }

        public async Task<PasswordData?> GetAsync()
        {
            await using var connection = await _database.CreateConnectionAsync();

            string sql = """
                SELECT
                    Id,
                    PasswordHash,
                    Salt,
                    Iterations,
                    CreatedAt,
                    UpdatedAt
                FROM
                    AdminPassword
                WHERE
                    Id = 1;
                """;

            await using var command = new SqliteCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            var password = new PasswordData()
            {
                Id = reader.GetInt32(reader.GetOrdinal(ColumnId)),
                PasswordHash = reader.GetString(reader.GetOrdinal(ColumnHash)),
                Salt = reader.GetString(reader.GetOrdinal(ColumnSalt)),
                Iterations = reader.GetInt32(reader.GetOrdinal(ColumnIterations)),
                CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal(ColumnCreatedAt))),
                UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal(ColumnUpdatedAt)))
            };
            return password;
        }
    }
}
