namespace Versatile.Plays.Battles.Commands;

public class ChangeProgressCommand : BattleCommand
{
    public BattleProgress Progress { get; }

    public ChangeProgressCommand(BattleProgress progress)
    {
        Progress = progress;
    }

    public override void Execute(BattleCommandArguments e)
    {
        if (e.Player.IsMe)
        {
            e.UpdateProgress = Progress;
        }
    }
}
