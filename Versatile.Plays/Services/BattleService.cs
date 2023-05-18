using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Versatile.Common;
using Versatile.Core.Helpers;
using Versatile.Core.Services;
using Versatile.Networks.Services;
using Versatile.Plays.Battles;
using Versatile.Plays.Battles.Commands;
using Versatile.Plays.Clients;
using Versatile.Plays.ViewModels;
using Versatile.Plays.Views;

namespace Versatile.Plays.Services;

public class BattleService
{
    private ClientService Client { get; set; }

    public BattlePlayer Player1 { get; set; }
    public BattlePlayer Player2 { get; set; }

    public event Action<LogRun[]> LogWrote;
    public event Action<SlotUpdatedEventArgs> SlotUpdated;
    public event Action PlaymatUpdated;
    public event Action Closed;
    public event Action<BattleProgress> ProgressChanged;

    public bool IsEnabled { get; set; }
    public DateTime BeginTime { get; set; }

    private Dictionary<string, Type> CommandTypes { get; } = new();

    public BattleProgress Progress { get; set; }
    public BattlePlayer TurnOwner { get; set; }

    public BattleService()
    {
    }

    public void Launch(ClientService client)
    {
        TurnOwner = null;
        Client = client;
        Client.BattleMessageRecived += ProcessBattleMessage;
        IsEnabled = true;
        BeginTime = DateTime.Now;

        ChangeProgress(BattleProgress.Beginning);
        RefreshPlaymat();
    }

    public void RefreshPlaymat()
    {
        PlaymatUpdated?.Invoke();
    }

    public void Close()
    {
        IsEnabled = false;
        Client.BattleMessageRecived -= ProcessBattleMessage;
        Client = null;
        VersatileApp.GetService<HookService>().Raise(new BattleStatusChangedArguments()
        {
            IsInBattle = false
        });
        Closed?.Invoke();
    }

    private readonly DispatcherQueue DispatcherQueue = DispatcherQueue.GetForCurrentThread();


