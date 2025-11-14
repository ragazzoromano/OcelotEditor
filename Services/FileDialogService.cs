using Microsoft.Win32;

namespace OcelotEditor.Services;

public class FileDialogService : IFileDialogService
{
    public string? ShowOpenFileDialog(string filter)
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter,
            CheckFileExists = true
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? ShowSaveFileDialog(string filter, string? initialPath = null)
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter,
            FileName = initialPath ?? string.Empty
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
