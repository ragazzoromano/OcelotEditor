using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using OcelotEditor.Models;
using OcelotEditor.Services;

namespace OcelotEditor.ViewModels;

public class MainViewModel : ObservableObject
{
    private const string FileFilter = "Ocelot configuration|ocelot.json|JSON files|*.json|All files|*.*";

    private readonly IConfigurationService _configurationService;
    private readonly IFileDialogService _fileDialogService;
    private readonly IMessageDialogService _messageDialogService;

    private readonly ICollectionView _routesView;
    private RouteViewModel? _selectedRoute;
    private string? _currentFilePath;
    private string _statusMessage = "Ready";
    private bool _hasUnsavedChanges;
    private bool _suppressChangeTracking;
    private string _routeUpstreamFilterText = string.Empty;
    private string _routeDownstreamFilterText = string.Empty;

    public MainViewModel(IConfigurationService configurationService,
        IFileDialogService fileDialogService,
        IMessageDialogService messageDialogService)
    {
        _configurationService = configurationService;
        _fileDialogService = fileDialogService;
        _messageDialogService = messageDialogService;

        Routes.CollectionChanged += OnRoutesCollectionChanged;
        _routesView = CollectionViewSource.GetDefaultView(Routes);
        _routesView.Filter = RouteFilter;
        GlobalConfiguration.ConfigurationChanged += (_, __) => MarkDirty();

        OpenCommand = new RelayCommand(Open);
        SaveCommand = new RelayCommand(Save);
        SaveAsCommand = new RelayCommand(SaveAs);
        AddRouteCommand = new RelayCommand(AddRoute);
        DuplicateRouteCommand = new RelayCommand(DuplicateRoute, () => SelectedRoute != null);
        DeleteRouteCommand = new RelayCommand(DeleteRoute, () => SelectedRoute != null);
        MoveRouteUpCommand = new RelayCommand(() => MoveRoute(-1), () => CanMoveRoute(-1));
        MoveRouteDownCommand = new RelayCommand(() => MoveRoute(1), () => CanMoveRoute(1));

        _suppressChangeTracking = true;
        AddRoute();
        HasUnsavedChanges = false;
        _suppressChangeTracking = false;
    }

    public ObservableCollection<RouteViewModel> Routes { get; } = new();

    public ICollectionView RoutesView => _routesView;

    public GlobalConfigurationViewModel GlobalConfiguration { get; } = new();

