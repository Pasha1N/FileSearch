using FileSearch.ViewModels;
using System.Windows;

namespace FileSearch
{

    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();
            View.MainWindow mainWindow = new View.MainWindow(mainWindowViewModel);
            mainWindow.Show();
        }
    }
}