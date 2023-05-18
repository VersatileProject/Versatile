using System.Collections.Generic;
using System.Linq;
using Versatile.Plays.ViewModels;

namespace Versatile.Plays.Battles.Commands;

public class ShuffleAllPrizeCommand : BattleCommand
{
    public override void Execute(BattleCommandArguments e)
    {
        var list = new List<BattleCard>();

        for (var i = 0; i < 6; i++)
        {
            var targetSlotkey = PlayerSlotKey.Prize1 + i;
            var targetSlot = e.Player.Slots[targetSlotkey];
            list.AddRange(targetSlot.Cards);
            targetSlot.Cards.Clear();
        }
        e.Battle.Shuffle(list, e.Random, BattleCardStatus.Unknown);

        for (var i = 0; i < 6; i++)
        {
            if (i >= list.Count) break;
            var targetSlotkey = PlayerSlotKey.Prize1 + i;
            var targetSlot = e.Player.Slots[targetSlotkey];
            targetSlot.Cards.Add(list[i]);
        }

        if(list.Count > 6)
        {
            var targetSlot = e.Player.Slots[PlayerSlotKey.Prize1];
            targetSlot.Cards.AddRange(list.Skip(6));
        }

        for (var i = 0; i < 6; i++)
        {
            var targetSlotkey = PlayerSlotKey.Prize1 + i;
            e.UpdateSlot(targetSlotkey);
        }

        e.WriteMessage("Battle/Command_ShufflePrize", e.Player.Name);
    }
}
