using Microsoft.Data.Sqlite;
using System.Globalization;
using WpfApp.Common.Models;
using WpfApp.Common.Repositories.Interfaces;
using WpfApp.Services;

namespace WpfApp.Common.Repositories
{
    public class WorkLandownerRepository : IWorkLandownerRepository
    {
        // INSERT文
        private const string InsertSql = """
            INSERT INTO WorkLandowners
            (
                WorkLandownerGuid,
                WorkId,
                LandownerId,
                CreatedAt,
                UpdatedAt
            )
            VALUES
            (
                $WorkLandownerGuid,
                $WorkId,
                $LandownerId,
                $CreatedAt,
                $UpdatedAt
            );
            """;

        // SQLパラメータ名
        private const string ParamGuid = "$WorkLandownerGuid";
        private const string ParamWorkId = "$WorkId";
        private const string ParamLandownerId = "$LandownerId";
        private const string ParamCreatedAt = "$CreatedAt";
        private const string ParamUpdatedAt = "$UpdatedAt";
        // SQLカラム名
        private const string ColumnGuid = "WorkLandownerGuid";
        private const string ColumnLandownerId = "LandownerId";
        private const string ColumnWorkId = "WorkId";
        private const string ColumnCreatedAt = "CreatedAt";
        private const string ColumnUpdatedAt = "UpdatedAt";
        private const string ColumnLandownerGuid = "LandownerGuid";
        private const string ColumnLandownerName = "LandownerName";
        private const string ColumnLandownerKana = "LandownerKana";
        // DateTime用データフォーマット
        private const string DataFormat = "yyyy-MM-dd HH:mm:ss.fff";

        private readonly DatabaseService _database;

        public WorkLandownerRepository(DatabaseService database)
        {
            _database = database;
        }

        public async Task<int> InsertAsync(WorkLandownerData workLandowner)
        {
            await using var connection = await _database.CreateConnectionAsync();

            string sql = InsertSql;
            await using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue(ParamGuid, workLandowner.WorkLandownerGuid);
            command.Parameters.AddWithValue(ParamWorkId, workLandowner.WorkId);
            command.Parameters.AddWithValue(ParamLandownerId, workLandowner.LandownerId);
            command.Parameters.AddWithValue(ParamCreatedAt, workLandowner.CreatedAt.ToString(DataFormat, CultureInfo.InvariantCulture));
            command.Parameters.AddWithValue(ParamUpdatedAt, workLandowner.UpdatedAt.ToString(DataFormat, CultureInfo.InvariantCulture));
            var ret = await command.ExecuteNonQueryAsync();
            return ret;
        }

        public async Task<int> InsertRangeAsync(List<WorkLandownerData> workLandowners)
        {
            await using var connection = await _database.CreateConnectionAsync();
            using var transaction = connection.BeginTransaction();

            string sql = InsertSql;
            await using var command = new SqliteCommand(sql, connection, transaction);
            // パラメータのみを設定
            command.Parameters.AddWithValue(ParamGuid, "");
            command.Parameters.AddWithValue(ParamWorkId, "");
            command.Parameters.AddWithValue(ParamLandownerId, "");
            command.Parameters.AddWithValue(ParamCreatedAt, "");
            command.Parameters.AddWithValue(ParamUpdatedAt, "");

            int cnt = 0;
            foreach (var workLandowner in workLandowners)
            {
                // パラメータに値を設定
                command.Parameters[ParamGuid].Value = workLandowner.WorkLandownerGuid;
                command.Parameters[ParamWorkId].Value = workLandowner.WorkId;
                command.Parameters[ParamLandownerId].Value = workLandowner.LandownerId;
                command.Parameters[ParamCreatedAt].Value = workLandowner.CreatedAt.ToString(DataFormat, CultureInfo.InvariantCulture);
                command.Parameters[ParamUpdatedAt].Value = workLandowner.UpdatedAt.ToString(DataFormat, CultureInfo.InvariantCulture);
                cnt += await command.ExecuteNonQueryAsync();
            }
            await transaction.CommitAsync();
            return cnt;
        }

        public async Task<int> RemoveLandownerAsync(int workId, int landownerId)
        {
            await using var connection = await _database.CreateConnectionAsync();

            string sql = """
                DELETE FROM
                    WorkLandowners
                WHERE
                    WorkId = $WorkId
                    AND LandownerId = $LandownerId;
                """;

            await using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue(ParamWorkId, workId);
            command.Parameters.AddWithValue(ParamLandownerId, landownerId);
            var ret = await command.ExecuteNonQueryAsync();
            return ret;
        }

        public async Task<List<int>> GetLandownerIdsByWorkIdAsync(int workId)
        {
            var landownerIds = new List<int>();

            await using var connection = await _database.CreateConnectionAsync();

            string sql = """
                SELECT 
                    LandownerId
                FROM
                    WorkLandowners
                WHERE
                    WorkId = $WorkId
                ORDER BY LandownerId;
                """;

            await using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue(ParamWorkId, workId);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                landownerIds.Add(reader.GetInt32(reader.GetOrdinal(ColumnLandownerId)));
            }
            return landownerIds;
        }

        public async Task<List<WorkLandownerData>> GetWorkLandownersByLandownerId(int landownerId)
        {
            var ret = new List<WorkLandownerData>();

            await using var connection = await _database.CreateConnectionAsync();

            string sql = """
                SELECT 
                    WorkId,
                    LandownerId
                FROM
                    WorkLandowners
                WHERE
                    LandownerId = $LandownerId
                ORDER BY WorkId;
                """;

            await using var command = new SqliteCommand( sql, connection);
            command.Parameters.AddWithValue(ParamLandownerId, landownerId);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var workLandowner = new WorkLandownerData()
                {
                    WorkId = reader.GetInt32(reader.GetOrdinal(ColumnWorkId)),
                    LandownerId = reader.GetInt32(reader.GetOrdinal(ColumnLandownerId))
                };
                ret.Add(workLandowner);
            }
            return ret;
        }

        public async Task<List<LandownerData>> GetLandownersByWorkIdAsync(int workId)
        {
            var ret = new List<LandownerData>();

            await using var connection = await _database.CreateConnectionAsync();

            string sql = """
                SELECT 
                    L.LandownerId,
                    L.LandownerGuid,
                    L.LandownerName,
                    L.LandownerKana,
                    L.CreatedAt,
                    L.UpdatedAt
                FROM
                    WorkLandowners AS WL
                INNER JOIN Landowners AS L
                    ON WL.LandownerId = L.LandownerId
                WHERE
                    WL.WorkId = $WorkId
                ORDER BY L.LandownerId;
                """;

            await using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue(ParamWorkId, workId);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var landowner = new LandownerData()
                {
                    LandownerId = reader.GetInt32(reader.GetOrdinal(ColumnLandownerId)),
                    LandownerGuid = reader.GetString(reader.GetOrdinal(ColumnLandownerGuid)),
                    LandownerName = reader.GetString(reader.GetOrdinal(ColumnLandownerName)),
                    LandownerKana = reader.GetString(reader.GetOrdinal(ColumnLandownerKana)),
                    CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal(ColumnCreatedAt))),
                    UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal(ColumnUpdatedAt)))
                };
                ret.Add(landowner);
            }
            return ret;
        }
    }
}