    public void ProcessBattleMessage(MessagePackageInfo msg, BattleCommand cmd)
    {
        if (!IsEnabled) return;

        DispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                var uid = cmd.Uid;
                var player = Player1.Uid == uid ? Player1 : Player2.Uid == uid ? Player2 : null;
                var opponent = Player1.Uid == uid ? Player2 : Player2.Uid == uid ? Player1 : null;

                var args = new BattleCommandArguments(this, player, opponent, msg.Timestamp);
                cmd.Execute(args);

                if (args.UpdatePlaymat)
                {
                    PlaymatUpdated?.Invoke();
                }
                else if(args.SlotsToUpdate?.Count > 0)
                {
                    foreach (var slot in args.SlotsToUpdate)
                    {
                        SlotUpdated?.Invoke(new(player.Slots[slot]));
                    }
                }
                if (args.UpdateProgress.HasValue)
                {
                    if(cmd is BeginTurnCommand) // todo: should not be here
                    {
                        TurnOwner = player;
                    }
                    else if (cmd is EndTurnCommand)
                    {
                        TurnOwner = null;
                    }
                    ChangeProgress(args.UpdateProgress.Value);
                }
            }
            catch(Exception e)
            {
                WriteMessage(new LogRun()
                {
                    Text = e.Message,
                    IsBold = true,
                    Color = Colors.Red,
                });
                IsEnabled = false;
            }
        });

    }

    public string GetCardNameList(BattleCard[] cards, int[] indexes)
    {
        var sb = new StringBuilder();
        for(var i = 0; i < indexes.Length; i++)
        {
            if (i > 0)
            {
                sb.Append(", ");
            }
            sb.Append(indexes[i] + 1);
            if (cards != null && cards[i].Status == BattleCardStatus.FaceUp)
            {
                sb.Append(" [" + cards[i].Data.DisplayName + "]");
            }
        }
        return sb.ToString();
    }

    public string GetCardNameList(IEnumerable<BattleCard> cards)
    {
        var sb = new StringBuilder();
        foreach (var card in cards)
        {
            if (sb.Length > 0)
            {
                sb.Append(", ");
            }
            sb.Append("[" + card.Data.DisplayName + "]");
        }
        return sb.ToString();
    }

    public BattleCard[] MoveCards(BattleSlot sourceSlot, int[] indexes, BattleSlot targetSlot, bool toBottom, BattleCardStatus? newStatus)
    {
        var cards = new List<BattleCard>();
        if(indexes == null)
        {
            cards.AddRange(sourceSlot.Cards);
            sourceSlot.Cards.Clear();
        }
        else
        {
            cards.AddRange(indexes.Select(i => sourceSlot.Cards[i]));
            sourceSlot.Cards = sourceSlot.Cards.Where((x, i) => !indexes.Contains(i)).ToList();
        }

        var isMyHand = sourceSlot.Player.IsMe && targetSlot.Type == PlayerSlotKey.Hand;
        foreach (var card in cards)
        {
            if (newStatus.HasValue)
            {
                card.Status = newStatus.Value;
            }
            if(isMyHand)
            {
                card.View();
            }
        }

        if (toBottom)
        {
            targetSlot.Cards.AddRange(cards);
        }
        else
        {
            targetSlot.Cards.InsertRange(0, cards);
        }

        return cards.ToArray();
    }

    public  void ViewCards(BattleSlot sourceSlot, int[] indexes)
    {
        if (sourceSlot.Player.IsMe)
        {
            if (indexes == null)
            {
                foreach (var card in sourceSlot.Cards)
                {
                    card.View();
                }
            }
            else
            {
                foreach (var index in indexes)
                {
                    sourceSlot.Cards[index].View();
                }
            }
        }
    }

    public void Shuffle(List<BattleCard> cards, Random random, BattleCardStatus? newStatus)
    {
        var result = cards.OrderBy(x => random.Next()).ToArray();
        if (newStatus.HasValue)
        {
            cards.ForEach(x => x.Status = newStatus.Value);
        }

        cards.Clear();
        cards.AddRange(result);
    }

    public BattleCard[] RevealCards(BattleSlot sourceSlot, int[] indexes)
    {
        var cards = new List<BattleCard>();
        if (indexes == null)
        {
            foreach (var card in sourceSlot.Cards)
            {
                card.Status = BattleCardStatus.FaceUp;
                cards.Add(card);
            }
        }
        else
        {
            foreach (var index in indexes)
            {
                sourceSlot.Cards[index].Status = BattleCardStatus.FaceUp;
                cards.Add(sourceSlot.Cards[index]);
            }
        }
        return cards.ToArray();
    }


    public void WriteMessage(params LogRun[] runs)
    {
        if (!IsEnabled) return;

        LogWrote?.Invoke(runs);
    }

    public void Send(BattleCommand command)
    {
        if (!IsEnabled) return;

        Client.Send(command);
        //var action = command.GetType().Name.Replace("Command", "");
        //var text = JsonSerializer.Serialize(command, command.GetType(), new JsonSerializerOptions()
        //{
        //    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
        //    IgnoreReadOnlyProperties = false,
        //});
        //var msg = new MessagePackageInfo(MessageType.Battle, action)
        //{
        //    ["text"] = text,
        //};
        //Client.Send(msg);
    }

    public void ChangeProgress(BattleProgress progress)
    {
        Progress = progress;
        ProgressChanged?.Invoke(progress);
    }

    public void SavePlaymat()
    {
        var savedata = new PlaymatSavedata()
        {
            Player1 = Player1.IsMe ? Player1.ToSavedata() : Player2.ToSavedata(),
            Player2 = Player1.IsMe ? Player2.ToSavedata() : Player1.ToSavedata(),
        };
        var text = Json.Stringify(savedata);

        var savefolder = Path.Combine(VersatileApp.DocumentPath, "Savedata");
        Directory.CreateDirectory(savefolder);
        var filename = $"{BeginTime:yyyy-MM-dd HH-mm-ss}.savedata";
        File.WriteAllText(Path.Combine(savefolder, filename), text);
    }
}

public class BattleStatusChangedArguments
{
    public bool IsInBattle { get; set; }
}