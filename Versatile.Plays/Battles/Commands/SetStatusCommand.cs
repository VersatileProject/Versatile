using System;
using System.Text.Json.Serialization;
using Versatile.Common;
using Versatile.Plays.ViewModels;

namespace Versatile.Plays.Battles.Commands;

public class SetStatusCommand : BattleCommand
{
    public PlayerSlotKey SlotKey { get; }
    public PokemonStatus? Status { get; }
    public bool? IsPoisoned { get; }
    public bool? IsBurned { get; }

    public SetStatusCommand(PlayerSlotKey slotKey, PokemonStatus status)
    {
        SlotKey = slotKey;
        Status = status;
    }

    [JsonConstructor]
    public SetStatusCommand(PlayerSlotKey slotKey, PokemonStatus? status = null, bool? isPoisoned = null, bool? isBurned = null)
    {
        SlotKey = slotKey;
        if(status.HasValue) Status = status.Value;
        IsPoisoned = isPoisoned;
        IsBurned = isBurned;
    }

    public override void Execute(BattleCommandArguments e)
    {
        if (SlotKey != PlayerSlotKey.Active)
        {
            throw new ArgumentOutOfRangeException($"{SlotKey} is not {PlayerSlotKey.Active}.");
        }

        var slot = e.Player.Slots[SlotKey];
        var oldSlotName = slot.GetName();
        if (Status.HasValue)
        {
            slot.Pokemon.Rotation = Status.Value;
            var statusname = VersatileApp.Localize(Status.Value, "Battle");
            e.WriteMessage("Battle/Command_SetStatus", e.Player.Name, oldSlotName, statusname);
        }
        else if (IsPoisoned == true)
        {
            slot.Pokemon.IsPoisoned = true;
            e.WriteMessage("Battle/Command_SetPoisoned", e.Player.Name, oldSlotName);
        }
        else if (IsPoisoned == false)
        {
            slot.Pokemon.IsPoisoned = false;
            e.WriteMessage("Battle/Command_UnsetPoisoned", e.Player.Name, oldSlotName);
        }
        else if (IsBurned == true)
        {
            slot.Pokemon.IsBurned = true;
            e.WriteMessage("Battle/Command_SetBurned", e.Player.Name, oldSlotName);
        }
        else if (IsBurned == false)
        {
            slot.Pokemon.IsBurned = false;
            e.WriteMessage("Battle/Command_UnsetBurned", e.Player.Name, oldSlotName);
        }
        e.UpdateSlot(SlotKey);
    }
}
