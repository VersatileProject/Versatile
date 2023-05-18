using Versatile.Plays.ViewModels;

namespace Versatile.Plays.Battles.Commands;

public class ViewAllPrizeCommand : BattleCommand
{
    public override void Execute(BattleCommandArguments e)
    {
        if (e.Player.IsMe)
        {
            for (var i = 0; i < 6; i++)
            {
                var targetSlotkey = PlayerSlotKey.Prize1 + i;
                var targetSlot = e.Player.Slots[targetSlotkey];
                foreach (var card in targetSlot.Cards)
                {
                    card.View();
                }
            }
        }

        e.WriteMessage("Battle/Command_ViewPrize", e.Player.Name);
    }
}
