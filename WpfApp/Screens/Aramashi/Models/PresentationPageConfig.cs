using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace WpfApp.Screens.Aramashi.Models
{
    public class PresentationPageConfig
    {
        public string Id { get; set; } = string.Empty;

        public TextBlockData? Title { get; set; }

        public string TitleText => Title?.TextRuns.FirstOrDefault()?.Text ?? "";

        public bool Video { get; set; } = false;

        public string? VideoPath { get; set; }

        public List<PageSection> Sections { get; set; } = new();

        [JsonIgnore]
        public string SourceJsonPath { get; set; } = string.Empty;

        [JsonIgnore]
        public int OriginalIndex { get; set; }

        public void ResolvePaths(string baseDir)
        {
            foreach (var section in Sections)
            {
                //section.ResolvePaths(baseDir);

                if (section.Image?.Images != null)
                {
                    foreach (var imgbtn in section.Image.Images)
                    {
                        imgbtn.ResolveImagePaths(baseDir);
                    }
                }

                if (section.ImageButton?.ImageButtons != null)
                {
                    foreach (var imgbtn in section.ImageButton.ImageButtons)
                    {
                        imgbtn.ResolveImageButtonPaths(baseDir);
                    }
                }
            }
        }
    }

    public class TextBlockData
    {
        public List<StyledTextRun> TextRuns { get; set; } = new();
        public double FontSize { get; set; } = 16;
        public double Indent {  get; set; } = 0;
        public string BackgroundColor { get; set; } = "Transparent";
        public double MaxWidth { get; set; } = 700;
        public string HorizontalAlignment { get; set; } = "stretch";
    }

    public class StyledTextRun
    {
        public string Text { get; set; } = "";
        public bool Bold { get; set; } = false;
        public string Color { get; set; } = "#000000";
        public bool Underline { get; set; } = false;

        public bool IsLink { get; set; } = false;
        public string? CommandName { get; set; }
        public string? CommandParameter { get; set; }

        [JsonIgnore]  // 実行時のみ設定
        public ICommand? Command { get; set; }
    }

    public static class TextBlockDataExtensions
    {
        public static string GetPlainText(this TextBlockData data)
            => string.Join("", data.TextRuns.Select(r => r.Text));

        public static void SetPlainText(this TextBlockData data, string text, bool bold, string color)
        {
            data.TextRuns.Clear();
            data.TextRuns.Add
                (
                    new StyledTextRun 
                    {
                        Text = text,
                        Bold = bold,
                        Color = color
                    });
        }
    }

    public class TableData
    {
        public string Id { get; set; } = "";
        public int Rows { get; set; } = 0;
        public int Columns { get; set; } = 0;
        public double Indent { get; set; } = 0;
        public bool ShowBorder { get; set; } = false;
        public bool HasPadding { get; set; } = false;
        public double MaxWidth { get; set; } = 700;
        public double MaxHeight { get; set; } = 400;
        public List<JsonElement> Cells { get; set; } = new();
    }

    public class PageSection
    {
        public TextBlockData? Subtitle { get; set; }
        public CheckBoxData? CheckBox { get; set; }
        public TextBlockData? Body { get; set; }
        public TableData? Table { get; set; }
        public ImageData? Image { get; set; }
        public TextButtonData? Button { get; set; }
        public ImageButtonData? ImageButton { get; set; }
        public TextBlockData? SignTarget { get; set; }
        public TextBlockData? SignMessage { get; set; }
        public ButtonData? SignButton { get; set; }


        //public string? ImagePath { get; set; }
        /*
        public void ResolvePaths(string baseDir)
        {
            if (string.IsNullOrWhiteSpace(ImagePath))
                return;

            var path = Path.Combine(baseDir, ImagePath);
            if (File.Exists(path))
                ImagePath = path;
        }
        */
    }

    public class CheckBoxData
    {
        public string Id { get; set; } = "";
        public string Label { get; set; } = string.Empty;
        public double FontSize { get; set; } = 18;
        public bool IsChecked { get; set; } = false;

        /* TableDataでのみ使用 */
        public string BackgroundColor { get; set; } = "Transparent";
        [JsonIgnore]
        public int CellIndex { get; set; }
    }

    public class ButtonData
    {
        public string Id { get; set; } = "";
        public string Text { get; set; } = string.Empty;
        public double FontSize { get; set; } = 20;
        public bool Bold { get; set; } = false;
        public double Height { get; set; } = 50;
        public double Width { get; set; } = 100;
        public string Style { get; set; } = "CommonButtonStyle";
        public string? CommandName { get; set; } 
        public string? CommandParameter { get; set; }

        [JsonIgnore]  // 実行時のみ設定
        public ICommand? Command { get; set; }
    }

    public class TextButtonData
    {
        public string Id { get; set; } = "";
        public string HorizontalAlignment { get; set; } = "center";
        public List<ButtonData> TextButtons { get; set; } = new ();
    }

    public class ImageButtonData
    {
        public string Id { get; set; } = "";
        public double MaxHeight { get; set; } = 100;
        public double MaxWidth { get; set; } = 100;
        public List<StyledImageButton> ImageButtons { get; set; } = new();
    }

    public class StyledImageButton
    {
        public string Id { get; set; } = "";
        public string Label {  get; set; } = string.Empty;
        public double LabelFontSize { get; set; } = 15;
        public string LabelColor { get; set; } = "#1F78B4";

        public string? ImagePath { get; set; }

        public void ResolveImageButtonPaths(string baseDir)
        {
            if (string.IsNullOrWhiteSpace(ImagePath))
                return;

            var path = Path.Combine(baseDir, ImagePath);
            if (File.Exists(path))
                ImagePath = path;
        }

        public string? CommandName { get; set; }
        public object? CommandParameter { get; set; }

        [JsonIgnore]  // 実行時のみ設定
        public ICommand? Command { get; set; }
    }

    public class ImageData
    {
        public double MaxHeight { get; set; } = 200;
        public double MaxWidth { get; set; } = 200;
        public string HorizontalAlignment { get; set; } = "center";
        public List<StyledImage> Images { get; set; } = new();
    }

    public class StyledImage
    {
        public string Id { get; set; } = "";
        public string Label { get; set; } = string.Empty;
        public double LabelFontSize { get; set; } = 15;
        public string LabelColor { get; set; } = "#000000";
        public string? ImagePath { get; set; }

        public void ResolveImagePaths(string baseDir)
        {
            if (string.IsNullOrWhiteSpace(ImagePath))
                return;

            var path = Path.Combine(baseDir, ImagePath);
            if (File.Exists(path))
                ImagePath = path;
        }
    }
}
