using CommunityToolkit.Common;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Versatile.Common;
using Versatile.Core.Services;
using Versatile.Core.Settings;
using Versatile.Helpers;
using Versatile.Navigation;

namespace Versatile;

public sealed partial class MainWindow : WindowEx
{
    public MainWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;
        Title = "Versatile";
        CenterToScreen();

        AppWindow.Closing += AppWindow_Closing;
    }

    private async void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        args.Cancel = true;

        var closingArgs = new MainWindowClosingArguments()
        {
            Cancel = false,
        };
        await App.GetService<HookService>().Raise(closingArgs, x => !x.Cancel);
        
        if (closingArgs.Cancel == true)
        {
            args.Cancel = true;
            return;
        }

        var closedArgs = new MainWindowClosedArguments();
        await App.GetService<HookService>().Raise(closedArgs);

        var settings = VersatileApp.GetService<ILocalSettingsService>();
        settings.SaveSetting();

        sender.Closing -= AppWindow_Closing;
        Application.Current.Exit();
    }

    private void CenterToScreen()
    {
        var displayArea = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest);
        if (displayArea is not null)
        {
            var x = (int)((displayArea.WorkArea.Width - this.Width) / 2);
            var y = (int)((displayArea.WorkArea.Height - this.Height) / 2);
            AppWindow.Move(new(x, y));
        }
    }

}
