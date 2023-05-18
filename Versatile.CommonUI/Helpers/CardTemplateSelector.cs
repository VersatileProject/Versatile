using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Versatile.Common.Cards;

namespace Versatile.CommonUI.Helpers;

public class CardTemplateSelector : DataTemplateSelector
{
    public DataTemplate Pokemon { get; set; }
    public DataTemplate Trainer { get; set; }
    public DataTemplate Energy { get; set; }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        if (item is not Card card) return null;

        return card.Type switch
        {
            CardType.Pokemon => Pokemon,
            CardType.Trainer => Trainer,
            CardType.Energy => Energy,
            _ => null,
        };
    }
}

public class CardAbilityTemplateSelector : DataTemplateSelector
{
    public DataTemplate Ability { get; set; }
    public DataTemplate Attack { get; set; }
    public DataTemplate Effect { get; set; }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        if (item is not CardAbility ability)
        {
            return null;
        }
        else if (ability.IsAttack)
        {
            return Attack;
        }
        else if (string.IsNullOrEmpty(ability.Name))
        {
            return Effect;
        }
        else
        {
            return Ability;
        }
    }
}
