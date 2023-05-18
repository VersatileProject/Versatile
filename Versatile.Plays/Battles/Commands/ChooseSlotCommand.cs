using Versatile.Plays.ViewModels;

namespace Versatile.Plays.Battles.Commands;

public class ChooseSlotCommand : BattleCommand
{
    public PlayerSlotKey SlotKey { get; }
    public bool IsOpponent { get; }

    public ChooseSlotCommand(PlayerSlotKey slotKey, bool isOpponent)
    {
        SlotKey = slotKey;
        IsOpponent = isOpponent;
    }

    public override void Execute(BattleCommandArguments e)
    {
        var slot = IsOpponent ? e.Opponent.Slots[SlotKey] : e.Player.Slots[SlotKey];

        if (IsOpponent)
        {
            e.WriteMessage("Battle/Command_ChooseOpponentSlot", e.Player.Name, slot.GetName());
        }
        else
        {
            e.WriteMessage("Battle/Command_ChooseSlot", e.Player.Name, slot.GetName());
        }
    }
}
