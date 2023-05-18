namespace Versatile.Plays.Battles.Commands;

public class ForfeitCommand : BattleCommand
{
    public override void Execute(BattleCommandArguments e)
    {
        e.WriteMessage("Battle/Command_Forfeit", e.Player.Name);
    }
}
