using Microsoft.Data.Sqlite;
using System.Globalization;
using WpfApp.Common.Models;
using WpfApp.Common.Repositories.Interfaces;
using WpfApp.Services;

namespace WpfApp.Common.Repositories
{
    public class ButtonRepository : IButtonRepository
    {
        // SQLパラメータ名
        private const string ParamType = "$Type";
        private const string ParamId = "$Id";
        private const string ParamContent = "$Content";
        private const string ParamCreatedAt = "$CreatedAt";
        private const string ParamUpdatedAt = "$UpdatedAt";
        // SQLカラム名
        private const string ColumnType = "Type";
        private const string ColumnId = "Id";
        private const string ColumnContent = "Content";
        private const string ColumnCreatedAt = "CreatedAt";
        private const string ColumnUpdatedAt = "UpdatedAt";
        // DateTime用データフォーマット
        private const string DataFormat = "yyyy-MM-dd HH:mm:ss.fff";

        private readonly DatabaseService _database;

        public ButtonRepository(DatabaseService database)
        {
            _database = database;
        }

        public async Task<int> CountAsync()
        {
            await using var connection = await _database.CreateConnectionAsync();

            string sql = """
                SELECT COUNT(*)
                FROM
                    Buttons;
                """;

            await using var command = new SqliteCommand(sql, connection);
            var ret = await command.ExecuteScalarAsync();
            return Convert.ToInt32(ret);
        }

        public async Task<int> InsertRangeAsync(List<ButtonData> buttons)
        {
            await using var connection = await _database.CreateConnectionAsync();
            using var transaction = connection.BeginTransaction();

            string sql = """
                INSERT OR IGNORE INTO Buttons
                (
                    Type,
                    Id,
                    Content,
                    CreatedAt,
                    UpdatedAt
                )
                VALUES
                (
                    $Type,
                    $Id,
                    $Content,
                    $CreatedAt,
                    $UpdatedAt
                );
                """;

            await using var command = new SqliteCommand(sql, connection, transaction);
            // パラメータのみを設定
            command.Parameters.AddWithValue(ParamType, "");
            command.Parameters.AddWithValue(ParamId, "");
            command.Parameters.AddWithValue(ParamContent, "");
            command.Parameters.AddWithValue(ParamCreatedAt, "");
            command.Parameters.AddWithValue(ParamUpdatedAt, "");

            int cnt = 0;
            foreach (var button in buttons)
            {
                command.Parameters[ParamType].Value = button.Type;
                command.Parameters[ParamId].Value = button.Id;
                command.Parameters[ParamContent].Value = button.Content;
                command.Parameters[ParamCreatedAt].Value = button.CreatedAt.ToString(DataFormat, CultureInfo.InvariantCulture);
                command.Parameters[ParamUpdatedAt].Value = button.UpdatedAt.ToString(DataFormat, CultureInfo.InvariantCulture);
                cnt += await command.ExecuteNonQueryAsync();
            }
            await transaction.CommitAsync();
            return cnt;
        }

        public async Task<int> UpdateAsync(ButtonData button)
        {
            await using var connection = await _database.CreateConnectionAsync();

            string sql = """
                UPDATE Buttons
                SET
                    Content = $Content,
                    UpdatedAt = $UpdatedAt
                WHERE
                    Type = $Type
                    AND Id = $Id;
                """;

            await using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue(ParamContent, button.Content);
            command.Parameters.AddWithValue(ParamUpdatedAt, button.UpdatedAt.ToString(DataFormat, CultureInfo.InvariantCulture));
            command.Parameters.AddWithValue(ParamType, button.Type);
            command.Parameters.AddWithValue(ParamId, button.Id);
            var ret = await command.ExecuteNonQueryAsync();
            return ret;
        }

        public async Task<List<ButtonData>> GetButtonsAsync(int type)
        {
            var ret = new List<ButtonData>();

            await using var connection = await _database.CreateConnectionAsync();

            string sql = """
                SELECT
                    Type,
                    Id,
                    Content,
                    CreatedAt,
                    UpdatedAt
                FROM
                    Buttons
                WHERE
                    Type = $Type
                ORDER BY Id;
                """;

            await using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue(ParamType, type);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var button = new ButtonData()
                {
                    Type = reader.GetInt32(reader.GetOrdinal(ColumnType)),
                    Id = reader.GetInt32(reader.GetOrdinal(ColumnId)),
                    Content = reader.GetString(reader.GetOrdinal(ColumnContent)),
                    CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal(ColumnCreatedAt))),
                    UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal(ColumnUpdatedAt)))
                };
                ret.Add(button);
            }
            return ret;
        }
    }
}
