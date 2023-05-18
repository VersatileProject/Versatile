using System;
using System.Collections.Generic;
using Microsoft.UI;
using Versatile.Common;
using Versatile.Networks.Services;
using Versatile.Plays.Services;
using Versatile.Plays.ViewModels;
using Versatile.Plays.Views;
using Windows.UI;

namespace Versatile.Plays.Battles.Commands;

public sealed class BattleCommandArguments
{
    public BattleService Battle { get; }
    public BattlePlayer Player { get; }
    public BattlePlayer Opponent { get; }
    public DateTime Timestamp { get; }

    public bool IsMe => Player.IsMe;

    public List<PlayerSlotKey> SlotsToUpdate { get; private set; }
    public bool UpdatePlaymat { get; set; }
    public BattleProgress? UpdateProgress { get; set; }
    public BattlePlayer UpdateTurnOwner { get; set; }

    public BattleCommandArguments(BattleService battle, BattlePlayer player, BattlePlayer opponent, DateTime timestamp)
    {
        Battle = battle;
        Player = player;
        Opponent = opponent;
        Timestamp = timestamp;
    }

    private Random _random;
    public Random Random
    {
        get
        {
            if (_random == null)
            {
                var seed = (int)Timestamp.Ticks;
                _random = new Random(seed);
            }
            return _random;
        }
    }

    public void WriteMessage(params LogRun[] runs)
    {
        Battle.WriteMessage(runs);
    }

    public void WriteMessage()
    {
        WriteMessage(
            new LogRun()
            {
                Text = "[--:--:--]",
                Color = Colors.Gray,
            }
        );
    }

    public void WriteMessage(string key, params object[] args)
    {
        var localizedText = VersatileApp.Localize(key, args);
        if (localizedText == null)
        {
            localizedText = string.Format(key, args);
        }
        WriteMessage(
            new LogRun()
            {
                Text = Timestamp.ToLocalTime().ToString("[HH:mm:ss] "),
                Color = Colors.Gray,
            },
            new LogRun()
            {
                Text = localizedText,
                Color = IsMe ? Colors.Green : Colors.Blue,
            }
        );
    }

    public void WriteUserMessage(string text)
    {
        WriteMessage(
            new LogRun()
            {
                Text = Timestamp.ToLocalTime().ToString("[HH:mm:ss] "),
                Color = Colors.Gray,
            },
            new LogRun()
            {
                Text = $"{Player.Name}: ",
                Color = IsMe ? Colors.Green : Colors.Blue,
            },
            new LogRun()
            {
                Text = text,
            }
        );
    }

    public void WriteMessage(string text, Color color)
    {
        WriteMessage(
            new LogRun()
            {
                Text = Timestamp.ToLocalTime().ToString("[HH:mm:ss] "),
                Color = Colors.Gray,
            },
            new LogRun()
            {
                Text = text,
                Color = color,
            }
        );
    }

    public void WriteSubMessage(string text)
    {
        WriteMessage(
            new LogRun()
            {
                Text = "  ",
            },
            new LogRun()
            {
                Text = text,
                Color = Colors.Gray,
            }
        );
    }

    public void WriteError(string text)
    {
        Battle.WriteMessage(
            new LogRun()
            {
                Text = text,
                IsBold = true,
                Color = Colors.Red,
            }
        );
    }

    public void UpdateSlot(PlayerSlotKey slot)
    {
        SlotsToUpdate ??= new();
        SlotsToUpdate.Add(slot);
    }

}
