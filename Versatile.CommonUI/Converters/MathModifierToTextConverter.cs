using System;
using Microsoft.UI.Xaml.Data;
using Versatile.Common.Cards;

namespace Versatile.CommonUI.Converters;

public class MathModifierToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {

        return value switch
        {
            MathModifier.Plus => "＋",
            MathModifier.Minus => "―",
            MathModifier.Multiple => "×",
            _ => "",
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
