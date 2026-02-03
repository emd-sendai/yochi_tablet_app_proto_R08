using System.Collections.ObjectModel;
using WpfApp.Common.Models;
using WpfApp.Screens.Admin.Models;
using static WpfApp.Common.ViewModels.VideoPlayerViewModel;


namespace WpfApp.Interfaces
{
    public interface IVideoService
    {
        string GetVideoFilePath(string direPath);

        IReadOnlyList<Chapter> GetVideoChapterList(string path);

        ObservableCollection<ChapterItemData> GetVideoChapterDatas(string path);

        void ChangeVideoFile(VideoData data, string sourceFilePath);

        void ClearChapter(string path);
    }
}
