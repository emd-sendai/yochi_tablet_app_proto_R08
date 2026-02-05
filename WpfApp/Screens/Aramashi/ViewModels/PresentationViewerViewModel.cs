using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using WpfApp.Common.ViewModels;
using WpfApp.Interfaces;
using WpfApp.Screens.Aramashi.Models;
using WpfApp.Screens.Menu.ViewModels;
using WpfApp.Services;
using static WpfApp.Common.ViewModels.VideoPlayerViewModel;

namespace WpfApp.Screens.Aramashi.ViewModels
{
    public partial class PresentationViewerViewModel : ObservableObject
    {
        public int _index = 0;
        private readonly NavigationService _nav;
        private readonly IDialogService _dia;
        private readonly PdfDocumentService _pdf;
        private readonly IMessageDialogService _messageDialog;
        private readonly IAppPathService _path;

        public ObservableCollection<PresentationPageConfig> Slides { get; } = new();

        [ObservableProperty]
        private PresentationPageConfig? currentSlide;

        [ObservableProperty]
        private Dictionary<string, bool> pageCheckStates = new();

        [ObservableProperty]
        private string? topButtonText;

        public bool AllPagesChecked => PageCheckStates.Count > 0 && PageCheckStates.All(kv => kv.Value);
        //public bool AllPagesChecked => PageCheckStates.All(kv => kv.Value); ページ情報が登録されておらずDictionaryが空の状態でもtrue扱いにする場合
        
        public bool CanGoPrev => _index > 0;
        public bool CanGoNext => _index < Slides.Count - 1;

        //public int SlideIndex => _index + 1;

        [ObservableProperty]
        private double scale = 1.0;

        public double MinScale { get; } = 0.1;
        public double MaxScale { get; } = 5.0;

        private List<string> _jsonPathList;

        public enum PresentationCategory
        {
            Aramashi,
            Others001,
            Others002,
            Others003,
            Others004,
            Others005,
        }

        private readonly PresentationCategory _category;

        public PresentationViewerViewModel(NavigationService navigationService, IDialogService dialogService, PresentationCategory category)
        {
            _nav = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dia = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _pdf = new PdfDocumentService();
            _messageDialog = new MessageDialogService();
            _path = new AppPathService();

            _category = category;

            _jsonPathList = GetJsonPaths();

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
                    _messageDialog.ShowMessage("スライドデータを取得できません");
                    return;
                }

                CurrentSlide = Slides.Count > 0 ? Slides[0] : null;
            }
            else
            {
                Debug.WriteLine($"[DEBUG] スライドデータを取得できません");
                _messageDialog.ShowMessage("スライドデータを取得できません");
                return;
            }

