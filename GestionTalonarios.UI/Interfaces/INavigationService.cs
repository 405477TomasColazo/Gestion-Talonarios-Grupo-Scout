using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GestionTalonarios.UI.Interfaces
{
    public interface INavigationService
    {
        bool? ShowDialog<T>() where T : Window;
    }
}
