namespace Versatile.Plays.Battles.Commands;

public class FlipCoinCommand : BattleCommand
{
    public override void Execute(BattleCommandArguments e)
    {
        var result = e.Random.Next(0, 2) == 0;
        if (result)
        {
            e.WriteMessage("Battle/Command_FlipCoinHead", e.Player.Name);
        }
        else
        {
            e.WriteMessage("Battle/Command_FlipCoinTail", e.Player.Name);
        }
    }
}
