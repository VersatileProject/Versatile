using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Versatile.Common;
using Versatile.Core.Services;
using Versatile.Core.Settings;
using Versatile.Plays.Battles;
using Versatile.Plays.Clients;
using Versatile.Plays.Servers;
using Versatile.Plays.Services;
using Versatile.Plays.Views;
using Windows.System;

namespace Versatile.Plays.ViewModels;

public class ConnectionViewModel : ObservableRecipient
{
    public event Action<LogRun[]> LogReceived;

    public ICommand LaunchLocalGameCommand { get; }
    public ICommand LaunchServerCommand { get; }
    public ICommand LaunchClientCommand { get; }
    public ICommand CloseNetworkCommand { get; }

    public ICommand CancelChallengeCommand { get; }
    public ICommand RefuseChallengeCommand { get; }
    public ICommand AcceptChallengeCommand { get; }

    private bool isRunning;
    public bool IsRunning { get => isRunning; set => SetProperty(ref isRunning, value); }

    private bool isConnected;
    public bool IsConnected { get => isConnected; set => SetProperty(ref isConnected, value); }

    private bool isInGame;
    public bool IsInGame { get => isInGame; set => SetProperty(ref isInGame, value); }

    private RunningMode _runningMode;
    public RunningMode Mode { get => _runningMode; set => SetProperty(ref _runningMode, value); }

    private string _challengeSendingText;
    public string ChallengeSendingText { get => _challengeSendingText; set => SetProperty(ref _challengeSendingText, value); }

    private string _challengeReceivingText;
    public string ChallengeReceivingText { get => _challengeReceivingText; set => SetProperty(ref _challengeReceivingText, value); }

    public ServerService Server { get; set; }

    public ClientService Client { get; set; }

    public string Nickname { get; set; }
    public string ClientAddress { get; set; }

    public string? OpponentUid { get; set; }

    public ObservableCollection<ClientSideUser> Users { get; set; } = new();

    private readonly Microsoft.UI.Dispatching.DispatcherQueue DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

    public ConnectionViewModel(ILocalSettingsService settings, HookService hookService)
    {
        LaunchServerCommand = new AsyncRelayCommand(LaunchServer);
        LaunchClientCommand = new AsyncRelayCommand<string>(LaunchClient);
        CloseNetworkCommand = new AsyncRelayCommand(CloseNetwork);
        LaunchLocalGameCommand = new AsyncRelayCommand(LaunchLocalGame);
        CancelChallengeCommand = new RelayCommand(CancelChallenge);
        AcceptChallengeCommand = new RelayCommand(AcceptChallenge);
        RefuseChallengeCommand = new RelayCommand(RefuseChallenge);

        Nickname = settings.ReadSetting<string>(nameof(Nickname));
        ClientAddress = settings.ReadSetting<string>(nameof(ClientAddress));

        hookService.Register<MainWindowClosedArguments>(args =>
        {
            settings.SaveSetting(nameof(Nickname), Nickname);
            settings.SaveSetting(nameof(ClientAddress), ClientAddress);
        });
    }

    public ClientSideUser GetUser(string uid) => Users.FirstOrDefault(x => x.Uid == uid);

    public static string GetDefaultNickname() => Environment.UserName;

    public string MyUid => Client?.Uid ?? "";

    private async Task LaunchServer()
    {
        IsRunning = true;
        if(Mode == RunningMode.Offline)
        {
            Mode = RunningMode.ServerClient;
        }

        Server = new();
        if (!await Server.Start())
        {
            await CloseNetwork();
            WriteServerMessage("Cannot launch server.");
            return;
        }

        WriteServerMessage($"Room/Server_Launched");

        await Task.Delay(200);
        await LaunchClient("127.0.0.1");
    }

