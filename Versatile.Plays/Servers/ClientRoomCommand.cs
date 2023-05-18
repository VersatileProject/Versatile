using System;
using System.Text.Json;
using Versatile.Networks.Services;
using Versatile.Plays.Battles.Commands;
using Versatile.Plays.Networks;

namespace Versatile.Plays.Servers;

public abstract class ClientRoomCommand : MessageCommand<ClientRoomCommand>, IMessageCommand
{
    protected override MessageType MessageType => MessageType.Room;
}


public class ClientSayCommand : ClientRoomCommand
{
    public string Text { get; set; }
}

public class ClientSendChallengeCommand : ClientRoomCommand
{
    public string TargetUid { get; set; }
}

public class ClientCancelChallengeCommand : ClientRoomCommand
{
    public string TargetUid { get; set; }
}

public class ClientAcceptChallengeCommand : ClientRoomCommand
{
    public string TargetUid { get; set; }
}

public class ClientRefuseChallengeCommand : ClientRoomCommand
{
    public string TargetUid { get; set; }
}

public class ClientLocalGameCommand : ClientRoomCommand
{
}