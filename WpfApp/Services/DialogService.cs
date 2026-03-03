using System.Diagnostics;
using System.Windows;
using WpfApp.Interfaces;
using WpfApp.Screens.Aramashi.ViewModels;

namespace WpfApp.Services
{
    /// <summary>
    /// 画面を表示するサービスクラス。
    /// </summary>
    public class DialogService : IDialogService
    {
        /// <summary>
        /// 画面をモーダル表示する。
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
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

            bool isClosedByVm = false;

            if (vm is IDialogResult<TResult> resultVm)
            {
                // ViewModel から閉じる場合
                resultVm.RequestClose += (_, _) =>
                {
                    if (!isClosedByVm)
                    {
                        isClosedByVm = true;
                        window.Close();
                        tcs.TrySetResult(resultVm.Result);
                    }
                };

                // Window の × ボタンや Alt+F4 の場合
                window.Closing += (_, e) =>
                {
                    if (!isClosedByVm)
                    {
                        isClosedByVm = true;
                        // ViewModel 側のイベントを呼ぶ
                        // 外部から呼べるメソッド CloseByUser を追加しておく必要があります
                        if (resultVm is SignatureViewModel sigVm)
                            sigVm.CloseByUser();

                        tcs.TrySetResult(resultVm.Result);
                    }
                };
            }

            Debug.WriteLine($"[DEBUG] DialogService: vmType={vmType.FullName}");
            Debug.WriteLine($"[DEBUG] DialogService: viewTypeName={viewTypeName}");
            Debug.WriteLine($"[DEBUG] DialogService: viewType={viewType?.FullName}");

            window.ShowDialog();
            return await tcs.Task;
        }

        /// <summary>
        /// 画面を非モーダル表示する。
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="parameters"></param>
        /// <exception cref="InvalidOperationException"></exception>
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
