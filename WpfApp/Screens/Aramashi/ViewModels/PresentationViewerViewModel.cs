using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using WpfApp.Common.ViewModels;
using WpfApp.Common.Views;
using WpfApp.Interfaces;
using WpfApp.Screens.Aramashi.Models;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;

namespace WpfApp.Screens.Aramashi.ViewModels
{
    public partial class PresentationViewerViewModel : ObservableObject
    {
        private int _index = 0;
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;

        public ObservableCollection<PresentationPageConfig> Slides { get; } = new();

        [ObservableProperty]
        private PresentationPageConfig? currentSlide;

        [ObservableProperty]
        private Dictionary<string, bool> pageCheckStates = new();

        [ObservableProperty]
        private string? presentationName;

        public bool AllPagesChecked => PageCheckStates.Count > 0 && PageCheckStates.All(kv => kv.Value);
        //public bool AllPagesChecked => PageCheckStates.All(kv => kv.Value); ページ情報が登録されておらずDictionaryが空の状態でもtrue扱いにする場合

        public bool CanGoPrev => _index > 0;
        public bool CanGoNext => _index < Slides.Count - 1;

        [ObservableProperty]
        private double scale = 1.0;

        public double MinScale { get; } = 0.1;
        public double MaxScale { get; } = 5.0;

        private List<string> _jsonPathList;

        public PresentationViewerViewModel(string _presentationName, NavigationService navigationService, IDialogService dialogService)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            _jsonPathList = GetJsonPaths(_presentationName);
            PresentationName = _presentationName ?? "";

            if (_jsonPathList.Count > 0)
            {
                var slideList = LoadSlidesFromJsons(_jsonPathList);

                if (slideList != null && slideList.Count > 0)
                {
                    Slides.Clear();
                    foreach (var slide in slideList)
                    {
                        Slides.Add(slide);
                    }
                }
                else
                {
                    Debug.WriteLine($"[DEBUG] スライドデータを取得できません");
                    MessageBox.Show("スライドデータを取得できません");
                    return;
                }

                CurrentSlide = Slides.Count > 0 ? Slides[0] : null;
            }
            else
            {
                Debug.WriteLine($"[DEBUG] スライドデータを取得できません");
                MessageBox.Show("スライドデータを取得できません");
                return;
            }
        }

        private List<string> GetJsonPaths(string presentationName)
        {
            List<string> pathList = new();
            var dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SlideResources");

            switch (presentationName)
            {
                case "aramashi":

                    for (int i = 1; ; i++)
                    {
                        var aramashiPath = Path.Combine(dataFolder, "Aramashi", $"slides_aramashi{i}.json");
                        if (!Path.Exists(aramashiPath)) break;

                        pathList.Add(aramashiPath);
                        Debug.WriteLine($"[DEBUG] スライドパスを追加: {aramashiPath}");
                    }
                    break;

                case "daitaichi":

                    var daitaichiPath = Path.Combine(dataFolder, "Others", "slides_daitaichi.json");
                    pathList.Add(daitaichiPath);
                    Debug.WriteLine($"[DEBUG] スライドパスを追加: {daitaichiPath}");
                    break;

                case "zeikin":

                    var zeikinPath = Path.Combine(dataFolder, "Others", "slides_zeikin.json");
                    pathList.Add(zeikinPath);
                    Debug.WriteLine($"[DEBUG] スライドパスを追加: {zeikinPath}");
                    break;
            }
            return pathList;
        }

