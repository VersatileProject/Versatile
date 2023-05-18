using System.Text.Json.Serialization;
using Versatile.Plays.ViewModels;

namespace Versatile.Plays.Battles.Commands;

public class RevealCardsCommand : BattleCommand
{
    public PlayerSlotKey SlotKey { get; }
    public int[] Indexes { get; }

    public RevealCardsCommand(PlayerSlotKey slotKey)
    {
        SlotKey = slotKey;
        Indexes = null;
    }

    [JsonConstructor]
    public RevealCardsCommand(PlayerSlotKey slotKey, int[] indexes)
    {
        SlotKey = slotKey;
        Indexes = indexes;
    }

    public override void Execute(BattleCommandArguments e)
    {
        var slot = e.Player.Slots[SlotKey];

        if (Indexes?.Length > 0)
        {
            var cards = e.Battle.RevealCards(slot, Indexes);
            var cardnames = e.Battle.GetCardNameList(cards, Indexes);
            e.WriteMessage("Battle/Command_RevealCards", e.Player.Name, slot.GetName(), cardnames);
        }
        else
        {
            e.Battle.ViewCards(slot, null);
            e.WriteMessage("Battle/Command_RevealAllCards", e.Player.Name, slot.GetName());
            var text2 = e.Battle.GetCardNameList(slot.Cards);
            e.WriteSubMessage(text2);
        }

        e.UpdateSlot(SlotKey);
    }
}
