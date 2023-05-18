namespace Versatile.Plays.Battles.Commands;

public class SayCommand : BattleCommand
{
    public string Text { get; }

    public SayCommand(string text)
    {
        Text = text;
    }

    public override void Execute(BattleCommandArguments e)
    {
        e.WriteUserMessage(Text);
    }
}
