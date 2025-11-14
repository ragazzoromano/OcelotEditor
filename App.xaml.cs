using System.Windows;
using OcelotEditor.Services;
using OcelotEditor.ViewModels;
using OcelotEditor.Views;

namespace OcelotEditor;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var configurationService = new JsonConfigurationService();
        var fileDialogService = new FileDialogService();
        var messageDialogService = new MessageDialogService();

        var mainViewModel = new MainViewModel(configurationService, fileDialogService, messageDialogService);

        var window = new MainWindow
        {
            DataContext = mainViewModel
        };

        window.Show();
    }
}
