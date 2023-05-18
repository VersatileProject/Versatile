using System;
using System.Text.Json;
using Versatile.Networks.Services;
using Versatile.Plays.Battles.Commands;
using Versatile.Plays.Networks;

namespace Versatile.Plays.Servers;

public abstract class ServerRoomCommand : MessageCommand<ServerRoomCommand>, IMessageCommand
{
    protected override MessageType MessageType => MessageType.Room;
}


public class UserListCommand : ServerRoomCommand
{
    public record UserInfo(string Uid, string Nickname);

    public UserInfo[] Users { get; set; }
}

public class UserEnterCommand : ServerRoomCommand
{
    public string Uid { get; set; }
    public string Nickname { get; set; }
}

public class UserLeaveCommand : ServerRoomCommand
{
    public string Uid { get; set; }
    public string Nickname { get; set; }
    //todo: reason
}

public class UserSayCommand : ServerRoomCommand
{
    public string Uid { get; set; }
    public string Nickname { get; set; }
    public string Text { get; set; }
}

public class UserSendChallengeCommand : ServerRoomCommand
{
    public string Player1Uid { get; set; }
    public string Player1Nickname { get; set; }
    public string Player2Uid { get; set; }
    public string Player2Nickname { get; set; }
}

public class UserCancelChallengeCommand : ServerRoomCommand
{
    public string Player1Uid { get; set; }
    public string Player1Nickname { get; set; }
    public string Player2Uid { get; set; }
    public string Player2Nickname { get; set; }
}

public class UserAcceptChallengeCommand : ServerRoomCommand
{
    public string Player1Uid { get; set; }
    public string Player1Nickname { get; set; }
    public string Player2Uid { get; set; }
    public string Player2Nickname { get; set; }
}

public class UserRefuseChallengeCommand : ServerRoomCommand
{
    public string Player1Uid { get; set; }
    public string Player1Nickname { get; set; }
    public string Player2Uid { get; set; }
    public string Player2Nickname { get; set; }
}

public class NewGameCommand : ServerRoomCommand
{
    public string GameId { get; set; }

    public string Player1Uid { get; set; }
    public string Player1Nickname { get; set; }
    public string Player2Uid { get; set; }
    public string Player2Nickname { get; set; }
}

public class NewLocalGameCommand : ServerRoomCommand
{
    public string GameId { get; set; }

    public string Player1Uid { get; set; }
    public string Player1Nickname { get; set; }
}

