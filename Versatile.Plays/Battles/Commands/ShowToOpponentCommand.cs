using System;
using System.Text.Json.Serialization;
using Versatile.Plays.ViewModels;

namespace Versatile.Plays.Battles.Commands;

public class ShowToOpponentCommand : BattleCommand
{
    public PlayerSlotKey SlotKey { get; }
    public int[] Indexes { get; }

    public ShowToOpponentCommand(PlayerSlotKey slotKey)
    {
        SlotKey = slotKey;
        Indexes = null;
    }

    [JsonConstructor]
    public ShowToOpponentCommand(PlayerSlotKey slotKey, int[] indexes)
    {
        SlotKey = slotKey;
        Indexes = indexes;
    }

    public override void Execute(BattleCommandArguments e)
    {
        var slot = e.Player.Slots[SlotKey];

        if (Indexes?.Length > 0)
        {
            if (e.Opponent.IsMe)
            {
                foreach (var index in Indexes)
                {
                    slot.Cards[index].View();
                }
            }
            var cardnames = e.Battle.GetCardNameList(null, Indexes);
            e.WriteMessage("Battle/Command_ShowToOpponent", e.Player.Name, slot.GetName(), cardnames);
        }
        else
        {
            if (e.Opponent.IsMe)
            {
                foreach (var card in slot.Cards)
                {
                    card.View();
                }
            }
            e.WriteMessage("Battle/Command_ShowSlotToOpponent", e.Player.Name, slot.GetName());
        }

        e.UpdateSlot(SlotKey);
    }
}
