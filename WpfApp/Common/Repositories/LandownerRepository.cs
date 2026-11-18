using Microsoft.Data.Sqlite;
using System.Globalization;
using WpfApp.Common.Models;
using WpfApp.Common.Repositories.Interfaces;
using WpfApp.Services;

namespace WpfApp.Common.Repositories
{
    public class LandownerRepository : ILandownerRepository
    {
        // INSERT文
        private const string InsertSql = """
            INSERT INTO Landowners
            (
                LandownerGuid,
                LandownerName,
                LandownerKana,
                CreatedAt,
                UpdatedAt
            )
            VALUES
            (
                $LandownerGuid,
                $LandownerName,
                $LandownerKana,
                $CreatedAt,
                $UpdatedAt
            );
            """;

        // SQLパラメータ名
        private const string ParamId = "$LandownerId";
        private const string ParamGuid = "$LandownerGuid";
        private const string ParamName = "$LandownerName";
        private const string ParamKana = "$LandownerKana";
        private const string ParamCreatedAt = "$CreatedAt";
        private const string ParamUpdatedAt = "$UpdatedAt";
        // SQLカラム名
        private const string ColumnId = "LandownerId";
        private const string ColumnGuid = "LandownerGuid";
        private const string ColumnName = "LandownerName";
        private const string ColumnKana = "LandownerKana";
        private const string ColumnCreatedAt = "CreatedAt";
        private const string ColumnUpdatedAt = "UpdatedAt";
        // DateTime用データフォーマット
        private const string DataFormat = "yyyy-MM-dd HH:mm:ss.fff";

        private readonly DatabaseService _database;

        public LandownerRepository(DatabaseService database)
        {
            _database = database;
        }

        public async Task<int> InsertAsync(LandownerData landowner)
        {
            await using var connection = await _database.CreateConnectionAsync();

            string sql = InsertSql;
            await using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue(ParamGuid, landowner.LandownerGuid);
            command.Parameters.AddWithValue(ParamName, landowner.LandownerName);
            command.Parameters.AddWithValue(ParamKana, landowner.LandownerKana);
            command.Parameters.AddWithValue(ParamCreatedAt, landowner.CreatedAt.ToString(DataFormat, CultureInfo.InvariantCulture));
            command.Parameters.AddWithValue(ParamUpdatedAt, landowner.UpdatedAt.ToString(DataFormat, CultureInfo.InvariantCulture));
            var ret = await command.ExecuteNonQueryAsync();
            return ret;
        }

        public async Task<int> InsertRangeAsync(List<LandownerData> landowners)
        {
            await using var connection = await _database.CreateConnectionAsync();
            using var transaction = connection.BeginTransaction();

            string sql = InsertSql;
            await using var command = new SqliteCommand(sql, connection, transaction);
            // パラメータのみを設定
            command.Parameters.AddWithValue(ParamGuid, "");
            command.Parameters.AddWithValue(ParamName, "");
            command.Parameters.AddWithValue(ParamKana, "");
            command.Parameters.AddWithValue(ParamCreatedAt, "");
            command.Parameters.AddWithValue(ParamUpdatedAt, "");

            int cnt = 0;
            foreach (var landowner in landowners)
            {
                // パラメータに値を設定
                command.Parameters[ParamGuid].Value = landowner.LandownerGuid;
                command.Parameters[ParamName].Value = landowner.LandownerName;
                command.Parameters[ParamKana].Value = landowner.LandownerKana;
                command.Parameters[ParamCreatedAt].Value = landowner.CreatedAt.ToString(DataFormat, CultureInfo.InvariantCulture);
                command.Parameters[ParamUpdatedAt].Value = landowner.UpdatedAt.ToString(DataFormat, CultureInfo.InvariantCulture);
                cnt += await command.ExecuteNonQueryAsync();
            }
            await transaction.CommitAsync();
            return cnt;
        }

        public async Task UpdateAsync(LandownerData landowner)
        {
            await using var connection = await _database.CreateConnectionAsync();

            string sql = """
                UPDATE Landowners
                SET
                    LandownerName = $LandownerName,
                    LandownerKana = $LandownerKana,
                    UpdatedAt = $UpdatedAt
                WHERE
                    LandownerId = $LandownerId;
                """;

            await using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue(ParamName, landowner.LandownerName);
            command.Parameters.AddWithValue(ParamKana, landowner.LandownerKana);
            command.Parameters.AddWithValue(ParamUpdatedAt, landowner.UpdatedAt.ToString(DataFormat, CultureInfo.InvariantCulture));
            command.Parameters.AddWithValue(ParamId, landowner.LandownerId);

            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int landownerId)
        {
            await using var connection = await _database.CreateConnectionAsync();

            string sql = """
                DELETE FROM
                    Landowners
                WHERE
                    LandownerId = $LandownerId;
                """;

            await using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue(ParamId, landownerId);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<LandownerData?> GetByIdAsync(int landownerId)
        {
            await using var connection = await _database.CreateConnectionAsync();

            string sql = """
                SELECT
                    LandownerId,
                    LandownerGuid,
                    LandownerName,
                    LandownerKana,
                    CreatedAt,
                    UpdatedAt
                FROM
                    Landowners
                WHERE
                    LandownerId = $LandownerId;
                """;

            await using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue(ParamId, landownerId);

            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            var ret = new LandownerData()
            {
                LandownerId = reader.GetInt32(reader.GetOrdinal(ColumnId)),
                LandownerGuid = reader.GetString(reader.GetOrdinal(ColumnGuid)),
                LandownerName = reader.GetString(reader.GetOrdinal(ColumnName)),
                LandownerKana = reader.GetString(reader.GetOrdinal(ColumnKana)),
                CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal(ColumnCreatedAt))),
                UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal(ColumnUpdatedAt)))
            };
            return ret;
        }

        public async Task<List<LandownerData>> GetAllAsync()
        {
            var ret = new List<LandownerData>();

            await using var connection = await _database.CreateConnectionAsync();

            string sql = """
                SELECT
                    LandownerId,
                    LandownerGuid,
                    LandownerName,
                    LandownerKana,
                    CreatedAt,
                    UpdatedAt
                FROM
                    Landowners
                ORDER BY LandownerId;
                """;

            await using var command = new SqliteCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var landowner = new LandownerData()
                {
                    LandownerId = reader.GetInt32(reader.GetOrdinal(ColumnId)),
                    LandownerGuid = reader.GetString(reader.GetOrdinal(ColumnGuid)),
                    LandownerName = reader.GetString(reader.GetOrdinal(ColumnName)),
                    LandownerKana = reader.GetString(reader.GetOrdinal(ColumnKana)),
                    CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal(ColumnCreatedAt))),
                    UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal(ColumnUpdatedAt)))
                };
                ret.Add(landowner);
            }

            return ret;
        }
    }
}
