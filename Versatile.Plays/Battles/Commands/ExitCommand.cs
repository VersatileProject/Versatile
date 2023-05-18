namespace Versatile.Plays.Battles.Commands;

public class ExitCommand : BattleCommand
{

    public override void Execute(BattleCommandArguments e)
    {
        e.WriteMessage("Battle/Command_Exit", e.Player.Name);
    }

}
