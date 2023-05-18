namespace Versatile.Plays.Battles.Commands;

public class BeginTurnCommand : BattleCommand
{
    public override void Execute(BattleCommandArguments e)
    {
        e.Player.IsTurnBegan = true;
        e.WriteMessage();
        e.WriteMessage("Battle/Command_BeginTurn", e.Player.Name);

        e.UpdateProgress = BattleProgress.InBattle;
    }
}
