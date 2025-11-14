using System.Collections.ObjectModel;

namespace OcelotEditor.ViewModels;

public class AuthenticationOptionsViewModel : ObservableObject
{
    private string _authenticationProviderKey = string.Empty;
    private string? _newScope;
    private string? _selectedScope;

    public AuthenticationOptionsViewModel()
    {
        AllowedScopes.CollectionChanged += (_, __) => OnOptionsChanged();
        AddScopeCommand = new RelayCommand(AddScope, () => !string.IsNullOrWhiteSpace(NewScope));
        RemoveScopeCommand = new RelayCommand(RemoveScope, () => !string.IsNullOrWhiteSpace(SelectedScope));
    }

    public ObservableCollection<string> AllowedScopes { get; } = new();

    public string AuthenticationProviderKey
    {
        get => _authenticationProviderKey;
        set
        {
            if (SetProperty(ref _authenticationProviderKey, value))
            {
                OnOptionsChanged();
            }
        }
    }

    public string? NewScope
    {
        get => _newScope;
        set
        {
            if (SetProperty(ref _newScope, value))
            {
                AddScopeCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string? SelectedScope
    {
        get => _selectedScope;
        set
        {
            if (SetProperty(ref _selectedScope, value))
            {
                RemoveScopeCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public RelayCommand AddScopeCommand { get; }

    public RelayCommand RemoveScopeCommand { get; }

    public event EventHandler? OptionsChanged;

    private void AddScope()
    {
        if (!string.IsNullOrWhiteSpace(NewScope))
        {
            AllowedScopes.Add(NewScope.Trim());
            NewScope = string.Empty;
            OnOptionsChanged();
        }
    }

    private void RemoveScope()
    {
        if (!string.IsNullOrWhiteSpace(SelectedScope))
        {
            AllowedScopes.Remove(SelectedScope);
            SelectedScope = null;
            OnOptionsChanged();
        }
    }

    private void OnOptionsChanged()
    {
        OptionsChanged?.Invoke(this, EventArgs.Empty);
    }
}
