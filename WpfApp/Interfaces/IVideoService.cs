using System.Collections.ObjectModel;
using WpfApp.Common.Models;


namespace WpfApp.Interfaces
{
    public interface IVideoService
    {
        ObservableCollection<VideoData> GetVideoDatas(string path);

        void ChangeVideoFile();
    }
}
