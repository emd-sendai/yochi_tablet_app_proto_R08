using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using WpfApp.Screens.Aramashi.Models;

namespace WpfApp.Services
{
    public static class ShutdownService
    {
        private static bool _hasCleaned = false;

        private static readonly string BaseDire = AppDomain.CurrentDomain.BaseDirectory;

        private static readonly string SlideFolder = Path.Combine(BaseDire, "SlideResources");
        private static readonly string AramashiFolder = Path.Combine(SlideFolder, "Aramashi");
        private static readonly string OthersFolder = Path.Combine(SlideFolder, "Others");
        private static readonly string DaitaichiSlidePath = Path.Combine(OthersFolder, "slides_daitaichi.json");
        private static readonly string ZeikinSlidePath = Path.Combine(OthersFolder, "slides_zeikin.json");

        private static readonly string SignFolder = Path.Combine(BaseDire, "Data", "Signature");
        private static readonly string DaitaichiIsfSignPath = Path.Combine(SignFolder, "signature_daitaichi.isf");
        private static readonly string DaitaichiPngSignPath = Path.Combine(SignFolder, "signature_daitaichi.png");
        private static readonly string ZeikinIsfSignPath = Path.Combine(SignFolder, "signature_zeikin.isf");
        private static readonly string ZeikinPngSignPath = Path.Combine(SignFolder, "signature_zeikin.png");

        private static readonly string TempFolder = Path.Combine(BaseDire, "Temp");

        public static void Cleanup()
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

                /* 用地補償の手順（あらまし） */
                // チェックボックスの初期化
                if (Path.Exists(AramashiFolder))
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

                /* 代替地 */
                // チェックボックスの初期化
                if (File.Exists(DaitaichiSlidePath))
                {
                    string json = File.ReadAllText(DaitaichiSlidePath);
                    var slides = JsonSerializer.Deserialize<List<PresentationPageConfig>>(json, readOptions);
                    ClearCheckBox(slides);
                    if(slides != null)
                    {
                        var _json = JsonSerializer.Serialize(slides, writeOptions);
                        File.WriteAllText(DaitaichiSlidePath, _json);
                        Debug.WriteLine($"[DEBUG] 代替地チェックボックスを初期化しました");
                    }
                }
                // 署名ISFデータの初期化
                if (File.Exists(DaitaichiIsfSignPath))
                {
                    File.Delete(DaitaichiIsfSignPath);
                    Debug.WriteLine($"[DEBUG] 代替地ISF署名データを初期化しました");
                }
                // 署名PNGデータの初期化
                if (File.Exists(DaitaichiPngSignPath))
                {
                    File.Delete(DaitaichiPngSignPath);
                    Debug.WriteLine($"[DEBUG] 代替地PNG署名データを初期化しました");
                }

                /* 税金等 */
                // チェックボックスの初期化
                if (File.Exists(ZeikinSlidePath))
                {
                    string json = File.ReadAllText(ZeikinSlidePath);
                    var slides = JsonSerializer.Deserialize<List<PresentationPageConfig>>(json, readOptions);
                    ClearCheckBox(slides);
                    if (slides != null)
                    {
                        var _json = JsonSerializer.Serialize(slides, writeOptions);
                        File.WriteAllText(ZeikinSlidePath, _json);
                        Debug.WriteLine($"[DEBUG] 税金等チェックボックスを初期化しました");
                    }
                }
                // 署名ISFデータの初期化
                if (File.Exists(ZeikinIsfSignPath))
                {
                    File.Delete(ZeikinIsfSignPath);
                    Debug.WriteLine($"[DEBUG] 税金等ISF署名データを初期化しました");
                }
                // 署名PNGデータの初期化
                if (File.Exists(ZeikinPngSignPath))
                {
                    File.Delete(ZeikinPngSignPath);
                    Debug.WriteLine($"[DEBUG] 税金等PNG署名データを初期化しました");
                }

                // 一時保存フォルダの初期化
                if (Directory.Exists(TempFolder))
                {
                    // サブフォルダやファイルをまとめて削除
                    Directory.Delete(TempFolder, true);
                    Debug.WriteLine($"[DEBUG] 一時保存フォルダを初期化しました: {TempFolder}");
                }

                Debug.WriteLine($"[DEBUG] データを初期化しました");

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] データの初期化ができませんでした");
                string logPath = Path.Combine(BaseDire, "error.log");
                File.AppendAllText(logPath, ex.ToString());
                Debug.WriteLine($"[ERROR] エラーログ : {logPath}");
            }
        }

        public static void ClearCheckBox(List<PresentationPageConfig>? slides)
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

        public static List<string> GetAramashiSlidePaths()
        {
            List<string> paths = new();

            for (int i = 1; ; i++)
            {
                var aramashiPath = Path.Combine(AramashiFolder, $"slides_aramashi{i}.json");
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
    }
}
