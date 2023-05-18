using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Versatile.Activation;
using Versatile.Browsers.Cards;
using Versatile.Browsers.Decks;
using Versatile.Common;
using Versatile.Common.Cards;
using Versatile.CommonUI;
using Versatile.CommonUI.Services;
using Versatile.Contracts.Services;
using Versatile.Core.Contracts.Services;
using Versatile.Core.Helpers;
using Versatile.Core.Services;
using Versatile.Core.Settings;
using Versatile.Localization.Services;
using Versatile.Navigation;
using Versatile.Plays.Services;
using Versatile.Plays.ViewModels;
using Versatile.Plays.Views;
using Versatile.Services;
using Versatile.ViewModels;
using Versatile.Views;
using Windows.ApplicationModel;

namespace Versatile;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if (GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return (T)service;
    }

    public static object GetService(Type type)
    {
        return (App.Current as App)!.Host.Services.GetService(type);
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers


            // Services
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            //services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Core Services
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<LocalizationService>();
            services.AddSingleton<HookService>();

            // Views and ViewModels
            services.AddSingleton<SettingsViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddSingleton<MainViewModel>();
            services.AddTransient<MainPage>();
            services.AddSingleton<ShellPage>();
            services.AddSingleton<ShellViewModel>();

            services.AddSingleton<CardDataBaseService>();
            services.AddSingleton<DialogService>();
            services.AddSingleton<WindowsService>();

            services.AddSingleton<CardBrowserViewModel>();
            services.AddSingleton<CardBrowserPage>();
            services.AddSingleton<CardProductViewModel>();
            services.AddSingleton<CardSearchViewModel>();
            services.AddSingleton<DeckEditorViewModel>();
            services.AddSingleton<DeckEditorPage>();

            services.AddSingleton<ConnectionViewModel>();
            services.AddSingleton<ConnectionPage>();
            services.AddSingleton<BattleViewModel>();
            services.AddSingleton<BattlePage>();
            services.AddSingleton<BattleService>();

            services.AddTransient<Window>(_ => MainWindow);

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        UnhandledException += App_UnhandledException;
        
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    private static Version GetVersion()
    {
        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            return new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            return Assembly.GetExecutingAssembly().GetName().Version!;
        }
    }

    // https://github.com/dotnet/maui/issues/12246
    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        var cmdargs = Environment.GetCommandLineArgs();

        var mre = new ManualResetEvent(false);

        BeforeLoad(cmdargs);

        _ = Task.Run(() => {
            Load(cmdargs);

            mre.WaitOne();

            MainWindow.DispatcherQueue.TryEnqueue(() => AfterLoad(cmdargs));
        });

        await GetService<IActivationService>().ActivateAsync(args);

        mre.Set();
    }

    private void BeforeLoad(string[] cmdargs)
    {
        var nav = GetService<INavigationService>();
        nav.Register<MainViewModel, MainPage>(PageKey.Main);
        nav.Register<SettingsViewModel, SettingsPage>(PageKey.Settings);
        nav.Register<CardBrowserViewModel, CardBrowserPage>(PageKey.Card);
        nav.Register<DevViewModel, DevPage>(PageKey.Debug);
        nav.Register<DeckEditorViewModel, DeckEditorPage>(PageKey.Deck);
        nav.Register<ConnectionViewModel, ConnectionPage>(PageKey.Connection);
        nav.Register<BattleViewModel, BattlePage>(PageKey.Battle);

        if (cmdargs.Contains("/dev"))
        {
            VersatileApp.DevMode = true;
        }
    }

    private void Load(string[] cmdargs)
    {
        VersatileApp.OnGetService += GetService;
        VersatileApp.NavigateTo += (key) => GetService<INavigationService>().NavigateTo(key);
        VersatileApp.DoLocalize += (name) => GetService<LocalizationService>().GetString(name);
        VersatileApp.Version = GetVersion();

        var cardService = App.GetService<CardDataBaseService>();
    }

    private void AfterLoad(string[] cmdargs)
    {
        var cardService = App.GetService<CardDataBaseService>();

        var shellVM = GetService<ShellViewModel>();
        shellVM.IsLoaded = true;
        shellVM.IsDevMode = VersatileApp.DevMode;

        var mainVM = GetService<MainViewModel>();
        var version = VersatileApp.Version;
        mainVM.VersionDescription = $"Version {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        //mainVM.DatabaseDescription = $"Database update at 1970/01/01 00:00:00";
        mainVM.CardLoadedMessage = $"Loaded {cardService?.CardCount} cards in {cardService.LoadingDuration.TotalMilliseconds:#}ms.";
        mainVM.IsLoaded = true;

        if (cmdargs.Contains("/localgame"))
        {
            VersatileApp.NavigateTo(PageKey.Connection);
            GetService<ConnectionViewModel>().LaunchLocalGameCommand.Execute(null);
        }
    }

}
