using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.Channel;
using SuperSocket.Command;
using Versatile.Common;
using Versatile.Networks.Services;
using Versatile.Plays.Battles.Commands;
using Versatile.Plays.Networks;
using Windows.System;

namespace Versatile.Plays.Servers;

public class ServerService : IDisposable
{
    private IHost ServerHost { get; set; }

    private int Port = 41945;

    public async Task<bool> Start()
    {
        ServerHost = SuperSocketHostBuilder
            .Create<MessagePackageInfo, MessagePipelineFilter>()
            .UseSession<UserSession>()
            .ConfigureSuperSocket(options =>
            {
                options.Name = "Game Server";
                options.Listeners = new() {
                    new ListenOptions()
                    {
                        Ip = "Any",
                        Port = Port
                    }
                };
            })
            .UseSessionHandler(OnSessionConnected, OnSessionClosed)
            .UsePackageHandler(async (session, package) =>
            {
                switch (package.Key)
                {
                    case MessageType.Connection:
                        {
                            var cmd = ClientConnectCommand.FromMessage(package);
                            ProcessConnectMessage((UserSession)session, cmd);
                        }
                        break;
                    case MessageType.Room:
                        {
                            var cmd = ClientRoomCommand.FromMessage(package);
                            ProcessRoomMessage((UserSession)session, cmd);
                        }
                        break;
                    case MessageType.Battle:
                        {
                            var cmd = BattleCommand.FromMessage(package);
                            ProcessBattleMessage((UserSession)session, cmd);
                        }
                        break;
                }
            })
            .UseInProcSessionContainer()
            .Build();

        _ = ServerHost.RunAsync();

        await Task.CompletedTask;

        return true;
    }

    public async void Stop()
    {
        try
        {
            await ServerHost.StopAsync();
            ServerHost.Dispose();
        }
        catch
        {

        }

        ServerHost = null;
    }

    private ValueTask OnSessionConnected(IAppSession session)
    {
        var usersession = session as UserSession;

        var serverinfo = new ServerInfoCommand()
        {
            Version = VersatileApp.Version,
            ClientUid = session.SessionID.ToString(),
        };
        usersession.Send(serverinfo);

        return new ValueTask();
    }

    private ValueTask OnSessionClosed(IAppSession session, CloseEventArgs args)
    {
        var usersession = session as UserSession;

        var sessions = GetSessions();
        var users = new UserListCommand()
        {
            Users = sessions?.Select(x => new UserListCommand.UserInfo(x.SessionID, x.Nickname)).ToArray(),
        };
        SendToAll(users);

        var userleave = new UserLeaveCommand()
        {
            Uid = usersession.SessionID,
            Nickname = usersession.Nickname,
        };
        SendToAll(userleave);

        return new ValueTask();
    }

    private void ProcessConnectMessage(UserSession session, ClientConnectCommand command)
    {
        switch (command)
        {
            case ClientInfoCommand cmd:
                {
                    var clientVersion = new Version(cmd.Version.Major, cmd.Version.Minor);
                    var serverVersion = new Version(VersatileApp.Version.Major, VersatileApp.Version.Minor);
                    var ret = new AuthorizationResultCommand();
                    if (clientVersion < serverVersion)
                    {
                        ret.Result = AuthorizationResult.OldClientVersion;
                    }
                    else if(clientVersion > serverVersion)
                    {
                        ret.Result = AuthorizationResult.OldServerVersion;
                    }
                    else
                    {
                        ret.Result =  AuthorizationResult.Succeeded;
                    }

                    if (ret.Result == AuthorizationResult.Succeeded)
                    {
                        ret.Message = "Welcome";
                        session.Nickname = cmd.Nickname;
                        session.Authorized = true;
                    }
                    session.Send(ret);


                    if (ret.Result == AuthorizationResult.Succeeded)
                    {
                        var sessions = GetSessions();
                        var users = new UserListCommand()
                        {
                            Users = sessions?.Select(x => new UserListCommand.UserInfo(x.SessionID, x.Nickname)).ToArray(),
                        };
                        SendToAll(users);

                        var userentered = new UserEnterCommand()
                        {
                            Uid = session.SessionID,
                            Nickname = session.Nickname,
                        };
                        SendToAll(userentered);
                    }
                }
                break;
        }
    }

