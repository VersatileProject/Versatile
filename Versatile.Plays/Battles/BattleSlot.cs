using System.Collections.Generic;
using Versatile.Common;
using Versatile.Plays.Battles;
using Versatile.Plays.ViewModels;

namespace Versatile.Plays.Battles;

public class BattleSlot
{
    public PlayerSlotKey Type { get; set; }
    public BattlePlayer Player { get; set; }
    public List<BattleCard> Cards { get; set; } = new();
    public BattlePokemon Pokemon { get; set; } = new();
    public bool IsEmpty => Cards.Count == 0;

    public BattleSlot(PlayerSlotKey type)
    {
        Type = type;
    }

    public string GetName()
    {
        var name = VersatileApp.Localize(Type, "Battle");
        if ((Type.IsPokemon() || Type == PlayerSlotKey.Stadium) && Cards.Count > 0)
        {
            name += "(" + Cards[0].GetCardName() + ")";
        }
        else if (Type is PlayerSlotKey.Hand or PlayerSlotKey.Deck or PlayerSlotKey.DiscardPile or PlayerSlotKey.LostZone)
        {
            name += "(" + Cards.Count + ")";
        }
        else if (Type.IsPrize())
        {
            name += "(" + Player.GetPrizeCount() + ")";
        }
        return name;
    }

    public void CheckPokemon()
    {
        if (Cards.Count == 0)
        {
            Pokemon = new();
        }
        else if (Type.IsBench())
        {
            Pokemon.IsBurned = false;
            Pokemon.IsPoisoned = false;
            Pokemon.Rotation = PokemonStatus.Normal;
        }
    }
}
