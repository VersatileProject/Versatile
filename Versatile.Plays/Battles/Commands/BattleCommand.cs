using System;
using System.Text.Json;
using Versatile.Networks.Services;
using Versatile.Plays.Networks;

namespace Versatile.Plays.Battles.Commands;

public abstract class BattleCommand : MessageCommand<BattleCommand>, IMessageCommand
{
    protected override MessageType MessageType => MessageType.Battle;

    public string Uid { get; set; }
    public string Nickname { get; set; }

    public abstract void Execute(BattleCommandArguments e);
}
