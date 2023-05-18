using System;
using Microsoft.UI.Xaml.Data;
using Versatile.Common;

namespace Versatile.CommonUI.Converters;

public class EnumToLocalizedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var path = (string)parameter;
        var typename = value.GetType().Name;
        var enumtext = value.ToString();

        return VersatileApp.Localize($"{path}/{typename}_{enumtext}");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

}
