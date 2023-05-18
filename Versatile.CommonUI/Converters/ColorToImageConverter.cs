using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using Versatile.Common.Cards;

namespace Versatile.CommonUI.Converters;

public class ColorToImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not CardColor color)
        {
            return null;
        }
        else
        {
            var uri = new Uri($"ms-appx:///Versatile.CommonUI/Assets/Colors/Icons/{color}.png");
            var bitmapImage = new BitmapImage(uri);
            return bitmapImage;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
