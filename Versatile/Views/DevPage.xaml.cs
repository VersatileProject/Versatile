using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace Versatile.Views;

public sealed partial class DevPage : Page
{
    public DevPage()
    {
        this.InitializeComponent();
    }

}

public class DevViewModel : ObservableRecipient
{
    public DevViewModel()
    {
    }

}
