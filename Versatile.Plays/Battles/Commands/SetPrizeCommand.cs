using System;
using System.Text.Json.Serialization;
using Versatile.Plays.ViewModels;

namespace Versatile.Plays.Battles.Commands;

public class SetPrizeCommand : BattleCommand
{
    public int Count { get; }

    public SetPrizeCommand()
    {
        Count = 6;
    }

    [JsonConstructor]
    public SetPrizeCommand(int count)
    {
        Count = count;
    }

    public override void Execute(BattleCommandArguments e)
    {
        if (Count == 0)
        {
            return;
        }
        if (Count < 1 || Count > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(Count), $"{nameof(Count)} must be between 1 and 6.");
        }

        var sourceSlot = e.Player.Slots[PlayerSlotKey.Deck];
        var count = Math.Min(sourceSlot.Cards.Count, Count);
        for (var i = 0; i < count; i++)
        {
            if (sourceSlot.IsEmpty) break;
            var targetSlotkey = PlayerSlotKey.Prize1 + i;
            var targetSlot = e.Player.Slots[targetSlotkey];
            e.Battle.MoveCards(sourceSlot, new[] { 0 }, targetSlot, false, null);
            e.UpdateSlot(targetSlotkey);
        }
        e.UpdateSlot(PlayerSlotKey.Deck);

        e.WriteMessage("Battle/Command_SetPrize", e.Player.Name, count);
    }
}
