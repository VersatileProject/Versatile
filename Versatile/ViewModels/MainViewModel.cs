using CommunityToolkit.Mvvm.ComponentModel;

namespace Versatile.ViewModels;

public class MainViewModel : ObservableRecipient
{
    private string _versionDescription;
    public string VersionDescription { get => _versionDescription; set => SetProperty(ref _versionDescription, value); }

    private string _databaseDescription;
    public string DatabaseDescription { get => _databaseDescription; set => SetProperty(ref _databaseDescription, value); }

    private string _cardLoadedMessage;
    public string CardLoadedMessage { get => _cardLoadedMessage; set => SetProperty(ref _cardLoadedMessage, value); }

    private bool _isLoaded;
    public bool IsLoaded { get => _isLoaded; set => SetProperty(ref _isLoaded, value); }

    public MainViewModel()
    {
    }

}
