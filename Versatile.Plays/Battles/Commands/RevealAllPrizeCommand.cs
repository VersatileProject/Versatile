using Versatile.Plays.ViewModels;

namespace Versatile.Plays.Battles.Commands;

public class RevealAllPrizeCommand : BattleCommand
{
    public override void Execute(BattleCommandArguments e)
    {
        for (var i = 0; i < 6; i++)
        {
            var targetSlotkey = PlayerSlotKey.Prize1 + i;
            var targetSlot = e.Player.Slots[targetSlotkey];
            if (targetSlot.Cards.Count > 0)
            {
                foreach (var card in targetSlot.Cards)
                {
                    card.Status = BattleCardStatus.FaceUp;
                }
                e.UpdateSlot(targetSlotkey);
            }
        }

        e.WriteMessage("Battle/Command_RevealAllPrize", e.Player.Name);
    }
}
