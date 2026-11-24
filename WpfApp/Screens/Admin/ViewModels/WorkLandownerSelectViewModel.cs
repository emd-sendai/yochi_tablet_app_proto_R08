using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WpfApp.Common.Models;
using WpfApp.Common.Repositories.Interfaces;

namespace WpfApp.Screens.Admin.ViewModels
{
    public partial class WorkLandownerSelectViewModel : ObservableObject
    {
        private readonly IWorkLandownerRepository _workLandownerRepos;

        public event Action? ConfirmRequested;
        public event Action? CancelRequested;

        /// <summary>
        /// 地権者一覧
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<LandownerData> landowners = new();

        /// <summary>
        /// 選択中の地権者（nullの場合は未選択状態を表す）
        /// </summary>
        [ObservableProperty]
        private LandownerData? selectedLandowner;

        public WorkLandownerSelectViewModel(IWorkLandownerRepository workLandownerRepos, int workId)
        {
            _workLandownerRepos = workLandownerRepos ?? throw new ArgumentNullException(nameof(workLandownerRepos));

            LoadLandowners(workId);
        }

        private async void LoadLandowners(int workId)
        {
            var list = await _workLandownerRepos.GetNotRelatedLandownersByWorkIdAsync(workId);
            Landowners = new ObservableCollection<LandownerData>(list);
        }

        [RelayCommand]
        private void Confirm()
        {
            if (SelectedLandowner == null) { return; }
            ConfirmRequested?.Invoke();
        }

        [RelayCommand]
        private void Cancel()
        {
            CancelRequested?.Invoke();
        }
    }
}
