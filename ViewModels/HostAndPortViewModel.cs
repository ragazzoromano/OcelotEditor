using System.ComponentModel;

namespace OcelotEditor.ViewModels;

public class HostAndPortViewModel : ObservableObject, IDataErrorInfo
{
    private string _host = string.Empty;
    private int _port;

    public string Host
    {
        get => _host;
        set => SetProperty(ref _host, value);
    }

    public int Port
    {
        get => _port;
        set => SetProperty(ref _port, value);
    }

    public bool IsValid => string.IsNullOrWhiteSpace(this[nameof(Host)]) && string.IsNullOrWhiteSpace(this[nameof(Port)]);

    public string Error => string.Empty;

    public string this[string columnName]
    {
        get
        {
            return columnName switch
            {
                nameof(Host) when string.IsNullOrWhiteSpace(Host) => "Host is required.",
                nameof(Port) when Port <= 0 => "Port must be greater than zero.",
                _ => string.Empty
            };
        }
    }

    public void RefreshValidation()
    {
        OnPropertyChanged(nameof(Host));
        OnPropertyChanged(nameof(Port));
    }
}
