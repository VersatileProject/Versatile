using System.Reflection;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Versatile.Common.Cards;
using Versatile.Contracts.Services;
using Versatile.Core.Helpers;
using Windows.ApplicationModel;
using Windows.Globalization;

namespace Versatile.ViewModels;

// https://github.com/microsoft/microsoft-ui-xaml/issues/5922
// https://github.com/microsoft/WindowsAppSDK/issues/1687
public class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;
    private string _versionDescription;

    public Dictionary<int, string> ThemeSource { get; }
    private int _selectedTheme;
    public int SelectedTheme { get => _selectedTheme; set => SetProperty(ref _selectedTheme, value); }

    public Dictionary<string, string> LanguageSource { get; }

    public SettingsViewModel(IThemeSelectorService themeSelectorService)
    {
        _themeSelectorService = themeSelectorService;
        _selectedTheme = (int)_themeSelectorService.Theme;

        ThemeSource = new()
        {
            {(int)ElementTheme.Default, ElementTheme.Default.ToString()},
            {(int)ElementTheme.Light, ElementTheme.Light.ToString()},
            {(int)ElementTheme.Dark, ElementTheme.Dark.ToString()},
        };

        LanguageSource = new()
        {
            {"", "Default"},
            {"en-US", "English"},
            {"ja-JP", "日本語"},
            {"zh-CN", "简体中文"},
        };
    }

    public async void SwitchTheme(int value)
    {
        if (SelectedTheme != value)
        {
            SelectedTheme = value;
            await _themeSelectorService.SetThemeAsync((ElementTheme)value);
        }
    }

}