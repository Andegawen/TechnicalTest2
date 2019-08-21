using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PP.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (this.MainWindow != null && MainWindow.DataContext == null)
                this.MainWindow.DataContext = new MainViewModel(new LocalMachineFileSearcher(new FileAbstraction()));
        }
    }
}
