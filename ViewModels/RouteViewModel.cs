using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using OcelotEditor.Models;

namespace OcelotEditor.ViewModels;

public class RouteViewModel : ObservableObject, IDataErrorInfo
{
    private static readonly string[] DefaultHttpMethods = { "GET", "POST", "PUT", "DELETE", "PATCH" };

    private readonly PropertyChangedEventHandler _hostChangedHandler;
    private readonly PropertyChangedEventHandler _httpMethodChangedHandler;

    private string _upstreamPathTemplate = string.Empty;
    private string _downstreamPathTemplate = string.Empty;
    private string _downstreamScheme = "http";
    private bool _routeIsCaseSensitive;
    private HostAndPortViewModel? _selectedHost;
    private readonly ICollectionView _hostsView;
    private string _hostFilterText = string.Empty;
    private string _portFilterText = string.Empty;

    public RouteViewModel(Route? model = null, IEnumerable<string>? httpMethodOptions = null)
    {
        httpMethodOptions ??= DefaultHttpMethods;

        _hostChangedHandler = (_, __) => OnRouteChanged();
        _httpMethodChangedHandler = (_, __) => OnRouteChanged();

        HttpMethodOptions = new ObservableCollection<HttpMethodOptionViewModel>(
            httpMethodOptions.Select(m => new HttpMethodOptionViewModel(m)));

        foreach (var option in HttpMethodOptions)
        {
            option.PropertyChanged += _httpMethodChangedHandler;
        }

        DownstreamHostAndPorts.CollectionChanged += OnHostsCollectionChanged;
        _hostsView = CollectionViewSource.GetDefaultView(DownstreamHostAndPorts);
        _hostsView.Filter = HostFilter;
        AuthenticationOptions.OptionsChanged += (_, __) => OnRouteChanged();

        AddHostCommand = new RelayCommand(AddHost);
        RemoveSelectedHostCommand = new RelayCommand(RemoveSelectedHost, () => SelectedHost != null);

        if (model != null)
        {
            LoadFromModel(model);
        }
        else
        {
            AddHost();
            var first = HttpMethodOptions.FirstOrDefault();
            if (first != null)
            {
                first.IsSelected = true;
            }
        }
    }

    public ObservableCollection<HttpMethodOptionViewModel> HttpMethodOptions { get; }

    public ObservableCollection<HostAndPortViewModel> DownstreamHostAndPorts { get; } = new();

    public ICollectionView HostsView => _hostsView;

    public AuthenticationOptionsViewModel AuthenticationOptions { get; } = new();

    public RelayCommand AddHostCommand { get; }

    public RelayCommand RemoveSelectedHostCommand { get; }

    public event EventHandler? RouteChanged;

    public string UpstreamPathTemplate
    {
        get => _upstreamPathTemplate;
        set
        {
            if (SetProperty(ref _upstreamPathTemplate, value))
            {
                OnRouteChanged();
            }
        }
    }

    public string DownstreamPathTemplate
    {
        get => _downstreamPathTemplate;
        set
        {
            if (SetProperty(ref _downstreamPathTemplate, value))
            {
                OnRouteChanged();
            }
        }
    }

    public string DownstreamScheme
    {
        get => _downstreamScheme;
        set
        {
            if (SetProperty(ref _downstreamScheme, value))
            {
                OnRouteChanged();
            }
        }
    }

    public bool RouteIsCaseSensitive
    {
        get => _routeIsCaseSensitive;
        set
        {
            if (SetProperty(ref _routeIsCaseSensitive, value))
            {
                OnRouteChanged();
            }
        }
    }