    private async Task LaunchClient(string address)
    {
        if(string.IsNullOrEmpty(address))
        {
            address = "127.0.0.1";
        }

        var m = Regex.Match(address, @"^(?:(.+?)://){0,1}(.+?)(?:\:(\d+)){0,1}$");
        if (!m.Success)
        {
            return;
        }

        var host = m.Groups[2].Value;
        var port = m.Groups[3].Value.Length > 0 ? int.Parse(m.Groups[3].Value) : 0;
        if (!Regex.IsMatch(host, @"\d+\.\d+\.\d+\.\d+"))
        {
            var entries = Dns.GetHostEntry(host);
            if (entries.AddressList.Length > 0)
            {
                host = entries.AddressList[0].ToString();
            }
            else
            {
                return;
            }
        }

        IsRunning = true;
        if (Mode == RunningMode.Offline)
        {
            Mode = RunningMode.Client;
        }

        Client = new()
        {
            Nickname = GetNickname(),
            Address = host,
            Port = port
        };
        Client.MessageRecived += Client_MessageRecived;
        Client.RoomMessageRecived += Client_RoomMessageRecived;
        Client.Authorized += Client_Authorized;
        Client.Disconnected += Client_Disconnected;

        try
        {
            WriteServerMessage($"Room/Client_ConnectingToServer", address);
            await Client.Connect();
        }
        catch
        {
            WriteServerMessage($"Room/Client_CanNotConnectToServer");
            await CloseNetwork();
        }
    }

    private async Task LaunchLocalGame()
    {
        IsRunning = true;
        IsInGame = true;
        Mode = RunningMode.Local;

        await LaunchServer();

        await Task.Delay(500);

        Client.Send(new ClientLocalGameCommand());
    }

    private async Task CloseNetwork()
    {
        // todo: confirm dialog
        IsInGame = false;
        IsConnected = false;
        IsRunning = false;
        Mode = RunningMode.Offline;

        if (Client != null)
        {
            Client.RoomMessageRecived -= Client_RoomMessageRecived;
            Client.Authorized -= Client_Authorized;
            Client.Disconnected -= Client_Disconnected;

            Client.Disconnect();
            Client.Dispose();
            Client = null;
        }

        if (Server != null)
        {
            await Task.Delay(200);

            Server.Stop();
            Server.Dispose();
            Server = null;
        }

        ChallengeSendingText = null;
        ChallengeReceivingText = null;

        Users.Clear();

        await Task.CompletedTask;
    }

    private void Client_MessageRecived(string msg)
    {
        DispatcherQueue.TryEnqueue(() => WriteClientMessage(DateTime.Now, msg));
    }

    private void Client_RoomMessageRecived(ServerRoomCommand msg)
    {
        DispatcherQueue.TryEnqueue(() => ProcessRoomMessage(msg));
    }

    private void Client_Disconnected()
    {
        CloseNetwork();
    }

    private void Client_Authorized(string message)
    {
        DispatcherQueue.TryEnqueue(() => {
            WriteServerMessage($"Room/Client_ConnectedToServer");
            if(!string.IsNullOrEmpty(message))
            {
                WriteClientMessage(message);
            }
            IsConnected = true;
        });
    }

    public void ClientSend(ClientRoomCommand cmd)
    {
        if (Client == null) return;
        Client.Send(cmd);
    }

    private string GetNickname()
    {
        var name = Nickname;
        if (string.IsNullOrWhiteSpace(name))
        {
            name = GetDefaultNickname();
        }
        return name;
    }

    public void SendChallenge(ClientSideUser user)
    {
        if (OpponentUid != null) return;

        OpponentUid = user.Uid;
        Client.Send(new ClientSendChallengeCommand
        {
            TargetUid = OpponentUid,
        });
    }

    public void CancelChallenge()
    {
        if (OpponentUid == null) return;

        Client.Send(new ClientCancelChallengeCommand
        {
            TargetUid = OpponentUid,
        });
    }

    public void AcceptChallenge()
    {
        if (OpponentUid == null) return;

        Client.Send(new ClientAcceptChallengeCommand
        {
            TargetUid = OpponentUid,
        });
    }

