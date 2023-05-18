using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using SuperSocket.Client;
using Versatile.Common;
using Versatile.Networks.Services;
using Versatile.Plays.Battles.Commands;
using Versatile.Plays.Networks;
using Versatile.Plays.Servers;

namespace Versatile.Plays.Clients;

public class ClientService : IDisposable
{
    private IEasyClient<MessagePackageInfo> Client { get; set; }

    public string Nickname { get; set; }
    public string Uid { get; set; }
    public string Address { get; set; }
    public int Port { get; set; }

    public System.Timers.Timer HeartbeatTimer { get; set; }

    public event Action<string> Authorized;
    public event Action Disconnected;

    public event Action<string> MessageRecived;
    public event Action<ServerRoomCommand> RoomMessageRecived;
    public event Action<MessagePackageInfo, BattleCommand> BattleMessageRecived;

    public void Send(IMessageCommand cmd)
    {
        if (Client == null) return;
        var msg = cmd.ToMessagePackageInfo();
        var data = msg.ToBytes();
        Client.SendAsync(data);
    }

    public async Task Connect()
    {
        var address = IPAddress.Parse(Address);
        var port = Port > 0 ? Port : 41945;
        var endpoint = new IPEndPoint(address, port);

        Client = new EasyClient<MessagePackageInfo>(new MessagePipelineFilter()).AsClient();

        if(!await Client.ConnectAsync(endpoint))
        {
            return;
        }

        Debug.WriteLine("Connected!");

        HeartbeatTimer = new (10000);
        HeartbeatTimer.Elapsed += (s, e) =>
        {
            Send(new ClientHeartbeatCommand());
        };
        HeartbeatTimer.Enabled = true;
        HeartbeatTimer.Start();

        _ = Task.Run(Receive);
    }

    public async Task Receive()
    {
        while (true)
        {
            var p = await Client.ReceiveAsync();

            if (p == null) // connection dropped
                break;

            switch (p.Key)
            {
                case MessageType.Connection:
                    {
                        var cmd = ServerConnectCommand.FromMessage(p);
                        ProcessConnectMessage(cmd);
                    }
                    break;
                case MessageType.Room:
                    ProcessRoomMessage(p);
                    break;
                case MessageType.Battle:
                    ProcessBattleMessage(p);
                    break;
            }
        }

        OnDisconnected();
    }

    public void Disconnect()
    {
        HeartbeatTimer.Stop();

        // Disconnect the client
        Debug.WriteLine("Client disconnecting...");
        Client?.CloseAsync();
        Debug.WriteLine("Done!");
    }

    private void OnDisconnected()
    {
        HeartbeatTimer.Stop();
        HeartbeatTimer.Dispose();
        HeartbeatTimer = null;

        Client = null;


        Disconnected?.Invoke();
    }

    private void ProcessConnectMessage(ServerConnectCommand command)
    {
        switch (command)
        {
            case ServerInfoCommand cmd:
                {
                    Uid = cmd.ClientUid;

                    var ret = new ClientInfoCommand()
                    {
                        Version = VersatileApp.Version,
                        Nickname = Nickname,
                    };
                    Send(ret);
                }
                break;
            case AuthorizationResultCommand cmd:
                {
                    if(cmd.Result == AuthorizationResult.Succeeded)
                    {
                        Authorized?.Invoke(cmd.Message);
                    }
                    else
                    {
                        Disconnect();
                    }
                }
                break;
        }
    }

    private void ProcessRoomMessage(MessagePackageInfo pkg)
    {
        var cmd = ServerRoomCommand.FromMessage(pkg);
        RoomMessageRecived?.Invoke(cmd);
    }

    private void ProcessBattleMessage(MessagePackageInfo pkg)
    {
        var cmd = BattleCommand.FromMessage(pkg);
        BattleMessageRecived?.Invoke(pkg, cmd);
    }

    public void Dispose()
    {
        HeartbeatTimer?.Stop();

        Client?.CloseAsync();
        Client = null;
    }

}
