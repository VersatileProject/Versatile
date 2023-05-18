using System;
using System.Collections.Generic;
using System.Linq;
using Versatile.Plays.ViewModels;

namespace Versatile.Plays.Battles.Commands;

public class ShuffleSlotCommand : BattleCommand
{
    public PlayerSlotKey SlotKey { get; }

    public ShuffleSlotCommand(PlayerSlotKey slotKey)
    {
        SlotKey = slotKey;
    }

    public override void Execute(BattleCommandArguments e)
    {
        var slot = e.Player.Slots[SlotKey];
        BattleCardStatus? status;
        if (SlotKey == PlayerSlotKey.Deck)
        {
            status = BattleCardStatus.Unknown;
        }
        else if (SlotKey == PlayerSlotKey.Hand)
        {
            status = e.IsMe ? BattleCardStatus.Self : BattleCardStatus.Unknown;
        }
        else if (SlotKey == PlayerSlotKey.Hide)
        {
            status = BattleCardStatus.Unknown;
        }
        else
        {
            throw new NotSupportedException($"Not supported to shuffle {SlotKey}.");
        }
        e.Battle.Shuffle(slot.Cards, e.Random, status);
        e.WriteMessage("Battle/Command_ShuffleSlot", e.Player.Name, slot.GetName());
        e.UpdateSlot(SlotKey);
    }

}