    public void RefuseChallenge()
    {
        if (OpponentUid == null) return;

        Client.Send(new ClientRefuseChallengeCommand
        {
            TargetUid = OpponentUid,
        });
    }

    private void ProcessRoomMessage(ServerRoomCommand msg)
    {
        switch (msg)
        {
            case UserListCommand cmd:
                {
                    Users.Clear();
                    foreach (var entry in cmd.Users)
                    {
                        Users.Add(new ClientSideUser()
                        {
                            Uid = entry.Uid,
                            Nickname = entry.Nickname,
                            IsMe = entry.Uid == MyUid,
                        });
                    }
                }
                break;
            case UserEnterCommand cmd:
                {
                    WriteServerMessage($"Room/Client_UserEntered", cmd.Nickname);
                }
                break;
            case UserLeaveCommand cmd:
                {
                    WriteServerMessage($"Room/Client_UserLeft", cmd.Nickname);
                }
                break;
            case UserSayCommand cmd:
                {
                    WriteUserChatMessage(DateTime.Now, cmd.Nickname, cmd.Text);
                }
                break;
            case UserSendChallengeCommand cmd:
                {
                    WriteServerMessage(DateTime.Now, $"Room/Client_ChallengeOffer", cmd.Player1Nickname, cmd.Player2Nickname);

                    if (cmd.Player1Uid == MyUid)
                    {
                        ChallengeSendingText = VersatileApp.Localize("Room/Client_ChallengeSended", cmd.Player2Nickname);
                    }
                    else if (cmd.Player2Uid == MyUid)
                    {
                        ChallengeReceivingText = VersatileApp.Localize("Room/Client_ChallengeReceived", cmd.Player1Nickname);
                    }
                }
                break;
            case UserCancelChallengeCommand cmd:
                {
                    WriteServerMessage(DateTime.Now, $"Room/Client_ChallengeCancelled", cmd.Player1Nickname);

                    if (cmd.Player1Uid == MyUid)
                    {
                        ChallengeSendingText = null;
                    }
                    else if (cmd.Player2Uid == MyUid)
                    {
                        ChallengeReceivingText = null;
                    }
                }
                break;
            case UserAcceptChallengeCommand cmd:
                {
                    WriteServerMessage(DateTime.Now, $"Room/Client_ChallengeAccepted", cmd.Player2Nickname, cmd.Player1Nickname);

                    if (cmd.Player1Uid == MyUid || cmd.Player2Uid == MyUid)
                    {
                        ChallengeReceivingText = null;
                        ChallengeSendingText = null;
                    }

                }
                break;
            case UserRefuseChallengeCommand cmd:
                {
                    WriteServerMessage(DateTime.Now, $"Room/Client_ChallengeRefused", cmd.Player2Nickname, cmd.Player1Nickname);

                    if (cmd.Player1Uid == MyUid || cmd.Player2Uid == MyUid)
                    {
                        ChallengeReceivingText = null;
                        ChallengeSendingText = null;
                    }
                }
                break;
            case NewGameCommand cmd:
                {
                    WriteServerMessage(DateTime.Now, $"Room/Client_NewGame", cmd.Player1Nickname, cmd.Player2Nickname);

                    var player1 = GetUser(cmd.Player1Uid);
                    var player2 = GetUser(cmd.Player2Uid);

                    if (player1.IsMe || player2.IsMe)
                    {
                        var battleVM = VersatileApp.GetService<BattleViewModel>();
                        if (battleVM.GameId != cmd.GameId)
                        {
                            var battleplayer1 = new BattlePlayer()
                            {
                                Uid = player1.Uid,
                                Name = player1.Nickname,
                                IsMe = player1.IsMe,
                            };
                            var battleplayer2 = new BattlePlayer()
                            {
                                Uid = player2.Uid,
                                Name = player2.Nickname,
                                IsMe = player2.IsMe,
                            };
                            if (player1.Nickname == player2.Nickname)
                            {
                                battleplayer1.Name += "(1P)";
                                battleplayer2.Name += "(2P)";
                            }

                            battleVM.MyUid = MyUid;
                            battleVM.GameId = cmd.GameId;
                            battleVM.IsLocal = false;
                            battleVM.InitializeGame(Client, battleplayer1, battleplayer2);
                            VersatileApp.GetService<BattleService>().Closed += OnGameExited;
                        }
                    }
                }
                break;
            case NewLocalGameCommand cmd:
                {
                    WriteServerMessage(DateTime.Now, $"Room/Client_NewLocalGame");

                    if (cmd.Player1Uid == MyUid)
                    {
                        var gameId = cmd.GameId;
                        var battleVM = VersatileApp.GetService<BattleViewModel>();
                        if (battleVM.GameId != gameId)
                        {
                            var battleplayer1 = new BattlePlayer()
                            {
                                Uid = cmd.Player1Uid,
                                Name = cmd.Player1Nickname,
                                IsMe = true,
                            };
                            var battleplayer2 = new BattlePlayer();

                            battleVM.MyUid = MyUid;
                            battleVM.GameId = gameId;
                            battleVM.IsLocal = true;
                            battleVM.InitializeGame(Client, battleplayer1, battleplayer2);
                            VersatileApp.GetService<BattleService>().Closed += OnGameExited;
                        }

                    }
                }
                break;
        }
    }

