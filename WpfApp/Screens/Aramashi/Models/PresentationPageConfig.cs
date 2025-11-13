using System.IO;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace WpfApp.Screens.Aramashi.Models
{
    public class PresentationPageConfig
    {
        public string Id { get; set; } = string.Empty;

        public TextBlockData? Title { get; set; }

        public string TitleText => Title?.TextRuns.FirstOrDefault()?.Text ?? "";

        public List<PageSection> Sections { get; set; } = new();

        public void ResolvePaths(string baseDir)
        {
            foreach (var section in Sections)
            {
                section.ResolvePaths(baseDir);
            }
        }
    }

    public class TextBlockData
    {
        public List<StyledTextRun> TextRuns { get; set; } = new();
        public double FontSize { get; set; } = 18;

    }

    public class StyledTextRun
    {
        public string Text { get; set; } = "";
        public bool Bold { get; set; } = false;
        public string Color { get; set; } = "#000000";
        public bool Underline { get; set; } = false;
    }

    public static class TextBlockDataExtensions
    {
        public static string GetPlainText(this TextBlockData data)
            => string.Join("", data.TextRuns.Select(r => r.Text));

        public static void SetPlainText(this TextBlockData data, string text)
        {
            data.TextRuns.Clear();
            data.TextRuns.Add(new StyledTextRun { Text = text });
        }
    }

    public class PageSection
    {
        public TextBlockData? Subtitle { get; set; }
        public CheckBoxData? CheckBox { get; set; }
        public TextBlockData? Body { get; set; }
        public string? ImagePath { get; set; }

        //public ButtonData? Button { get; set; }

        public TextBlockData? SignMessage { get; set; }
        public ButtonData? SignButton { get; set; }

        public void ResolvePaths(string baseDir)
        {
            if (string.IsNullOrWhiteSpace(ImagePath))
                return;

            var path = Path.Combine(baseDir, ImagePath);
            if (File.Exists(path))
                ImagePath = path;
        }
    }

    public class CheckBoxData
    {
        public string Id { get; set; } = "";
        public string Label { get; set; } = "";
        public bool IsChecked { get; set; } = false;
    }

    public class ButtonData
    {
        public string Id { get; set; } = "";
        public string Text { get; set; } = string.Empty;
        public double FontSize { get; set; } = 20;
        public bool Bold { get; set; } = false;
        public double Height { get; set; } = 50;
        public double Width { get; set; } = 100;
        public string? CommandName { get; set; } 
        public object? CommandParameter { get; set; }

        [JsonIgnore]  // 実行時のみ設定
        public ICommand? Command { get; set; }
    }
}
