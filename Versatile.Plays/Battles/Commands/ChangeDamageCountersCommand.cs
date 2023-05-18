using System;
using System.Text.Json.Serialization;
using Versatile.Plays.ViewModels;

namespace Versatile.Plays.Battles.Commands;

public class ChangeDamageCountersCommand : BattleCommand
{
    public PlayerSlotKey SlotKey { get; }
    public int DamageCounters { get; }

    public ChangeDamageCountersCommand(PlayerSlotKey slotKey, int damageCounters)
    {
        SlotKey = slotKey;
        DamageCounters = damageCounters;
    }

    public override void Execute(BattleCommandArguments e)
    {
        if (!SlotKey.IsPokemon())
        {
            throw new ArgumentOutOfRangeException($"{SlotKey} is not Pokemon.");
        }
        if (DamageCounters < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(DamageCounters), $"The damage counters must be equal or greater than 0.");
        }

        var slot = e.Player.Slots[SlotKey];
        var oldDC = slot.Pokemon.DamageCounters;
        var newDC = DamageCounters;

        slot.Pokemon.DamageCounters = newDC;
        if (newDC > oldDC)
        {
            e.WriteMessage("Battle/Command_PutDC", e.Player.Name, newDC - oldDC, slot.GetName());
        }
        else if (newDC < oldDC)
        {
            e.WriteMessage("Battle/Command_RemoveDC", e.Player.Name, oldDC - newDC, slot.GetName());
        }
        else
        {
            //throw new ArgumentOutOfRangeException(nameof(DamageCounters), $"The new value of damage counters must be different from the old one.");
        }

        e.UpdateSlot(SlotKey);
    }
}