    public void OnGameExited()
    {
        IsInGame = false;

        if (Mode == RunningMode.Local)
        {
            CloseNetwork();
        }

        VersatileApp.GetService<BattleService>().Closed -= OnGameExited;
    }

    private void WriteLog(params LogRun[] runs)
    {
        DispatcherQueue.TryEnqueue(() => LogReceived?.Invoke(runs));
    }

    private void WriteServerMessage(string key, params object[] args)
    {
        var localizedText = VersatileApp.Localize(key, args);
        if (localizedText == null) localizedText = key;
        WriteLog(
            new LogRun()
            {
                Text = DateTime.Now.ToString("[HH:mm:ss] "),
                Color = Colors.Gray,
            },
            new LogRun()
            {
                Text = localizedText,
                Color = Colors.Orange,
            }
        );
    }

    private void WriteServerMessage(DateTime timestamp, string key, params object[] args)
    {
        var localizedText = VersatileApp.Localize(key, args);
        if (localizedText == null) localizedText = key;
        WriteLog(
            new LogRun()
            {
                Text = timestamp.ToLocalTime().ToString("[HH:mm:ss] "),
                Color = Colors.Gray,
            },
            new LogRun()
            {
                Text = localizedText,
                Color = Colors.Orange,
            }
        );
    }

    private void WriteClientMessage(string text)
    {
        WriteLog(
            new LogRun()
            {
                Text = DateTime.Now.ToString("[HH:mm:ss] "),
                Color = Colors.Gray,
            },
            new LogRun()
            {
                Text = text,
                Color = Colors.Green,
            }
        );
    }

    private void WriteClientMessage(DateTime timestamp, string text)
    {
        WriteLog(
            new LogRun()
            {
                Text = timestamp.ToLocalTime().ToString("[HH:mm:ss] "),
                Color = Colors.Gray,
            },
            new LogRun()
            {
                Text = text,
                Color = Colors.Green,
            }
        );
    }

    private void WriteUserChatMessage(DateTime timestamp, string username, string text)
    {
        WriteLog(
            new LogRun()
            {
                Text = timestamp.ToLocalTime().ToString("[HH:mm:ss] "),
                Color = Colors.Gray,
            },
            new LogRun()
            {
                Text = $"{username}: ",
                Color = Colors.Blue,
            },
            new LogRun()
            {
                Text = text,
            }
        );
    }

}
