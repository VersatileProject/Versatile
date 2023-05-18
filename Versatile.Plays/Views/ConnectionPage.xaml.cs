using Microsoft.UI.Xaml.Controls;
using Versatile.Common;
using Versatile.Networks.Services;
using Versatile.Plays.Servers;
using Versatile.Plays.ViewModels;

namespace Versatile.Plays.Views;

public sealed partial class ConnectionPage : Page
{
    private ConnectionViewModel ViewModel { get; set; }

    private string UsernamePlaceholder { get; set; }

    public ConnectionPage()
    {
        ViewModel = VersatileApp.GetService<ConnectionViewModel>();
        ViewModel.LogReceived += OnLogReceived;

        UsernamePlaceholder = ConnectionViewModel.GetDefaultNickname();

        this.InitializeComponent();
    }

    private void OnLogReceived(LogRun[] obj)
    {
        RoomLogBox.AppendText(obj);
    }

    private void ChatPanel_MessageSended(object sender, ChatMessageEventArgs e)
    {
        var msg = new ClientSayCommand()
        {
            Text = e.Message,
        };
        ViewModel.ClientSend(msg);
    }

    private void UserCommandBarFlyout_Opening(object sender, object e)
    {
        
    }

}

public enum RunningMode
{
    Offline,
    Local,
    ServerClient,
    Client,
}
