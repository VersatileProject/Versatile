using Microsoft.UI.Xaml.Controls;

using Versatile.ViewModels;

namespace Versatile.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel
    {
        get;
    }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
    }

    private void SelectedLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.RemovedItems.Count == 0)
        {
            return;
        };
    }

    private void SelectedTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.RemovedItems.Count == 0)
        {
            return;
        };
        ViewModel.SwitchTheme((int)((ComboBox)sender).SelectedValue);
    }
}
