using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Navigation;
using Versatile.Core.Services;
using Versatile.Navigation;
using Versatile.Plays.Services;
using Versatile.Views;

namespace Versatile.ViewModels;

public class ShellViewModel : ObservableRecipient
{
    private bool _isBackEnabled;
    private object? _selected;

    public INavigationService NavigationService
    {
        get;
    }

    public INavigationViewService NavigationViewService
    {
        get;
    }

    public bool IsBackEnabled
    {
        get => _isBackEnabled;
        set => SetProperty(ref _isBackEnabled, value);
    }

    public object? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    private bool _isBattleTabEnabled;
    public bool IsBattleTabEnabled
    {
        get => _isBattleTabEnabled;
        set => SetProperty(ref _isBattleTabEnabled, value);
    }

    private bool _isLoaded;
    public bool IsLoaded { get => _isLoaded; set => SetProperty(ref _isLoaded, value); }

    private bool _isDevMode;
    public bool IsDevMode { get => _isDevMode; set => SetProperty(ref _isDevMode, value); }


    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService, HookService hookService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;

        hookService.Register<BattleStatusChangedArguments>(args =>
        {
            IsBattleTabEnabled = args.IsInBattle;
        });
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;

        if (e.SourcePageType == typeof(SettingsPage))
        {
            Selected = NavigationViewService.SettingsItem;
            return;
        }

        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
    }
}
