using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Versatile.Common;
using Versatile.CommonUI;

namespace Versatile.Navigation;

public interface INavigationService
{
    event NavigatedEventHandler Navigated;

    bool CanGoBack
    {
        get;
    }

    Frame? Frame
    {
        get; set;
    }

    bool NavigateTo(PageKey pageKey, object? parameter = null, bool clearNavigation = false);

    bool GoBack();
    void Register<TPage, TViewModel>(PageKey Key);
    PageKey GetKeyFromPage(Type pageType);
}
