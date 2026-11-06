using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp.Shell.Models
{
    public interface IShellLayout
    {
        Type HeaderType { get; }
        
        Type SidebarType { get; }
    }
}