    private void ProcessRoomMessage(UserSession session, ClientRoomCommand command)
    {
        switch (command)
        {
            case ClientSayCommand cmd:
                {
                    var ret = new UserSayCommand()
                    {
                        Uid = session.SessionID,
                        Nickname = session.Nickname,
                        Text = cmd.Text,
                    };
                    SendToAll(ret);
                }
                break;
            case ClientSendChallengeCommand cmd:
                {
                    if (cmd.TargetUid == session.SessionID) break;
                    if (session.GameId != null) break;

                    var target = GetSession(cmd.TargetUid);
                    if (target == null) break;
                    if (target.GameId != null) break;

                    var newgameId = Guid.NewGuid().ToString();
                    session.GameId = newgameId;
                    target.GameId = newgameId;
                    var ret = new UserSendChallengeCommand()
                    {
                        Player1Uid = session.SessionID,
                        Player1Nickname = session.Nickname,
                        Player2Uid = target.SessionID,
                        Player2Nickname = target.Nickname,
                    };
                    SendToAll(ret);
                }
                break;
            case ClientCancelChallengeCommand cmd:
                {
                    if (cmd.TargetUid == session.SessionID) break;
                    if (session.GameId == null) break;

                    session.GameId = null;

                    var target = GetSession(cmd.TargetUid);
                    if (target != null && target.GameId != null)
                    {
                        target.GameId = null;
                    }

                    var ret = new UserCancelChallengeCommand()
                    {
                        Player1Uid = session.SessionID,
                        Player1Nickname = session.Nickname,
                        Player2Uid = target.SessionID,
                        Player2Nickname = target.Nickname,
                    };
                    SendToAll(ret);
                }
                break;
            case ClientAcceptChallengeCommand cmd:
                {
                    if (cmd.TargetUid == session.SessionID) break;
                    if (session.GameId == null) break;

                    var challenger = GetSession(cmd.TargetUid);
                    if (challenger == null)
                    {
                        session.GameId = null;
                        break;
                    }
                    if (challenger.GameId != session.GameId)
                    {
                        session.GameId = null;
                        break;
                    }

                    var ret = new UserAcceptChallengeCommand()
                    {
                        Player1Uid = challenger.SessionID,
                        Player1Nickname = challenger.Nickname,
                        Player2Uid = session.SessionID,
                        Player2Nickname = session.Nickname,
                    };
                    SendToAll(ret);

                    var newgamemsg = new NewGameCommand()
                    {
                        GameId = session.GameId,
                        Player1Uid = challenger.SessionID,
                        Player1Nickname = challenger.Nickname,
                        Player2Uid = session.SessionID,
                        Player2Nickname = session.Nickname,
                    };
                    SendToAll(newgamemsg);
                }
                break;
            case ClientRefuseChallengeCommand cmd:
                {
                    if (cmd.TargetUid == session.SessionID) break;
                    if (session.GameId == null) break;

                    session.GameId = null;

                    var challenger = GetSession(cmd.TargetUid);
                    if (challenger != null && challenger.GameId != null)
                    {
                        challenger.GameId = null;
                    }

                    var ret = new UserRefuseChallengeCommand()
                    {
                        Player1Uid = challenger.SessionID,
                        Player1Nickname = challenger.Nickname,
                        Player2Uid = session.SessionID,
                        Player2Nickname = session.Nickname,
                    };
                    SendToAll(ret);
                }
                break;
            case ClientLocalGameCommand cmd:
                {
                    var newgameId = Guid.NewGuid().ToString();
                    session.GameId = newgameId;
                    var ret = new NewLocalGameCommand()
                    {
                        GameId = newgameId,
                        Player1Uid = session.SessionID,
                        Player1Nickname = session.Nickname,
                    };

                    SendToAll(ret);
                }
                break;
        }
    }

    private void ProcessBattleMessage(UserSession session, BattleCommand command)
    {
        command.Uid = session.SessionID;
        command.Nickname = session.Nickname;

        var msg = command.ToMessagePackageInfo();
        msg.Add("timestamp", DateTime.UtcNow);
        var bytes = msg.ToBytes();

        var sessions = GetSessions();
        foreach (var s in sessions)
        {
            if(s.GameId == session.GameId)
            {
                s.Send(bytes);
            }
        }
    }

    private UserSession[] GetSessions()
    {
        var sessions = ServerHost?.AsServer().GetSessionContainer()?.GetSessions().OfType<UserSession>().Where(x => x.Authorized);
        return sessions?.ToArray();
    }

    private UserSession GetSession(string sid)
    {
        var session = ServerHost?.AsServer().GetSessionContainer().GetSessionByID(sid);
        return session is UserSession us ? us : null;
    }

    private void SendToAll(IMessageCommand command)
    {
        if(ServerHost== null) 
        {
            return;
        }

        var msg = command.ToMessagePackageInfo();
        msg.Add("timestamp", DateTime.UtcNow);
        var bytes = msg.ToBytes();

        var sessions = GetSessions();
        foreach (var session in sessions)
        {
            session.Send(bytes);
        }
    }

    public void Dispose()
    {
        ServerHost?.Dispose();
    }

}

