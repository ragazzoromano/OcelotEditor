using System.Windows;

namespace OcelotEditor.Services;

public class MessageDialogService : IMessageDialogService
{
    public void ShowMessage(string message, string caption)
    {
        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowError(string message, string caption)
    {
        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public bool Confirm(string message, string caption)
    {
        return MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }
}
