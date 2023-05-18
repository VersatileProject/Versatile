using System.Linq;
using System.Text.Json.Serialization;
using Versatile.Plays.ViewModels;

namespace Versatile.Plays.Battles.Commands;

public class ViewCardsCommand : BattleCommand
{
    public PlayerSlotKey SlotKey { get; }
    public int[] Indexes { get; }

    public ViewCardsCommand(PlayerSlotKey slotKey)
    {
        SlotKey = slotKey;
    }

    [JsonConstructor]
    public ViewCardsCommand(PlayerSlotKey slotKey, int[] indexes)
    {
        SlotKey = slotKey;
        Indexes = indexes;
    }

    public override void Execute(BattleCommandArguments e)
    {
        var slot = e.Player.Slots[SlotKey];

        if (Indexes?.Length > 0)
        {
            e.Battle.ViewCards(slot, Indexes);
            var cardnames = string.Join(", ", Indexes.Select(i => i+1));
            e.WriteMessage("Battle/Command_ViewCards", e.Player.Name, slot.GetName(), cardnames);
        }
        else
        {
            e.Battle.ViewCards(slot, null);
            e.WriteMessage("Battle/Command_ViewAllCards", e.Player.Name, slot.GetName());
        }

        e.UpdateSlot(SlotKey);
    }
}
