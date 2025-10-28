using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfApp.Interfaces;
using WpfApp.Common.Views;

namespace WpfApp.Services
{
    public class DialogService : IDialogService
    {
        public async Task<TResult?> ShowDialogAsync<TViewModel, TResult>(params object[] parameters) where TViewModel : class
        {
            var tcs = new TaskCompletionSource<TResult?>();
            var vmType = typeof(TViewModel);
            var viewTypeName = vmType.FullName!.Replace(".ViewModels.", ".Views.").Replace("ViewModel", "Window");
            var viewType = vmType.Assembly.GetType(viewTypeName)
                ?? throw new InvalidOperationException($"対応する View が見つかりません: {viewTypeName}");

            var window = Activator.CreateInstance(viewType) as Window ?? throw new InvalidOperationException($"View の生成に失敗: {viewType.Name}");
            var vm = Activator.CreateInstance(vmType, parameters) as TViewModel ?? throw new InvalidOperationException($"ViewModel の生成に失敗: {vmType.Name}");

            window.DataContext = vm;

            if (vm is IDialogResult<TResult> resultVm)
            {
                resultVm.RequestClose += (_, _) =>
                {
                    window.Close();
                    tcs.SetResult(resultVm.Result);
                };
            }

            window.ShowDialog();
            return await tcs.Task;
        }

        public void ShowWindow<TViewModel>(params object[] parameters) where TViewModel : class
        {
            var vmType = typeof(TViewModel);
            System.Diagnostics.Debug.WriteLine($"[DEBUG] 変換前の名前: {vmType.FullName}");
            var viewTypeName = vmType.FullName!.Replace(".ViewModels.", ".Views.").Replace("ViewModel", "Window");
            var viewType = vmType.Assembly.GetType(viewTypeName) ?? throw new InvalidOperationException($"対応する View が見つかりません: {viewTypeName}");

            if (Activator.CreateInstance(viewType) is not Window window)
                throw new InvalidOperationException($"View の生成に失敗: {viewType.Name}");

            var vm = Activator.CreateInstance(vmType, parameters) as TViewModel ?? throw new InvalidOperationException($"ViewModel の生成に失敗: {vmType.Name}");

            window.DataContext = vm;
            window.Show();
        }
    }
}
