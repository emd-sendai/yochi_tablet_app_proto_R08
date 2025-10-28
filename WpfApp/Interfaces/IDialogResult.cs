using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp.Interfaces
{
    public interface IDialogResult<TResult>
    {
        event EventHandler? RequestClose;
        TResult? Result { get; }

    }
}
