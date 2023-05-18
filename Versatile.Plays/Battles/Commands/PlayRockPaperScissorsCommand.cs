using System;
using Versatile.Common;

namespace Versatile.Plays.Battles.Commands;

public class PlayRockPaperScissorsCommand : BattleCommand
{
    public RockPaperScissors Play { get; }

    public PlayRockPaperScissorsCommand(RockPaperScissors play)
    {
        Play = play;
    }

    public override void Execute(BattleCommandArguments e)
    {
        if (e.Opponent.LastRPS.HasValue)
        {
            var result = (e.Opponent.LastRPS.Value, Play) switch
            {
                (RockPaperScissors.Rock, RockPaperScissors.Rock) => VersatileApp.Localize("Battle/Command_PlayRPS_Draw"),
                (RockPaperScissors.Paper, RockPaperScissors.Paper) => VersatileApp.Localize("Battle/Command_PlayRPS_Draw"),
                (RockPaperScissors.Scissors, RockPaperScissors.Scissors) => VersatileApp.Localize("Battle/Command_PlayRPS_Draw"),
                (RockPaperScissors.Rock, RockPaperScissors.Scissors) => VersatileApp.Localize("Battle/Command_PlayRPS_Win", e.Opponent.Name),
                (RockPaperScissors.Paper, RockPaperScissors.Rock) => VersatileApp.Localize("Battle/Command_PlayRPS_Win", e.Opponent.Name),
                (RockPaperScissors.Scissors, RockPaperScissors.Paper) => VersatileApp.Localize("Battle/Command_PlayRPS_Win", e.Opponent.Name),
                (RockPaperScissors.Rock, RockPaperScissors.Paper) => VersatileApp.Localize("Battle/Command_PlayRPS_Win", e.Player.Name),
                (RockPaperScissors.Paper, RockPaperScissors.Scissors) => VersatileApp.Localize("Battle/Command_PlayRPS_Win", e.Player.Name),
                (RockPaperScissors.Scissors, RockPaperScissors.Rock) => VersatileApp.Localize("Battle/Command_PlayRPS_Win", e.Player.Name),
                _ => throw new NotSupportedException(),
            };
            var p1 = VersatileApp.Localize(Play, "Battle");
            var p2 = VersatileApp.Localize(e.Opponent.LastRPS.Value, "Battle");
            e.WriteMessage("Battle/Command_PlayRPS", e.Opponent.Name, p2, e.Player.Name, p1, result);

            e.Opponent.LastRPS = null;
        }
        else
        {
            e.Player.LastRPS = Play;
            e.WriteMessage("Battle/Command_PlayRPS_Waiting", e.Player.Name);
        }
    }
}
