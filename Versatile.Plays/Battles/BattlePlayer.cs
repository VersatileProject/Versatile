using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Versatile.Common.Cards;
using Versatile.Plays.ViewModels;

namespace Versatile.Plays.Battles;

public class BattlePlayer
{
    public string Uid { get; set; }
    public string Name { get; set; }
    public bool IsMe { get; set; }

    public bool IsReady { get; set; }
    public bool HasDeck { get; set; }
    public bool IsTurnBegan { get; set; }

    public bool HasGxMarker { get; set; }
    public bool HasVstarMarker { get; set; }

    public RockPaperScissors? LastRPS { get; set; }

    public Dictionary<PlayerSlotKey, BattleSlot> Slots { get; set; }

    public BattlePlayer()
    {
        Slots = new();
        foreach (var t in Enum.GetValues<PlayerSlotKey>())
        {
            Slots.Add(t, new(t) { Player = this });
        }
    }

    public void LoadDeck((Card, int)[] dict)
    {
        foreach (var (key, list) in Slots)
        {
            list.Cards.Clear();
        }

        var battlecards = new List<BattleCard>();
        foreach (var (card, qty) in dict)
        {
            for (var i = 0; i < qty; i++)
            {
                battlecards.Add(new BattleCard()
                {
                    Data = card,
                    Status = IsMe ? BattleCardStatus.Self : BattleCardStatus.Unknown,
                });
            }
        }

        Slots[PlayerSlotKey.Deck].Cards.AddRange(battlecards);
        HasDeck = true;
    }

    public int GetPrizeCount()
    {
        return Slots.Values.Where(x => x.Type.IsPrize()).Sum(x => x.Cards.Count);
    }

    public BattlePlayerSavedata ToSavedata()
    {
        return new BattlePlayerSavedata()
        {
            Slots = Slots.Where(x => !x.Value.IsEmpty).ToDictionary(
                x => x.Key,
                x => new BattlePlayerSavedata.Slot(x.Value.Cards.Select(y => new BattlePlayerSavedata.CardData(y.Data.Key, y.Status == BattleCardStatus.FaceUp)).ToArray(), x.Value.Type is >= PlayerSlotKey.Active and <= PlayerSlotKey.Bench10 ? x.Value.Pokemon : null)
            ),
            HasGxMarker = HasGxMarker,
            HasVstarMarker = HasVstarMarker,
            Name = Name,
        };
    }
}

public class PlaymatSavedata
{
    public BattlePlayerSavedata Player1 { get; set; }
    public BattlePlayerSavedata Player2 { get; set; }
}


public class BattlePlayerSavedata
{
    public record CardData(string Key, bool FaceUp);
    public record Slot(CardData[] Cards, BattlePokemon Pokemon);

    public Dictionary<PlayerSlotKey, Slot> Slots { get; set; }
    public bool HasGxMarker { get; set; }
    public bool HasVstarMarker { get; set; }
    public string Name { get; set; }
}
