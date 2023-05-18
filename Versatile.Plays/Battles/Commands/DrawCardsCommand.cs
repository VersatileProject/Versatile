using System;
using System.Linq;
using Versatile.Plays.ViewModels;

namespace Versatile.Plays.Battles.Commands;

public class DrawCardsCommand : BattleCommand
{
    public int Count { get; }

    public DrawCardsCommand(int count)
    {
        Count = count;
    }

    public override void Execute(BattleCommandArguments e)
    {
        if (Count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Count));
        }

        var deck = e.Player.Slots[PlayerSlotKey.Deck];
        var count = Math.Min(Count, deck.Cards.Count);

        if (count == 0)
        {
            e.WriteMessage("Battle/Command_DrawCardsFailed", e.Player.Name);
        }
        else
        {
            var hands = e.Player.Slots[PlayerSlotKey.Hand];
            var status = e.IsMe ? BattleCardStatus.Self : BattleCardStatus.Unknown;
            var indexes = Enumerable.Range(0, count).ToArray();
            e.Battle.MoveCards(deck, indexes, hands, true, status);
            e.WriteMessage("Battle/Command_DrawCards", e.Player.Name, count);
            e.UpdateSlot(PlayerSlotKey.Deck);
            e.UpdateSlot(PlayerSlotKey.Hand);
        }

    }
}