    public RouteViewModel? SelectedRoute
    {
        get => _selectedRoute;
        set
        {
            if (SetProperty(ref _selectedRoute, value))
            {
                DuplicateRouteCommand.RaiseCanExecuteChanged();
                DeleteRouteCommand.RaiseCanExecuteChanged();
                MoveRouteUpCommand.RaiseCanExecuteChanged();
                MoveRouteDownCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string? CurrentFilePath
    {
        get => _currentFilePath;
        private set => SetProperty(ref _currentFilePath, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        private set => SetProperty(ref _hasUnsavedChanges, value);
    }

    public string RouteUpstreamFilterText
    {
        get => _routeUpstreamFilterText;
        set
        {
            if (SetProperty(ref _routeUpstreamFilterText, value))
            {
                _routesView.Refresh();
            }
        }
    }

    public string RouteDownstreamFilterText
    {
        get => _routeDownstreamFilterText;
        set
        {
            if (SetProperty(ref _routeDownstreamFilterText, value))
            {
                _routesView.Refresh();
            }
        }
    }

    public RelayCommand OpenCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand SaveAsCommand { get; }
    public RelayCommand AddRouteCommand { get; }
    public RelayCommand DuplicateRouteCommand { get; }
    public RelayCommand DeleteRouteCommand { get; }
    public RelayCommand MoveRouteUpCommand { get; }
    public RelayCommand MoveRouteDownCommand { get; }

    private void OnRoutesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            return;
        }

        if (e.NewItems != null)
        {
            foreach (RouteViewModel route in e.NewItems)
            {
                route.RouteChanged += RouteChanged;
            }
        }

        if (e.OldItems != null)
        {
            foreach (RouteViewModel route in e.OldItems)
            {
                route.RouteChanged -= RouteChanged;
            }
        }

        MoveRouteUpCommand.RaiseCanExecuteChanged();
        MoveRouteDownCommand.RaiseCanExecuteChanged();
        _routesView.Refresh();
        MarkDirty();
    }

    private void RouteChanged(object? sender, EventArgs e)
    {
        _routesView.Refresh();
        MarkDirty();
    }

    private void MarkDirty()
    {
        if (_suppressChangeTracking)
        {
            return;
        }

        HasUnsavedChanges = true;
    }

    private void Open()
    {
        if (!ConfirmDiscardChanges())
        {
            return;
        }

        var path = _fileDialogService.ShowOpenFileDialog(FileFilter);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        try
        {
            var configuration = _configurationService.Load(path);
            ApplyConfiguration(configuration);
            CurrentFilePath = path;
            StatusMessage = "Configuration loaded";
        }
        catch (Exception ex)
        {
            _messageDialogService.ShowError($"Unable to load configuration: {ex.Message}", "Error");
        }
    }

    private void Save()
    {
        if (string.IsNullOrEmpty(CurrentFilePath))
        {
            SaveAs();
            return;
        }

        if (!ValidateBeforeSave())
        {
            return;
        }

        WriteConfiguration(CurrentFilePath);
    }

    private void SaveAs()
    {
        var path = _fileDialogService.ShowSaveFileDialog(FileFilter, CurrentFilePath);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        if (!ValidateBeforeSave())
        {
            return;
        }

        WriteConfiguration(path);
        CurrentFilePath = path;
    }

    private void WriteConfiguration(string path)
    {
        try
        {
            var configuration = BuildConfiguration();
            _configurationService.Save(path, configuration);
            StatusMessage = "Configuration saved";
            HasUnsavedChanges = false;
        }
        catch (Exception ex)
        {
            _messageDialogService.ShowError($"Unable to save configuration: {ex.Message}", "Error");
        }
    }

    private bool ValidateBeforeSave()
    {
        if (!Routes.Any())
        {
            _messageDialogService.ShowError("Add at least one route before saving.", "Validation error");
            return false;
        }

        var invalidRoutes = Routes.Where(r => !r.Validate()).ToList();
        if (invalidRoutes.Any())
        {
            _messageDialogService.ShowError("Some routes contain invalid data. Please correct highlighted fields.", "Validation error");
            return false;
        }

        return true;
    }

    private void AddRoute()
    {
        var route = new RouteViewModel();
        Routes.Add(route);
        SelectedRoute = route;
    }

    private void DuplicateRoute()
    {
        if (SelectedRoute == null)
        {
            return;
        }

        var clone = SelectedRoute.Clone();
        var index = Routes.IndexOf(SelectedRoute);
        Routes.Insert(index + 1, clone);
        SelectedRoute = clone;
    }

    private void DeleteRoute()
    {
        if (SelectedRoute == null)
        {
            return;
        }

        if (!_messageDialogService.Confirm("Delete selected route?", "Confirm"))
        {
            return;
        }

        var index = Routes.IndexOf(SelectedRoute);
        Routes.Remove(SelectedRoute);

        if (Routes.Count == 0)
        {
            SelectedRoute = null;
        }
        else
        {
            var newIndex = Math.Clamp(index - 1, 0, Routes.Count - 1);
            SelectedRoute = Routes.ElementAt(newIndex);
        }
    }

    private void MoveRoute(int direction)
    {
        if (SelectedRoute == null)
        {
            return;
        }

        var index = Routes.IndexOf(SelectedRoute);
        var newIndex = index + direction;
        if (newIndex < 0 || newIndex >= Routes.Count)
        {
            return;
        }

        Routes.Move(index, newIndex);
        MoveRouteUpCommand.RaiseCanExecuteChanged();
        MoveRouteDownCommand.RaiseCanExecuteChanged();
    }

    private bool CanMoveRoute(int direction)
    {
        if (SelectedRoute == null)
        {
            return false;
        }

        var index = Routes.IndexOf(SelectedRoute);
        var newIndex = index + direction;
        return newIndex >= 0 && newIndex < Routes.Count;
    }

    private bool ConfirmDiscardChanges()
    {
        if (!HasUnsavedChanges)
        {
            return true;
        }

        return _messageDialogService.Confirm("Discard unsaved changes?", "Confirm");
    }

    private void ApplyConfiguration(OcelotConfiguration configuration)
    {
        _suppressChangeTracking = true;
        DetachAllRoutes();
        Routes.Clear();
        foreach (var route in configuration.Routes ?? new List<Route>())
        {
            Routes.Add(new RouteViewModel(route));
        }

        if (!Routes.Any())
        {
            Routes.Add(new RouteViewModel());
        }

        GlobalConfiguration.BaseUrl = configuration.GlobalConfiguration?.BaseUrl ?? string.Empty;
        _suppressChangeTracking = false;
        HasUnsavedChanges = false;
        SelectedRoute = Routes.FirstOrDefault();
        _routesView.Refresh();
    }

    private void DetachAllRoutes()
    {
        foreach (var route in Routes)
        {
            route.RouteChanged -= RouteChanged;
        }
    }

    private OcelotConfiguration BuildConfiguration()
    {
        return new OcelotConfiguration
        {
            Routes = Routes.Select(r => r.ToModel()).ToList(),
            GlobalConfiguration = new GlobalConfiguration
            {
                BaseUrl = GlobalConfiguration.BaseUrl
            }
        };
    }

    private bool RouteFilter(object obj)
    {
        if (obj is not RouteViewModel route)
        {
            return false;
        }

        var matchesUpstream = string.IsNullOrWhiteSpace(RouteUpstreamFilterText)
            || (!string.IsNullOrEmpty(route.UpstreamPathTemplate)
                && route.UpstreamPathTemplate.Contains(RouteUpstreamFilterText, StringComparison.OrdinalIgnoreCase));

        var matchesDownstream = string.IsNullOrWhiteSpace(RouteDownstreamFilterText)
            || (!string.IsNullOrEmpty(route.DownstreamPathTemplate)
                && route.DownstreamPathTemplate.Contains(RouteDownstreamFilterText, StringComparison.OrdinalIgnoreCase));

        return matchesUpstream && matchesDownstream;
    }
}
