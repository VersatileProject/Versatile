using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Versatile.Common.Cards;

namespace Versatile.Browsers.Decks;

public sealed partial class CardPanel : UserControl
{
    public event Action<Card> CardSelected;

    public Card[] Cards
    {
        get => (Card[])GetValue(CardsProperty);
        set => SetValue(CardsProperty, value);
    }

    public static readonly DependencyProperty CardsProperty
        = DependencyProperty.Register(
              "Cards",
              typeof(Card[]),
              typeof(CardPanel),
              new PropertyMetadata(null)
          );

    public int CardSpacing
    {
        get => (int)GetValue(CardSpacingProperty);
        set => SetValue(CardSpacingProperty, value);
    }

    public static readonly DependencyProperty CardSpacingProperty
        = DependencyProperty.Register(
              "CardSpacing",
              typeof(int),
              typeof(CardPanel),
              new PropertyMetadata(0)
          );

    public int CardHeight
    {
        get => (int)GetValue(CardHeightProperty);
        set => SetValue(CardHeightProperty, value);
    }

    public static readonly DependencyProperty CardHeightProperty
        = DependencyProperty.Register(
              "CardHeight",
              typeof(int),
              typeof(CardPanel),
              new PropertyMetadata(0)
          );

    public CardPanel()
    {
        this.InitializeComponent();
    }

    private void Image_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        if(sender is Image image && image.DataContext is Card card)
        {
            CardSelected?.Invoke(card);
        }
    }

}

public class CardSpacingToMarginConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => new Thickness(-(int)value, 0, 0, 0);

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class CardSpacingToPaddingConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => new Thickness((int)value, 0, 0, 0);

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}