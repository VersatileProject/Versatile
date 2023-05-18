using System;
using SuperSocket;
using SuperSocket.Server;
using Versatile.Plays.Networks;

namespace Versatile.Plays.Servers;

public class UserSession : AppSession
{
    public bool Authorized { get; set; }

    public string Nickname { get; set; }

    public string? GameId { get; set; }

    public void Send(IMessageCommand command)
    {
        var msg = command.ToMessagePackageInfo();
        msg.Add("timestamp", DateTime.UtcNow);
        var bytes = msg.ToBytes();
        ((IAppSession)this).SendAsync(bytes);
    }

    public void Send(byte[] bytes)
    {
        ((IAppSession)this).SendAsync(bytes);
    }

}

