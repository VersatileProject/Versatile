using Microsoft.UI.Xaml.Controls;
using Versatile.Common.Cards;
using Versatile.ViewModels;

namespace Versatile.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }

}
