using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using WpfApp.Common.Models;
using WpfApp.Common.Repositories.Interfaces;
using WpfApp.Interfaces;

namespace WpfApp.Services
{
    public class DatabaseInitializeService
    {
        private const string CreateWorksTableSql = """
            CREATE TABLE IF NOT EXISTS Works
            (
                WorkId INTEGER PRIMARY KEY AUTOINCREMENT,
                WorkGuid TEXT NOT NULL UNIQUE,
                WorkName TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );
            """;

        private const string CreateLandownersTableSql = """
            CREATE TABLE IF NOT EXISTS Landowners
            (
                LandownerId INTEGER PRIMARY KEY AUTOINCREMENT,
                LandownerGuid TEXT NOT NULL UNIQUE,
                LandownerName TEXT NOT NULL,
                LandownerKana TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );
            """;

        private const string CreateWorkLandownersTableSql = """
            CREATE TABLE IF NOT EXISTS WorkLandowners
            (
                WorkLandownerGuid TEXT NOT NULL UNIQUE,
                WorkId INTEGER NOT NULL,
                LandownerId INTEGER NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
            
                PRIMARY KEY (WorkId, LandownerId),
            
                FOREIGN KEY (WorkId)
                    REFERENCES Works(WorkId)
                    ON DELETE CASCADE,
            
                FOREIGN KEY (LandownerId)
                    REFERENCES Landowners(LandownerId)
                    ON DELETE CASCADE
            );
            """;

        private readonly IAppPathService _path;
        private readonly DatabaseService _database;
        private readonly IWorkRepository _workRepos;
        private readonly ILandownerRepository _landownerRepos;
        private readonly IWorkLandownerRepository _workLandownerRepos;

        public DatabaseInitializeService(DatabaseService databaseService, IWorkRepository workRepository,
            ILandownerRepository landownerRepository, IWorkLandownerRepository workLandownerRepository)
        {
            _path = new AppPathService();
            _database = databaseService;
            _workRepos = workRepository;
            _landownerRepos = landownerRepository;
            _workLandownerRepos = workLandownerRepository;
        }

        public async Task InitializeAsync()
        {
            Directory.CreateDirectory(_path.DatabaseDire);

            await using var connection = await _database.CreateConnectionAsync();

            bool worksTableExists = await TableExistsAsync(connection, "Works");
            Debug.WriteLine($"[DEBUG] worksTableExists : {worksTableExists}");
            bool landownersTableExists = await TableExistsAsync(connection, "Landowners");
            Debug.WriteLine($"[DEBUG] landownersTableExists : {landownersTableExists}");
            bool worklandownersTableExits = await TableExistsAsync(connection, "WorkLandowners");
            Debug.WriteLine($"[DEBUG] worklandownersTableExits : {worklandownersTableExits}");

            // テーブルを作成
            await CreateTables(connection, CreateWorksTableSql);
            await CreateTables(connection, CreateLandownersTableSql);
            await CreateTables(connection, CreateWorkLandownersTableSql);

            int worksCnt = 0;
            int landownersCnt = 0;

            // 事業テーブルが新規作成された場合
            if (!worksTableExists)
            {
                worksCnt = await InsertWorksSampleDataAsync();
            }
            // 地権者テーブルが新規作成された場合
            if (!landownersTableExists)
            {
                landownersCnt = await InsertLandownersSampleDataAsync();
            }
            // 事業テーブル、地権者テーブル、事業と地権者の紐付けテーブルが新規作成された場合
            if (!worksTableExists && !landownersTableExists && !worklandownersTableExits)
            {
                bool hasEnoughData = false;
                // 事業が5件以上かつ地権者が10件以上作成された場合
                if (worksCnt >= 5 && landownersCnt >= 10)
                {
                    hasEnoughData = true;
                }
                await InsertWorkLandownersSampleDataAsync(hasEnoughData);
            }
        }

        private async Task CreateTables(SqliteConnection connection, string sql)
        {
            await using var command = new SqliteCommand(sql, connection);
            await command.ExecuteNonQueryAsync();
        }

        private async Task<bool> TableExistsAsync(SqliteConnection connection, string tableName)
        {
            string sql = """
                SELECT COUNT(*)
                FROM sqlite_master
                WHERE type = 'table'
                    AND name = $TableName
                """;

            await using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("$TableName", tableName);

            var ret = await command.ExecuteScalarAsync();
            Debug.WriteLine($"[DEBUG] {tableName}TableCOUNT : {Convert.ToInt64(ret)}");

            return Convert.ToInt64(ret) > 0;
        }

