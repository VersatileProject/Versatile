using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Versatile.Common.Cards;
using Versatile.Plays.Battles;

namespace Versatile.Plays.Views;

public class CardInfoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var card = (BattleCard)value;
        if (card.Status is BattleCardStatus.Self or BattleCardStatus.FaceUp)
        {
            return card.Data.Name;
        }
        else
        {
            return "?????";
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

