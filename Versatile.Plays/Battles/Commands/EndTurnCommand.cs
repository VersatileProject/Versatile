using System;

namespace Versatile.Plays.Battles.Commands;

public class EndTurnCommand : BattleCommand
{
    public override void Execute(BattleCommandArguments e)
    {
        e.WriteMessage("Battle/Command_EndTurn", e.Player.Name);
        e.WriteMessage();

        e.UpdateProgress = BattleProgress.InBattle;

        if(e.Battle.Player1.IsMe || e.Battle.Player2.IsMe)
        {
            try
            {
                e.Battle.SavePlaymat();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