            TopButtonText = GetTopButtonText();
        }

        private List<string> GetJsonPaths()
        {
            List<string> pathList = new();

            switch (_category)
            {
                case PresentationCategory.Aramashi:

                    for (int i = 1; ; i++)
                    {
                        var aramashiPath = _path.GetAramashiSlidePath(i);
                        if (!Path.Exists(aramashiPath)) break;

                        pathList.Add(aramashiPath);
                        Debug.WriteLine($"[DEBUG] スライドパスを追加: {aramashiPath}");
                    }
                    break;

                case PresentationCategory.Others001:

                    var others001Path = _path.OthersSlide001Path;
                    pathList.Add(others001Path);
                    Debug.WriteLine($"[DEBUG] スライドパスを追加: {others001Path}");
                    break;

                case PresentationCategory.Others002:

                    var others002Path = _path.OthersSlide002Path;
                    pathList.Add(others002Path);
                    Debug.WriteLine($"[DEBUG] スライドパスを追加: {others002Path}");
                    break;

                case PresentationCategory.Others003:

                    var others003Path = _path.OthersSlide003Path;
                    pathList.Add(others003Path);
                    Debug.WriteLine($"[DEBUG] スライドパスを追加: {others003Path}");
                    break;

                case PresentationCategory.Others004:

                    var others004Path = _path.OthersSlide004Path;
                    pathList.Add(others004Path);
                    Debug.WriteLine($"[DEBUG] スライドパスを追加: {others004Path}");
                    break;
            }
            return pathList;
        }

        private List<PresentationPageConfig> LoadSlidesFromJsons(List<string> pathList)
        {
            List<PresentationPageConfig> ret = new List<PresentationPageConfig>();

            int globalIndex = 0;
            int slidePartIndex = 1;

            foreach (var path in pathList) 
            {
                var videofolderName = slidePartIndex.ToString("D3");

                if (File.Exists(path))
                {
                    var jsonText = File.ReadAllText(path);
                    var slideList = JsonSerializer.Deserialize<List<PresentationPageConfig>>(jsonText,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (slideList != null)
                    {
                        // スライドを順番に並べ替える
                        slideList = slideList.OrderBy(x => x.OriginalIndex).ToList();

                        string slideDir = Path.GetDirectoryName(path)!;
                        int slideTopPageGlobalIndex = 0;

                        for (int i = 0; i < slideList.Count; i++)
                        {
                            slideList[i].SourceJsonPath = path;     // 元のJSONパス
                            slideList[i].PageIndex = $"{slideList[i].OriginalIndex + 1} / {slideList.Count}";　　// ページ番号表示用
                            slideList[i].GlobalIndex = globalIndex; // 全体での順番
                            slideList[i].ResolvePaths(slideDir);
                            slideList[i].VideoFolderName = videofolderName; // 用地補償の手順スライドでのみ使用

                            if (i == 0)
                            {
                                slideTopPageGlobalIndex = globalIndex;
                            }
                            slideList[i].TopPageGlobalIndex = slideTopPageGlobalIndex;　　// 手順（項目）のTOPページ番号

                            ret.Add(slideList[i]);

                            globalIndex++;
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

                slidePartIndex++;
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
                    // スライドを順番に並べ替える
                    slideList = slideList.OrderBy(x => x.OriginalIndex).ToList();

                    string slideDir = Path.GetDirectoryName(path)!;

                    for (int i = 0; i < slideList.Count; i++)
                    {
                        slideList[i].SourceJsonPath = path;  // 元のJSONパス
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

        private string GetTopButtonText()
        {
            var ret = "本項目TOP";

            if (_category == PresentationCategory.Aramashi)
            {
                ret = "本手順TOP";
            }

            return ret;
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
                _messageDialog.ShowMessage($"チェックボックスの情報を保存できません");
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
                //OnPropertyChanged(nameof(SlideIndex));
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
                //OnPropertyChanged(nameof(SlideIndex));
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
                //OnPropertyChanged(nameof(SlideIndex));
            }
        }

        [RelayCommand]
        private void GoToTopPage()
        {
            if(CurrentSlide == null) return;

            GoToSlide(CurrentSlide.TopPageGlobalIndex);
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
        private void GoToDaitaichi()
        {
            _nav.Navigate(new PresentationViewerViewModel(_nav, _dia, PresentationCategory.Others001));
        }

        [RelayCommand]
        private void GoToZeikin(int pageIndex)
        {
            var newVM = new PresentationViewerViewModel(_nav, _dia, PresentationCategory.Others002);
            _nav.Navigate(newVM);
            newVM.GoToSlide(pageIndex);
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
                _messageDialog.ShowMessage("チェックされていない項目があります");
            }
            else
            {
                if(_category == PresentationCategory.Others001 || _category == PresentationCategory.Others002)
                {
                    // DialogService を使ってモーダル Window を開く
                    var result = await _dia.ShowDialogAsync<SignatureViewModel, string>(editedSignMessage, _jsonPathList[0], _category);
                }
                else
                {
                    _messageDialog.ShowMessage("署名データを作成できません");
                    Debug.WriteLine($"[DEBUG] PresentationNameがNULLです");
                    return;
                }
            }
        }

        [RelayCommand]
        private void OpenUrl(string url)
        {
            if (_messageDialog.ShowConfirm($"外部サイトをブラウザで開きます。よろしいですか？"))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                    Debug.WriteLine($"[DEBUG] 外部サイトをブラウザで開きます :{url}");
                }
                catch (Exception ex)
                {
                    _messageDialog.ShowMessage($"以下の外部サイトをブラウザで開くことができません。\n{url}");
                    Debug.WriteLine($"[ERROR] 外部サイトをブラウザで開くことができません :{url}");
                    Debug.WriteLine($"[ERROR] {ex.Message}");
                }
            }
        }

        [RelayCommand]
        private void OpenPdfDocuments(string folderName)
        {
            if (!string.IsNullOrWhiteSpace(folderName))
            {
                var folderPath = Path.Combine(_path.OthersPdfDire, folderName);

                var pdfList = _pdf.GetPdfList(folderPath);

                // PDF資料が存在しない場合
                if (pdfList.Count <= 0)
                {
                    Debug.WriteLine($"[ERROR] PDFファイルが存在しません {folderPath}");
                    //メッセージウィンドウを表示
                    _messageDialog.ShowMessage("PDFファイルが存在しません。");
                }
                else
                {
                    // 非モーダル表示
                    _dia.ShowWindow<DocumentSelectViewModel>(pdfList);
                }
            }
            else
            {
                Debug.WriteLine($"[ERROR] folderNameが空です");
            }
        }

        [RelayCommand]
        private void OpenOthers003PdfDocument(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                if (_messageDialog.ShowConfirm($"PDFファイルを開きます。よろしいですか？"))
                {
                    _pdf.OpenPdf(_path.Others003PdfDire, fileName);
                }
            }
            else
            {
                Debug.WriteLine($"[ERROR] fileNameが空です");
            }
        }

        [RelayCommand]
        private void OpenOthers004PdfDocument(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                if (_messageDialog.ShowConfirm($"PDFファイルを開きます。よろしいですか？"))
                {
                    _pdf.OpenPdf(_path.Others004PdfDire, fileName);
                }
            }
            else
            {
                Debug.WriteLine($"[ERROR] fileNameが空です");
            }
        }

        [RelayCommand]
        private void OpenVideoPlayer()
        {
            if (CurrentSlide == null) { return; }

            string path = string.Empty;

            if (_category == PresentationCategory.Aramashi)
            {
                path = Path.Combine(_path.MediaPartsAramashiDire, CurrentSlide.VideoFolderName);
            }
            else if (_category == PresentationCategory.Others001)
            {
                path = Path.Combine(_path.MediaPartsOthersDire, "001");
            }
            else if (_category == PresentationCategory.Others002)
            {
                path = Path.Combine(_path.MediaPartsOthersDire, "002");
            }

            if(!Directory.Exists(path))
            {
                _messageDialog.ShowMessage("動画が存在しません。");
                Debug.WriteLine($"[ERROR] 動画フォルダが存在しません: {path}");
                return;
            }

            // 非モーダル表示
            _dia.ShowWindow<VideoPlayerViewModel>(path, MediaCategory.Parts);
        }
    }
}
