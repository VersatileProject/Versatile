using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Versatile.Common;

namespace Versatile.Navigation;

public class NavigationHelper
{
    public static PageKey GetNavigateTo(NavigationViewItem item) => (PageKey)item.GetValue(NavigateToProperty);

    public static void SetNavigateTo(NavigationViewItem item, PageKey value) => item.SetValue(NavigateToProperty, value);

    public static readonly DependencyProperty NavigateToProperty =
        DependencyProperty.RegisterAttached("NavigateTo", typeof(PageKey), typeof(NavigationHelper), new PropertyMetadata(null));

}
