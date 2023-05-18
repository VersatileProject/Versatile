using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Versatile.Common;
using Versatile.Common.Cards;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Markup;

namespace Versatile.CommonUI.Views;

[ContentProperty(Name = "MainContent")]
public sealed partial class CardInfoPanel : UserControl
{
    public static DependencyProperty MainContentProperty = DependencyProperty.Register("MainContent", typeof(object), typeof(CardInfoPanel), null);

    public object MainContent { get => GetValue(MainContentProperty); set => SetValue(MainContentProperty, value); }

    public bool IsDevMode { get; set; }

    public CardInfoPanel()
    {
        this.InitializeComponent();

        IsDevMode = Debugger.IsAttached;
    }

    private void CardNameMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {

    }

    private void CardTextFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        if(e.OriginalSource is TextBlock tb)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(tb.Text);
            Clipboard.SetContent(dataPackage);
        }
    }
}

public class TagTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var tag = (string)value;
        var text = VersatileApp.Localize($"Card/Tag_{tag}");
        if (string.IsNullOrEmpty(text))
        {
            return tag;
        }
        else
        {
            return text;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

}
public class RulesToAbilitiesConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if(value is CardRule[] rules)
        {
            return rules.Select(x => new CardAbility()
            {
                Type = AbilityType.Rule,
                Name = x.Name,
                Text = x.Text,
            }).ToArray();
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

}