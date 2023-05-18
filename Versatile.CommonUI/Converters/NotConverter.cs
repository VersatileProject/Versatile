using System;
using Microsoft.UI.Xaml.Data;

namespace Versatile.CommonUI.Converters;

public class NotConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return !(bool?)value ?? true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return !(value as bool?);
    }
}
