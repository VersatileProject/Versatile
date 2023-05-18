using System;
using Microsoft.UI.Xaml.Data;

namespace Versatile.CommonUI.Converters;

public class EqualConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (value != null && value.Equals(parameter)) || (value == null && parameter == null);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

}
