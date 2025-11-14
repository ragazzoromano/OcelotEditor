namespace OcelotEditor.ViewModels;

public class HttpMethodOptionViewModel : ObservableObject
{
    private bool _isSelected;

    public HttpMethodOptionViewModel(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
