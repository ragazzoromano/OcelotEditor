namespace OcelotEditor.ViewModels;

public class GlobalConfigurationViewModel : ObservableObject
{
    private string _baseUrl = string.Empty;

    public string BaseUrl
    {
        get => _baseUrl;
        set
        {
            if (SetProperty(ref _baseUrl, value))
            {
                ConfigurationChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public event EventHandler? ConfigurationChanged;
}
