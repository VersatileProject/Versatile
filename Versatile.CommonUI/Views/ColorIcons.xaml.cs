using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Versatile.Common.Cards;

namespace Versatile.CommonUI.Views;

public sealed partial class ColorIcons : UserControl
{
    public static readonly DependencyProperty ColorsProperty = DependencyProperty.Register(
        "Colors",
        typeof(object),
        typeof(ColorIcons),
        new PropertyMetadata(null)
        );

    public object Colors
    {
        get => GetValue(ColorsProperty);
        set => SetValue(ColorsProperty, value);
    }

    public static readonly DependencyProperty ShowVoidProperty = DependencyProperty.Register(
        "ShowVoid",
        typeof(bool),
        typeof(ColorIcons),
        new PropertyMetadata(false)
        );

    public bool ShowVoid 
    { 
        get => (bool)GetValue(ShowVoidProperty);
        set => SetValue(ShowVoidProperty, value);
    }

    public ColorIcons()
    {
        this.InitializeComponent();
    }

    private void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        switch (Colors)
        {
            case null when ShowVoid:
            case CardColor[] colors when ShowVoid && colors.Length == 0:
                IconContainer.ItemsSource = new CardColor[] { CardColor.Void };
                break;
            case CardColor color:
                IconContainer.ItemsSource = new CardColor[] { color };
                break;
            case IEnumerable<CardColor> colors:
                IconContainer.ItemsSource = colors;
                break;
            case int number:
                IconContainer.ItemsSource = Enumerable.Repeat(CardColor.Colorless, number);
                break;
            default:
                IconContainer.ItemsSource = null;
                break;
        }
    }
}