        private async Task<int> InsertWorksSampleDataAsync()
        {
            List<WorkData> works = LoadWorksSampleDataFromJson();
            foreach (WorkData work in works)
            {
                work.WorkGuid = Guid.NewGuid().ToString();
                DateTime now = DateTime.Now;
                work.CreatedAt = now;
                work.UpdatedAt = now;
            }
            int cnt = await _workRepos.InsertRangeAsync(works);
            Debug.WriteLine($"[DEBUG] 事業のサンプルデータを {cnt} 件作成しました");
            return cnt;
        }

        private List<WorkData> LoadWorksSampleDataFromJson()
        {
            var path = _path.WorksPath;
            // 仮データ（JSONファイルからデータを取得できなかった時用）
            List<WorkData> ret = new()
            {
                new WorkData()
                {
                    WorkName = "国道１号○○事業"
                }
            };

            try
            {
                if (!File.Exists(path))
                {
                    Debug.WriteLine($"[ERROR] 事業のサンプルデータが見つかりません: {path}");
                    return ret;
                }

                string json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<WorkData>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (list != null)
                {
                    ret = list;
                    Debug.WriteLine($"[DEBUG] 事業のサンプルデータを {ret.Count} 件読み込みました");
                }
                return ret;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] 事業のサンプルデータを読み込むことができません: {ex}");
                return ret;
            }
        }

        private async Task<int> InsertLandownersSampleDataAsync()
        {
            List<LandownerData> landowners = LoadLandownersSampleDataFromJson();
            foreach (var landowner in landowners)
            {
                landowner.LandownerGuid = Guid.NewGuid().ToString();
                DateTime now = DateTime.Now;
                landowner.CreatedAt = now;
                landowner.UpdatedAt = now;
            }
            int cnt = await _landownerRepos.InsertRangeAsync(landowners);
            Debug.WriteLine($"[DEBUG] 地権者のサンプルデータを {cnt} 件作成しました");
            return cnt;
        }

        private List<LandownerData> LoadLandownersSampleDataFromJson()
        {
            var path = _path.LandownersPath;
            // 仮データ（JSONファイルからデータを取得できなかった時用）
            List<LandownerData> ret = new()
            {
                new LandownerData()
                {
                    LandownerName = "国土太郎",
                    LandownerKana = "コクドタロウ"
                }
            };

            try
            {
                if (!File.Exists(path))
                {
                    Debug.WriteLine($"[ERROR] 地権者のサンプルデータが見つかりません: {path}");
                    return ret;
                }

                string json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<LandownerData>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (list != null)
                {
                    ret = list;
                    Debug.WriteLine($"[DEBUG] 地権者のサンプルデータを {ret.Count} 件読み込みました");
                }
                return ret;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] 地権者のサンプルデータを読み込むことができません: {ex}");
                return ret;
            }
        }

        private async Task InsertWorkLandownersSampleDataAsync(bool hasEnoughData)
        {
            List<WorkLandownerData> workLandowners = LoadWorkLandownersSampleDataFromJson(hasEnoughData);
            foreach (var workLandowner in workLandowners)
            {
                workLandowner.WorkLandownerGuid = Guid.NewGuid().ToString();
                DateTime now = DateTime.Now;
                workLandowner.CreatedAt = now;
                workLandowner.UpdatedAt = now;
            }
            int cnt = await _workLandownerRepos.InsertRangeAsync(workLandowners);
            Debug.WriteLine($"[DEBUG] 事業と地権者の紐付けのサンプルデータを {cnt} 件作成しました");
        }

        private List<WorkLandownerData> LoadWorkLandownersSampleDataFromJson(bool hasEnoughData)
        {
            var path = _path.WorkLandownersPath;
            // 仮データ（JSONファイルからデータを取得できなかった時用）
            List<WorkLandownerData> ret = new()
            {
                new WorkLandownerData()
                {
                    WorkId = 1,
                    LandownerId = 1
                }
            };
            // 事業データと地権者データが不足している場合
            if (!hasEnoughData) { return ret; }

            try
            {
                if (!File.Exists(path))
                {
                    Debug.WriteLine($"[ERROR] 事業と地権者の紐付けのサンプルデータが見つかりません: {path}");
                    return ret;
                }

                string json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<WorkLandownerData>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (list != null)
                {
                    ret = list;
                    Debug.WriteLine($"[DEBUG] 事業と地権者の紐付けのサンプルデータを {ret.Count} 件読み込みました");
                }
                return ret;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] 事業と地権者の紐付けのサンプルデータを読み込むことができません: {ex}");
                return ret;
            }
        }
    }
}
