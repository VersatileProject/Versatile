using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using Versatile.Common.Cards;

namespace Versatile.CommonUI.Converters;

public class CardIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not Card card)
        {
            return new BitmapImage(new Uri("ms-appx:///Versatile.CommonUI/Assets/CardIcons/unknown.png"));
        }
        else
        {
            return card.Type switch
            {
                CardType.Pokemon => new BitmapImage(new Uri("ms-appx:///Versatile.CommonUI/Assets/CardIcons/pokemon.png")),
                CardType.Trainer => card.Trainer?.Type switch
                {
                    TrainerCardType.Item => new BitmapImage(new Uri("ms-appx:///Versatile.CommonUI/Assets/CardIcons/item.png")),
                    TrainerCardType.PokemonTool => new BitmapImage(new Uri("ms-appx:///Versatile.CommonUI/Assets/CardIcons/pokemontool.png")),
                    TrainerCardType.Supporter => new BitmapImage(new Uri("ms-appx:///Versatile.CommonUI/Assets/CardIcons/supporter.png")),
                    TrainerCardType.Stadium => new BitmapImage(new Uri("ms-appx:///Versatile.CommonUI/Assets/CardIcons/stadium.png")),
                    _ => new BitmapImage(new Uri("ms-appx:///Versatile.CommonUI/Assets/CardIcons/unknown.png")),
                },
                CardType.Energy => card.Energy?.Type switch
                {
                    EnergyCardType.Basic => new BitmapImage(new Uri("ms-appx:///Versatile.CommonUI/Assets/CardIcons/energy.png")),
                    EnergyCardType.Special => new BitmapImage(new Uri("ms-appx:///Versatile.CommonUI/Assets/CardIcons/energy2.png")),
                    _ => new BitmapImage(new Uri("ms-appx:///Versatile.CommonUI/Assets/CardIcons/unknown.png")),
                },
                _ => new BitmapImage(new Uri("ms-appx:///Versatile.CommonUI/Assets/CardIcons/unknown.png")),
            };
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
