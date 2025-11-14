namespace OcelotEditor.Services;

public interface IFileDialogService
{
    string? ShowOpenFileDialog(string filter);

    string? ShowSaveFileDialog(string filter, string? initialPath = null);
}
