using Microsoft.Data.Sqlite;
using System.Globalization;
using WpfApp.Common.Models;
using WpfApp.Common.Repositories.Interfaces;
using WpfApp.Services;

namespace WpfApp.Common.Repositories
{
    public class WorkRepository : IWorkRepository
    {
        // INSERT文
        private const string InsertSql = """
            INSERT INTO Works
            (
                WorkGuid,
                WorkName,
                CreatedAt,
                UpdatedAt
            )
            VALUES
            (
                $WorkGuid,
                $WorkName,
                $CreatedAt,
                $UpdatedAt
            );
            """;

        // SQLパラメータ名
        private const string ParamId = "$WorkId";
        private const string ParamGuid = "$WorkGuid";
        private const string ParamName = "$WorkName";
        private const string ParamCreatedAt = "$CreatedAt";
        private const string ParamUpdatedAt = "$UpdatedAt";
        // SQLカラム名
        private const string ColumnId = "WorkId";
        private const string ColumnGuid = "WorkGuid";
        private const string ColumnName = "WorkName";
        private const string ColumnCreatedAt = "CreatedAt";
        private const string ColumnUpdatedAt = "UpdatedAt";
        // DateTime用データフォーマット
        private const string DataFormat = "yyyy-MM-dd HH:mm:ss.fff";

        private readonly DatabaseService _database;

        public WorkRepository(DatabaseService database)
        {
            _database = database;
        }

        public async Task<int> InsertAsync(WorkData work)
        {
            await using var connection = await _database.CreateConnectionAsync();

            string sql = InsertSql;
            await using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue(ParamGuid, work.WorkGuid);
            command.Parameters.AddWithValue(ParamName, work.WorkName);
            command.Parameters.AddWithValue(ParamCreatedAt, work.CreatedAt.ToString(DataFormat, CultureInfo.InvariantCulture));
            command.Parameters.AddWithValue(ParamUpdatedAt, work.UpdatedAt.ToString(DataFormat, CultureInfo.InvariantCulture));
            var ret = await command.ExecuteNonQueryAsync();
            return ret;
        }

        public async Task<int> InsertRangeAsync(List<WorkData> works)
        {
            await using var connection = await _database.CreateConnectionAsync();
            using var transaction = connection.BeginTransaction();

            string sql = InsertSql;
            await using var command = new SqliteCommand(sql, connection, transaction);
            // パラメータのみを設定
            command.Parameters.AddWithValue(ParamGuid, "");
            command.Parameters.AddWithValue(ParamName, "");
            command.Parameters.AddWithValue(ParamCreatedAt, "");
            command.Parameters.AddWithValue(ParamUpdatedAt, "");

            int cnt = 0;
            foreach (var work in works)
            {
                // パラメータに値を設定
                command.Parameters[ParamGuid].Value = work.WorkGuid;
                command.Parameters[ParamName].Value = work.WorkName;
                command.Parameters[ParamCreatedAt].Value = work.CreatedAt.ToString(DataFormat, CultureInfo.InvariantCulture);
                command.Parameters[ParamUpdatedAt].Value = work.UpdatedAt.ToString(DataFormat, CultureInfo.InvariantCulture);
                cnt += await command.ExecuteNonQueryAsync();
            }
            await transaction.CommitAsync();
            return cnt;
        }

        public async Task UpdateAsync(WorkData work)
        {
            await using var connection = await _database.CreateConnectionAsync();

            string sql = """
                UPDATE Works
                SET
                    WorkName = $WorkName,
                    UpdatedAt = $UpdatedAt
                WHERE
                    WorkId = $WorkId;
                """;

            await using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue(ParamName, work.WorkName);
            command.Parameters.AddWithValue(ParamUpdatedAt, work.UpdatedAt.ToString(DataFormat, CultureInfo.InvariantCulture));
            command.Parameters.AddWithValue(ParamId, work.WorkId);

            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int workId)
        {
            await using var connection = await _database.CreateConnectionAsync();

            string sql = """
                DELETE FROM
                    Works
                WHERE
                    WorkId = $WorkId;
                """;

            await using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue(ParamId, workId);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<WorkData?> GetByIdAsync(int workId) 
        {
            await using var connection = await _database.CreateConnectionAsync();

            string sql = """
                SELECT
                    WorkId,
                    WorkGuid,
                    WorkName,
                    CreatedAt,
                    UpdatedAt
                FROM
                    Works
                WHERE
                    WorkId = $WorkId;
                """;

            await using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue(ParamId, workId);

            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            var work = new WorkData()
            {
                WorkId = reader.GetInt32(reader.GetOrdinal(ColumnId)),
                WorkGuid = reader.GetString(reader.GetOrdinal(ColumnGuid)),
                WorkName = reader.GetString(reader.GetOrdinal(ColumnName)),
                CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal(ColumnCreatedAt))),
                UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal(ColumnUpdatedAt)))
            };
            return work;
        }

        public async Task<List<WorkData>> GetAllAsync()
        {
            var ret = new List<WorkData>();

            await using var connection = await _database.CreateConnectionAsync();

            string sql = """
                SELECT
                    WorkId,
                    WorkGuid,
                    WorkName,
                    CreatedAt,
                    UpdatedAt
                FROM
                    Works
                ORDER BY WorkId;
                """;

            await using var command = new SqliteCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var work = new WorkData()
                {
                    WorkId = reader.GetInt32(reader.GetOrdinal(ColumnId)),
                    WorkGuid = reader.GetString(reader.GetOrdinal(ColumnGuid)),
                    WorkName = reader.GetString(reader.GetOrdinal(ColumnName)),
                    CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal(ColumnCreatedAt))),
                    UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal(ColumnUpdatedAt)))
                };
                ret.Add(work);
            }
            return ret;
        }
    }
}
