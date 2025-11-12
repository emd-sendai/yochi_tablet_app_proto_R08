using System.IO;
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

    public class PageSection
    {
        public TextBlockData? Subtitle { get; set; }
        public CheckBoxData? CheckBox { get; set; }
        public TextBlockData? Body { get; set; }
        public string? ImagePath { get; set; }

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
        public string BackgroundColor { get; set; } = "#FFFFFF";
        public string ForegroundColor { get; set; } = "#000000";
        public double FontSize { get; set; } = 14;
        public bool Bold { get; set; } = false;
        public ICommand? Command { get; set; } 
        public object? CommandParameter { get; set; }
    }
}