    public HostAndPortViewModel? SelectedHost
    {
        get => _selectedHost;
        set
        {
            if (SetProperty(ref _selectedHost, value))
            {
                RemoveSelectedHostCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string HostFilterText
    {
        get => _hostFilterText;
        set
        {
            if (SetProperty(ref _hostFilterText, value))
            {
                _hostsView.Refresh();
            }
        }
    }

    public string PortFilterText
    {
        get => _portFilterText;
        set
        {
            if (SetProperty(ref _portFilterText, value))
            {
                _hostsView.Refresh();
            }
        }
    }

    public string PrimaryHostSummary => DownstreamHostAndPorts.FirstOrDefault() is { } host
        ? $"{host.Host}:{host.Port}"
        : string.Empty;

    public IEnumerable<string> SelectedHttpMethods => HttpMethodOptions.Where(h => h.IsSelected).Select(h => h.Name);

    private void OnHostsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (HostAndPortViewModel host in e.NewItems)
            {
                host.PropertyChanged += _hostChangedHandler;
            }
        }

        if (e.OldItems != null)
        {
            foreach (HostAndPortViewModel host in e.OldItems)
            {
                host.PropertyChanged -= _hostChangedHandler;
            }
        }

        OnPropertyChanged(nameof(PrimaryHostSummary));
        OnRouteChanged();
    }

    private void AddHost()
    {
        var host = new HostAndPortViewModel { Host = "localhost", Port = 80 };
        DownstreamHostAndPorts.Add(host);
        SelectedHost = host;
    }

    private void RemoveSelectedHost()
    {
        if (SelectedHost != null)
        {
            DownstreamHostAndPorts.Remove(SelectedHost);
            SelectedHost = null;
            OnPropertyChanged(nameof(PrimaryHostSummary));
            OnRouteChanged();
        }
    }

    public void LoadFromModel(Route model)
    {
        UpstreamPathTemplate = model.UpstreamPathTemplate;
        DownstreamPathTemplate = model.DownstreamPathTemplate;
        DownstreamScheme = string.IsNullOrWhiteSpace(model.DownstreamScheme) ? "http" : model.DownstreamScheme;
        RouteIsCaseSensitive = model.RouteIsCaseSensitive;

        ClearHostSubscriptions();
        DownstreamHostAndPorts.Clear();
        foreach (var host in model.DownstreamHostAndPorts ?? new List<HostAndPort>())
        {
            var vm = new HostAndPortViewModel { Host = host.Host, Port = host.Port };
            DownstreamHostAndPorts.Add(vm);
        }

        if (!DownstreamHostAndPorts.Any())
        {
            AddHost();
        }

        foreach (var option in HttpMethodOptions)
        {
            option.IsSelected = false;
        }

        foreach (var method in model.UpstreamHttpMethod ?? Enumerable.Empty<string>())
        {
            EnsureHttpMethodOption(method).IsSelected = true;
        }

        var auth = model.AuthenticationOptions ?? new AuthenticationOptions();
        AuthenticationOptions.AuthenticationProviderKey = auth.AuthenticationProviderKey;
        AuthenticationOptions.AllowedScopes.Clear();
        foreach (var scope in auth.AllowedScopes ?? new List<string>())
        {
            AuthenticationOptions.AllowedScopes.Add(scope);
        }

        OnPropertyChanged(nameof(PrimaryHostSummary));
    }

    private void ClearHostSubscriptions()
    {
        foreach (var host in DownstreamHostAndPorts)
        {
            host.PropertyChanged -= _hostChangedHandler;
        }
    }

    private HttpMethodOptionViewModel EnsureHttpMethodOption(string method)
    {
        var option = HttpMethodOptions.FirstOrDefault(m => string.Equals(m.Name, method, StringComparison.OrdinalIgnoreCase));
        if (option == null)
        {
            option = new HttpMethodOptionViewModel(method);
            option.PropertyChanged += _httpMethodChangedHandler;
            HttpMethodOptions.Add(option);
        }

        return option;
    }

    public Route ToModel()
    {
        return new Route
        {
            UpstreamPathTemplate = UpstreamPathTemplate,
            DownstreamPathTemplate = DownstreamPathTemplate,
            DownstreamScheme = DownstreamScheme,
            RouteIsCaseSensitive = RouteIsCaseSensitive,
            UpstreamHttpMethod = SelectedHttpMethods.ToList(),
            DownstreamHostAndPorts = DownstreamHostAndPorts.Select(h => new HostAndPort
            {
                Host = h.Host,
                Port = h.Port
            }).ToList(),
            AuthenticationOptions = new AuthenticationOptions
            {
                AuthenticationProviderKey = AuthenticationOptions.AuthenticationProviderKey,
                AllowedScopes = AuthenticationOptions.AllowedScopes.ToList()
            }
        };
    }

    public RouteViewModel Clone()
    {
        return new RouteViewModel(ToModel(), HttpMethodOptions.Select(h => h.Name));
    }

    public bool Validate()
    {
        OnPropertyChanged(nameof(UpstreamPathTemplate));
        OnPropertyChanged(nameof(DownstreamPathTemplate));
        foreach (var host in DownstreamHostAndPorts)
        {
            host.RefreshValidation();
        }

        var upstreamValid = !string.IsNullOrWhiteSpace(UpstreamPathTemplate);
        var downstreamValid = !string.IsNullOrWhiteSpace(DownstreamPathTemplate);
        var hostValid = DownstreamHostAndPorts.Any(h => h.IsValid);

        return upstreamValid && downstreamValid && hostValid;
    }

    public string Error => string.Empty;

    public string this[string columnName]
    {
        get
        {
            return columnName switch
            {
                nameof(UpstreamPathTemplate) when string.IsNullOrWhiteSpace(UpstreamPathTemplate) => "Upstream path template is required.",
                nameof(DownstreamPathTemplate) when string.IsNullOrWhiteSpace(DownstreamPathTemplate) => "Downstream path template is required.",
                _ => string.Empty
            };
        }
    }

    private void OnRouteChanged()
    {
        RouteChanged?.Invoke(this, EventArgs.Empty);
    }

    private bool HostFilter(object obj)
    {
        if (obj is not HostAndPortViewModel host)
        {
            return false;
        }

        var matchesHost = string.IsNullOrWhiteSpace(HostFilterText)
            || host.Host.Contains(HostFilterText, StringComparison.OrdinalIgnoreCase);

        var matchesPort = string.IsNullOrWhiteSpace(PortFilterText)
            || host.Port.ToString().Contains(PortFilterText, StringComparison.OrdinalIgnoreCase);

        return matchesHost && matchesPort;
    }
}
