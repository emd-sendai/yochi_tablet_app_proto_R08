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
        private const string CreateWorksTable = """
            CREATE TABLE IF NOT EXISTS Works
            (
                WorkId INTEGER PRIMARY KEY AUTOINCREMENT,
                WorkGuid TEXT NOT NULL UNIQUE,
                WorkName TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );
            """;

        private const string CreateLandownersTable = """
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

        private const string CreateWorkLandownersTable = """
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

        private const string CreateAdminPasswordTable = """
            CREATE TABLE IF NOT EXISTS AdminPassword
            (
                Id INTEGER PRIMARY KEY CHECK (Id = 1),
                PasswordHash TEXT NOT NULL,
                Salt TEXT NOT NULL,
                Iterations INTEGER NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );
            """;

        private const string CreateButtonsTable = """
            CREATE TABLE IF NOT EXISTS Buttons
            (
                Type INTEGER NOT NULL,
                Id INTEGER NOT NULL,
                Content TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,

                PRIMARY KEY (Type, Id)
            );
            """;

        private readonly IAppPathService _path;
        private readonly PasswordService _password;
        private readonly DatabaseService _database;
        private readonly IWorkRepository _workRepos;
        private readonly ILandownerRepository _landownerRepos;
        private readonly IWorkLandownerRepository _workLandownerRepos;
        private readonly IAdminPasswordRepository _adminPasswordRepos;
        private readonly IButtonRepository _buttonRepos;

        public DatabaseInitializeService(DatabaseService databaseService, IWorkRepository workRepository,
            ILandownerRepository landownerRepository, IWorkLandownerRepository workLandownerRepository, 
            IAdminPasswordRepository adminPasswordRepository, IButtonRepository buttonRepository)
        {
            _database = databaseService;
            _workRepos = workRepository;
            _landownerRepos = landownerRepository;
            _workLandownerRepos = workLandownerRepository;
            _adminPasswordRepos = adminPasswordRepository;
            _buttonRepos = buttonRepository;
            _path = new AppPathService();
            _password = new PasswordService();

        }

        public async Task InitializeAsync()
        {
            Directory.CreateDirectory(_path.DatabaseDire);

            await using var connection = await _database.CreateConnectionAsync();

            // テーブル作成処理実行前に存在を確認する
            bool worksTableExists = await TableExistsAsync(connection, "Works");
            Debug.WriteLine($"[DEBUG] worksTableExists : {worksTableExists}");
            bool landownersTableExists = await TableExistsAsync(connection, "Landowners");
            Debug.WriteLine($"[DEBUG] landownersTableExists : {landownersTableExists}");
            bool worklandownersTableExits = await TableExistsAsync(connection, "WorkLandowners");
            Debug.WriteLine($"[DEBUG] worklandownersTableExits : {worklandownersTableExits}");

            // テーブルを作成
            await CreateTables(connection, CreateWorksTable);
            await CreateTables(connection, CreateLandownersTable);
            await CreateTables(connection, CreateWorkLandownersTable);
            await CreateTables(connection, CreateAdminPasswordTable);
            await CreateTables(connection, CreateButtonsTable);

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

            bool adminPasswordDataExits = await _adminPasswordRepos.ExistsAsync();
            Debug.WriteLine($"[DEBUG] adminPasswordDataExits : {adminPasswordDataExits}");
            // 管理パスワードテーブルにデータが存在しない場合
            if (!adminPasswordDataExits)
            {
                await InsertAdminPasswordInitialDataAsync();
            }

            int bottonCount = await _buttonRepos.CountAsync();
            // ボタンテーブルにデータが不足している場合
            if (bottonCount != 31)
            {
                await InsertButtonsInitialDataAsync();
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

        private async Task InsertAdminPasswordInitialDataAsync()
        {
            // 初期管理パスワードデータを作成
            PasswordData password = _password.HashPassword("0000");
            DateTime now = DateTime.Now;
            password.CreatedAt = now;
            password.UpdatedAt = now;

            int cnt = await _adminPasswordRepos.InsertAsync(password);
            Debug.WriteLine($"[DEBUG] 管理者パスワードの初期データを {cnt} 件作成しました");
        }

        private async Task InsertButtonsInitialDataAsync()
        {
            List<ButtonData> buttons = LoadButtonsInitialDataFromJson();

            // JSONファイルからデータを取得できなかった場合、仮データを作成する
            if (buttons.Count != 31)
            {
                // 用地補償の手順（type:1、10個）
                buttons = CreateDummyInitialData(1, 10);
                // その他（type:2、5個）
                buttons.AddRange(CreateDummyInitialData(2, 5));
                // 事業全般資料（type:3、8個）
                buttons.AddRange(CreateDummyInitialData(3, 8));
                // 地権者個別資料（type:4、8個）
                buttons.AddRange(CreateDummyInitialData(4, 8));
            }

            foreach (var button in buttons)
            {
                DateTime now = DateTime.Now;
                button.CreatedAt = now;
                button.UpdatedAt = now;
            }
            int cnt = await _buttonRepos.InsertRangeAsync(buttons);
            Debug.WriteLine($"[DEBUG] ボタンの初期データを {cnt} 件作成しました");
        }

        private List<ButtonData> LoadButtonsInitialDataFromJson()
        {
            var path = _path.ButtonsPath;
            List<ButtonData> ret = new ();

            try
            {
                if (!File.Exists(path))
                {
                    Debug.WriteLine($"[ERROR] ボタンの初期データが見つかりません: {path}");
                    return ret;
                }

                string json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<ButtonData>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (list != null)
                {
                    ret = list;
                    Debug.WriteLine($"[DEBUG] ボタンの初期データを {ret.Count} 件読み込みました");
                }
                return ret;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] ボタンの初期データを読み込むことができません: {ex}");
                return ret;
            }
        }

        private List<ButtonData> CreateDummyInitialData(int type, int cnt)
        {
            List<ButtonData> ret = new();
            int id = 1;

            for (int i = 0; i < cnt; i++)
            {
                var button = new ButtonData()
                {
                    Type = type,
                    Id = id,
                    Content = id.ToString()
                };
                ret.Add(button);
                id++;
            }
            return ret;
        }
    }
}