        private List<PresentationPageConfig> LoadSlidesFromJsons(List<string> pathList)
        {
            List<PresentationPageConfig> ret = new List<PresentationPageConfig>();

            foreach (var path in pathList) 
            {
                if (File.Exists(path))
                {
                    var jsonText = File.ReadAllText(path);
                    var slideList = JsonSerializer.Deserialize<List<PresentationPageConfig>>(jsonText,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (slideList != null)
                    {
                        string slideDir = Path.GetDirectoryName(path)!;

                        for (int i = 0; i < slideList.Count; i++)
                        {
                            slideList[i].SourceJsonPath = path;  // 元のJSONパス
                            slideList[i].OriginalIndex = i;         // JSON内での順番
                            slideList[i].ResolvePaths(slideDir);
                            ret.Add(slideList[i]);
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"[DEBUG] スライドデータが空です: {path}");
                        break;
                    }
                }
                else
                {
                    Debug.WriteLine($"[DEBUG] スライドデータが見つかりません: {path}");
                    break;
                }
            }
            return ret;
        }

        private List<PresentationPageConfig> LoadSlidesFromJson(string path)
        {
            List<PresentationPageConfig> ret = new List<PresentationPageConfig>();

            if (File.Exists(path))
            {
                var jsonText = File.ReadAllText(path);
                var slideList = JsonSerializer.Deserialize<List<PresentationPageConfig>>(jsonText,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (slideList != null)
                {
                    string slideDir = Path.GetDirectoryName(path)!;

                    for (int i = 0; i < slideList.Count; i++)
                    {
                        slideList[i].SourceJsonPath = path;  // 元のJSONパス
                        slideList[i].OriginalIndex = i;         // JSON内での順番
                        slideList[i].ResolvePaths(slideDir);
                        ret.Add(slideList[i]);
                    }
                    return ret;
                }
                else
                {
                    Debug.WriteLine($"[DEBUG] スライドデータが空です: {path}");
                    return ret;
                }
            }
            else
            {
                Debug.WriteLine($"[DEBUG] スライドデータが見つかりません: {path}");
                return ret;
            }
        }

        private void SaveSlidesToJson(PresentationPageConfig slide)
        {
            try
            {
                if(slide != null && File.Exists(slide.SourceJsonPath))
                {
                    List<PresentationPageConfig> saveSlideList = LoadSlidesFromJson(slide.SourceJsonPath);

                    for(int i = 0; i < saveSlideList.Count; i++)
                    {
                        if (saveSlideList[i].OriginalIndex == slide.OriginalIndex)
                        {
                            saveSlideList[i] = slide;
                        }
                    }
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    var json = JsonSerializer.Serialize(saveSlideList, options);
                    File.WriteAllText(slide.SourceJsonPath, json);
                    Debug.WriteLine($"[DEBUG] チェックボックスの情報を保存しました: {slide.SourceJsonPath}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"チェックボックスの情報を保存できません");
                Debug.WriteLine($"[DEBUG] チェックボックスの情報を保存できません: {ex.Message}");
            }
        }

        public void UpdateCheckBoxState(string slideId, string checkBoxId, bool isChecked)
        {
            PresentationPageConfig? slide = Slides.FirstOrDefault(s => s.Id == slideId);
            if (slide == null) return;

            var section = slide.Sections.FirstOrDefault(sec => sec.CheckBox?.Id == checkBoxId);
            if (section?.CheckBox != null)
            {
                section.CheckBox.IsChecked = isChecked;
                SaveSlidesToJson(slide);
            }

            // CheckBoxの状態が変わるたびに全チェック判定を更新
            UpdateAllCheckedState(slideId);
        }

        public void UpdateAllCheckedState(string slideId)
        {
            var slide = Slides.FirstOrDefault(s => s.Id == slideId);
            if (slide == null) return;

            var sections = slide.Sections;
            if (sections == null) return;

            List<CheckBoxData> checkBoxes = new();

            for(int i = 0; i < sections.Count; i++)
            {
                var section = sections[i];
                if (section.CheckBox != null)
                {
                    checkBoxes.Add(section.CheckBox);
                }
            }

            // チェックボックスが存在しない場合は true 扱い
            bool allChecked = checkBoxes.Count == 0 || checkBoxes.All(cb => cb.IsChecked);

            // 辞書に結果を格納（既存なら上書き）
            PageCheckStates[slideId] = allChecked;

            // Dictionary変更通知
            OnPropertyChanged(nameof(PageCheckStates));
            // すべてのPageのCheckBox判定変更通知
            OnPropertyChanged(nameof(AllPagesChecked));
        }

        public void UpdateTableCheckBoxState(string slideId, TableData data, CheckBoxData cbData, string checkBoxId, bool isChecked)
        {
            PresentationPageConfig? slide = Slides.FirstOrDefault(s => s.Id == slideId);
            if (slide == null) return;

            var section = slide.Sections.FirstOrDefault(sec => sec.Table?.Id == data.Id);
            if(section == null) return;

            if (section.Table?.Cells.Count > 0)
            {
                var cells = section.Table.Cells;
                var node = JsonNode.Parse(cells[cbData.CellIndex].GetRawText())!.AsObject();
                node["IsChecked"] = isChecked;
                cells[cbData.CellIndex] = JsonSerializer.Deserialize<JsonElement>(node.ToJsonString());

                SaveSlidesToJson(slide);
            }

            // CheckBoxの状態が変わるたびに全チェック判定を更新
            UpdateAllTableCheckedState(slideId, data);
        }

        public void UpdateAllTableCheckedState(string slideId, TableData data)
        {
            var slide = Slides.FirstOrDefault(s => s.Id == slideId);
            if (slide == null) return;

            var section = slide.Sections.FirstOrDefault(sec => sec.Table?.Id == data.Id);
            if (section == null) return;

            var cells = section.Table?.Cells;
            if (cells == null || cells.Count <= 0) return;

            List<CheckBoxData> checkBoxes = new ();

            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                if (cell.TryGetProperty("isChecked", out _) || cell.TryGetProperty("IsChecked", out _))
                {
                    Debug.WriteLine($"[DEBUG] cell: {i} のCheckBoxの状態を記録します");
                    var obj = JsonSerializer.Deserialize<CheckBoxData>(cell.GetRawText(), 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (obj != null && obj is CheckBoxData)
                    {
                        checkBoxes.Add(obj);
                    }
                }
            }

            // チェックボックスが存在しない場合は true 扱い
            bool allChecked = checkBoxes.Count == 0 || checkBoxes.All(cb => cb.IsChecked);

            // 辞書に結果を格納（既存なら上書き）
            PageCheckStates[slideId] = allChecked;

            // Dictionary変更通知
            OnPropertyChanged(nameof(PageCheckStates));
            // すべてのPageのCheckBox判定変更通知
            OnPropertyChanged(nameof(AllPagesChecked));
        }

        [RelayCommand]
        public void Zoom(double factor)
        {
            double newScale = Scale * factor;
            if (newScale < MinScale) newScale = MinScale;
            if (newScale > MaxScale) newScale = MaxScale;
            Scale = newScale;
        }

        [RelayCommand]
        public void ZoomIn() => Zoom(1.1);

        [RelayCommand]
        public void ZoomOut() => Zoom(1 / 1.1);

        [RelayCommand]
        public void GoToSlide(int index)
        {
            if (index >= 0 && index < Slides.Count)
            {
                _index = index;
                CurrentSlide = Slides[_index];
                OnPropertyChanged(nameof(CanGoPrev));
                OnPropertyChanged(nameof(CanGoNext));
            }
        }

        [RelayCommand]
        private void Next()
        {
            if (_index < Slides.Count - 1)
            {
                CurrentSlide = Slides[++_index];
                OnPropertyChanged(nameof(CanGoPrev));
                OnPropertyChanged(nameof(CanGoNext));
            }
        }

        [RelayCommand]
        private void Prev()
        {
            if (_index > 0)
            {
                CurrentSlide = Slides[--_index];
                OnPropertyChanged(nameof(CanGoPrev));
                OnPropertyChanged(nameof(CanGoNext));
            }
        }

        [RelayCommand]
        private void GoToMenu()
        {
            _nav.Navigate(new MenuViewModel(_nav, _dia));
        }

        [RelayCommand]
        private void GoToAramashiMenu()
        {
            _nav.Navigate(new AramashiMenuViewModel(_nav, _dia));
        }

        [RelayCommand]
        private void PreviewImage(string imagePath)
        {
            _dia.ShowWindow<ImagePreviewViewModel>(imagePath);
        }

        [RelayCommand]
        private void ShowInheritanceOverview(string imagePath)
        {
            _dia.ShowWindow<InheritanceOverviewViewModel>();
        }

        [RelayCommand]
        private async Task EditSignatureAsync(string editedSignMessage)
        {

            if (AllPagesChecked == false)
            {
                //メッセージウィンドウを表示
                var vm = new MessageDialogViewModel
                {
                    Message = "チェックされていない項目があります"
                };

                var dialog = new MessageDialogWindow
                {
                    DataContext = vm,
                    Owner = Application.Current.MainWindow
                };

                bool? result = dialog.ShowDialog();
                if (result == true)
                {
                    // OK押下後の処理
                }
            }
            else
            {
                if(PresentationName == null)
                {
                    MessageBox.Show("署名データを作成できません");
                    Debug.WriteLine($"[DEBUG] PresentationNameがNULLです");
                    return;
                }
                // DialogService を使ってモーダル Window を開く
                var result = await _dia.ShowDialogAsync<SignatureViewModel, string>(editedSignMessage, _jsonPathList[0], PresentationName);

                if (result != null)
                {
                }
            }
        }
    }
}
