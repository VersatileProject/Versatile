namespace Versatile.Plays.Battles.Commands;

public class RollDieCommand : BattleCommand
{
    public override void Execute(BattleCommandArguments e)
    {
        var result = e.Random.Next(1, 7);
        e.WriteMessage("Battle/Command_RollDie", e.Player.Name, result);
    }
}
