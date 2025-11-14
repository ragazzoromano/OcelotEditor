namespace OcelotEditor.Services;

public interface IMessageDialogService
{
    void ShowMessage(string message, string caption);

    void ShowError(string message, string caption);

    bool Confirm(string message, string caption);
}
