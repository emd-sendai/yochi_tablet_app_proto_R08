using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.IO;

namespace WpfApp.Screens.Aramashi.Models
{
    public class SlideContent
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string? ImageFileName { get; set; }

        // 表示用の絶対パスを持たせる（XAMLのImage Sourceが読めるように）
        [JsonIgnore]
        public string? ImagePath { get; set; }

        // JSON読込後に絶対パスを組み立てる
        public void ResolvePaths(string baseDirectory)
        {
            if (!string.IsNullOrEmpty(ImageFileName))
                ImagePath = Path.Combine(baseDirectory, ImageFileName);
        }
    }

}
