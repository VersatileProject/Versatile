using System;
using Versatile.Networks.Services;
using Versatile.Plays.Battles.Commands;
using Versatile.Plays.Networks;

namespace Versatile.Plays.Servers;

public abstract class ServerConnectCommand : MessageCommand<ServerConnectCommand>, IMessageCommand
{
    protected override MessageType MessageType => MessageType.Connection;
}

public class ServerInfoCommand : ServerConnectCommand
{
    public Version Version { get; set; }
    public string ClientUid { get; set; }
}

public class AuthorizationResultCommand : ServerConnectCommand
{
    public AuthorizationResult Result { get; set; }
    public string Message { get; set; }
}

public abstract class ClientConnectCommand : MessageCommand<ClientConnectCommand>, IMessageCommand
{
    protected override MessageType MessageType => MessageType.Connection;
}

public class ClientInfoCommand : ClientConnectCommand
{
    public Version Version { get; set; }
    public string Nickname { get; set; }
}

public class ClientHeartbeatCommand : ClientConnectCommand
{
}
