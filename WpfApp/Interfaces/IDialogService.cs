using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp.Interfaces
{
    public interface IDialogService
    {
        Task<TResult?> ShowDialogAsync<TViewModel, TResult>(params object[] parameters) where TViewModel : class;
        void ShowWindow<TViewModel>(params object[] parameters) where TViewModel : class;
    }
}
