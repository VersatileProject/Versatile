using System.Linq;
using Versatile.Plays.ViewModels;

namespace Versatile.Plays.Battles.Commands;

public class ChooseCardsCommand : BattleCommand
{
    public PlayerSlotKey SlotKey { get; }
    public bool IsOpponent { get; }
    public int[] Indexes { get; }

    public ChooseCardsCommand(PlayerSlotKey slotKey, bool isOpponent, int[] indexes)
    {
        SlotKey = slotKey;
        IsOpponent = isOpponent;
        Indexes = indexes;
    }

    public override void Execute(BattleCommandArguments e)
    {
        var slot = IsOpponent ? e.Opponent.Slots[SlotKey] : e.Player.Slots[SlotKey];

        if (Indexes?.Length > 0)
        {
            var cards = Indexes.Select(i => slot.Cards[i]).ToArray();
            var cardnames = e.Battle.GetCardNameList(cards, Indexes);
            if (IsOpponent)
            {
                e.WriteMessage("Battle/Command_ChooseOpponentCard", e.Player.Name, slot.GetName(), cardnames);
            }
            else
            {
                e.WriteMessage("Battle/Command_ChooseCard", e.Player.Name, slot.GetName(), cardnames);
            }
        }
    }
}
