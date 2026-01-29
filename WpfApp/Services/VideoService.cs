using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp.Common.Models;
using WpfApp.Interfaces;

namespace WpfApp.Services
{
    public class VideoService : IVideoService
    {
        public ObservableCollection<VideoData> GetVideoDatas(string path)
        {
            var ret = new ObservableCollection<VideoData>();

            if (!Directory.Exists(path))
            {
                return ret;
            }
            else
            {
                
                return ret;
            }
        }

        public void ChangeVideoFile()
        {

        }

        private void DeleteVideoFile()
        {

        }

        private void AddVideoFile()
        {

        }
    }
}
