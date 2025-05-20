using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GestionTalonarios.UI.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace GestionTalonarios.UI.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public bool? ShowDialog<T>() where T : Window
        {
            var window = _serviceProvider.GetService<T>();
            return window.ShowDialog();
        }
    }
}
