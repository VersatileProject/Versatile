using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Versatile.Plays.Clients;
using Versatile.Plays.Servers;
using Versatile.Plays.ViewModels;

namespace Versatile.Plays.Views;

public sealed partial class ConnectionUserListControl : UserControl
{
    public ConnectionViewModel ViewModel
    {
        get => (ConnectionViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty
        = DependencyProperty.Register(
              "ViewModel",
              typeof(ConnectionViewModel),
              typeof(ConnectionUserListControl),
              new PropertyMetadata(null)
          );

    private ClientSideUser SelectedUser { get; set; }

    public ConnectionUserListControl()
    {
        this.InitializeComponent();

    }

    private void AppBarButton_Click(object sender, RoutedEventArgs e)
    {

    }

    private void ChallengeButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedUser.IsMe)
        {
            return;
        }

        ViewModel.SendChallenge(SelectedUser);
    }

    private void MenuFlyout_Opening(object sender, object e)
    {
        var mf = sender as MenuFlyout;
        var lvi = mf.Target as ListViewItem;
        SelectedUser = lvi.Content as ClientSideUser;
    }


}
