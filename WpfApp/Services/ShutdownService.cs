using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using WpfApp.Interfaces;
using WpfApp.Screens.Aramashi.Models;

namespace WpfApp.Services
{
    public class ShutdownService
    {
        private readonly IAppPathService _path;

        private static bool _hasCleaned = false;

        public ShutdownService()
        {
            _path = new AppPathService();
        }

        public void Cleanup()
        {
            if (_hasCleaned) return;

            _hasCleaned = true;

            // 初期化処理
            try
            {
                // JSON読み込み時の設定
                var readOptions = new JsonSerializerOptions
                {
                    // 大文字と小文字の区別をしない
                    PropertyNameCaseInsensitive = true
                };

                // JSON書き込み時の設定
                var writeOptions = new JsonSerializerOptions
                {
                    // インデントを追加して整形する
                    WriteIndented = true
                };

                /* 用地補償の手順 - 補償のあらまし */
                // チェックボックスの初期化
                if (Path.Exists(_path.AramashiSlideDire))
                {
                    var paths = GetAramashiSlidePaths();
                    int index = 1;

                    foreach (var path in paths)
                    {
                        string json = File.ReadAllText(path);
                        var slides = JsonSerializer.Deserialize<List<PresentationPageConfig>>(json, readOptions);
                        // テーブル内のチェックボックスの初期化
                        ClearTableCheckBox(slides);
                        // チェックボックス（その他）の初期化
                        ClearCheckBox(slides);

                        if (slides != null)
                        {
                            var _json = JsonSerializer.Serialize(slides, writeOptions);
                            File.WriteAllText(path, _json);
                            Debug.WriteLine($"[DEBUG] 用地補償の手順: {index} のチェックボックスを初期化しました");
                        }
                        index++;
                    }
                }

                /* 用地補償の手順 - その他 */
                if (Directory.Exists(_path.OthersSlideDire))
                {
                    var paths = Directory.GetFiles(_path.OthersSlideDire, "*.json");

                    foreach (var path in paths)
                    {
                        string json = File.ReadAllText(path);
                        var slides = JsonSerializer.Deserialize<List<PresentationPageConfig>>(json, readOptions);
                        ClearCheckBox(slides);

                        if (slides != null)
                        {
                            var _json = JsonSerializer.Serialize(slides, writeOptions);
                            File.WriteAllText(path, _json);
                            Debug.WriteLine($"[DEBUG] チェックボックスを初期化しました: {path}");
                        }
                    }
                }


                // 署名データの初期化
                if (Directory.Exists(_path.SignatureDire))
                {
                    // サブフォルダやファイルをまとめて削除
                    Directory.Delete(_path.SignatureDire, true);
                    Debug.WriteLine($"[DEBUG] 署名データを初期化しました: {_path.SignatureDire}");
                }

                // 一時保存フォルダの初期化
                if (Directory.Exists(_path.TempDire))
                {
                    // サブフォルダやファイルをまとめて削除
                    Directory.Delete(_path.TempDire, true);
                    Debug.WriteLine($"[DEBUG] 一時保存フォルダを初期化しました: {_path.TempDire}");
                }

                Debug.WriteLine($"[DEBUG] データを初期化しました");

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] データの初期化ができませんでした");

                Directory.CreateDirectory(_path.LogsDire);
                // 古いlogファイルを削除
                CleanupOldLogs();

                string logPath = Path.Combine(_path.LogsDire, $"shutdown_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                File.AppendAllText(logPath, ex.ToString());
                Debug.WriteLine($"[ERROR] シャットダウン処理時のlogファイルを生成しました : {logPath}");
            }
        }

        public void ClearCheckBox(List<PresentationPageConfig>? slides)
        {
            if (slides == null) return;

            foreach (var slide in slides)
            {
                // Sections
                foreach (var sec in slide.Sections)
                {
                    if (sec.CheckBox != null)
                    {
                        sec.CheckBox.IsChecked = false;
                    }
                }
            }
        }

        public List<string> GetAramashiSlidePaths()
        {
            List<string> paths = new();

            for (int i = 1; ; i++)
            {
                var aramashiPath = Path.Combine(_path.AramashiSlideDire, $"slides_aramashi{i}.json");
                if (!Path.Exists(aramashiPath)) break;

                paths.Add(aramashiPath);
                Debug.WriteLine($"[DEBUG] 用地補償の手順のスライドパスを追加: {aramashiPath}");
            }
            return paths;
        }

        public static void ClearTableCheckBox(List<PresentationPageConfig>? slides)
        {
            if (slides == null) return;

            foreach (var slide in slides)
            {
                // Sections
                foreach (var sec in slide.Sections)
                {
                    // Table が存在する場合
                    if (sec.Table != null)
                    {
                        var cells = sec.Table.Cells;

                        for (int i = 0; i < cells.Count; i++)
                        {
                            // cell に CheckBoxData が格納されている場合
                            if (cells[i].TryGetProperty("isChecked", out _) || cells[i].TryGetProperty("IsChecked", out _))
                            {
                                var node = JsonNode.Parse(cells[i].GetRawText())!.AsObject();
                                node["IsChecked"] = false;
                                cells[i] = JsonSerializer.Deserialize<JsonElement>(node.ToJsonString());
                            }
                        }
                    }
                }
            }
        }


        private void CleanupOldLogs()
        {
            if (!Directory.Exists(_path.LogsDire)) { return; }

            var files = new DirectoryInfo(_path.LogsDire)
                .GetFiles("*.log")
                .OrderByDescending(f => f.CreationTime)
                .ToList();

            // 最新10ファイルを残し、それ以外は削除
            foreach (var oldFile in files.Skip(10))
            {
                try
                {
                    oldFile.Delete();
                }
                catch
                {
                    // 削除失敗は無視
                }
            }
        }

    }
}
