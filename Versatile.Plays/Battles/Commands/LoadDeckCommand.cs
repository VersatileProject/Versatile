using System.Collections.Generic;
using Versatile.Common;
using Versatile.Common.Cards;

namespace Versatile.Plays.Battles.Commands;

public class LoadDeckCommand : BattleCommand
{
    public record CardEntry(Card Card, int Quantity);

    public CardEntry[] Cards { get; }

    public LoadDeckCommand(CardEntry[] cards)
    {
        Cards = cards;
    }

    public override void Execute(BattleCommandArguments e)
    {
        var cards = new List<(Card, int)>();
        foreach (var (card, qty) in Cards)
        {
            cards.Add((card, qty));
        }

        e.Player.LoadDeck(cards.ToArray());

        e.WriteMessage("Battle/Command_LoadDeck", e.Player.Name);

        foreach (var slot in e.Player.Slots.Values)
        {
            e.UpdateSlot(slot.Type);
        }

        // todo: check deck

        if (e.IsMe)
        {
            e.UpdateProgress = BattleProgress.DeckLoaded;
        }
    }
}
