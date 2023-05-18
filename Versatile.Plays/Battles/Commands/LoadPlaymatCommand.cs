using System.Linq;
using Versatile.Common;
using Versatile.Common.Cards;
using Versatile.Plays.ViewModels;

namespace Versatile.Plays.Battles.Commands;

//todo: extreme ugliness

public class LoadPlaymatCommand : BattleCommand
{
    public PlaymatSavedata Savedata { get; }

    public LoadPlaymatCommand(PlaymatSavedata savedata)
    {
        Savedata = savedata;
    }

    public override void Execute(BattleCommandArguments e)
    {
        var player1 = e.Battle.Player1.IsMe ? e.Battle.Player1 : e.Battle.Player2;
        var player2 = e.Battle.Player1.IsMe ? e.Battle.Player2 : e.Battle.Player1;
        var cardService = VersatileApp.GetService<CardDataBaseService>();

        var unknownCards = Savedata.Player1.Slots.Values.Concat(Savedata.Player2.Slots.Values)
            .SelectMany(x => x.Cards)
            .Select(x => x.Key)
            .Distinct()
            .Where(x => cardService.Get(x) == null)
            .ToArray();

        if (unknownCards.Length > 0)
        {
            e.WriteError(string.Format("Unknown cards: {0}", string.Join(", ", unknownCards)));
            return;
        }

        foreach (var (target, source) in new[] {(player1, Savedata.Player1), (player2, Savedata.Player2)})
        {
            target.HasVstarMarker = source.HasVstarMarker;
            target.HasGxMarker = source.HasGxMarker;
            foreach(var slotkey in target.Slots.Keys)
            {
                if (source.Slots.ContainsKey(slotkey))
                {
                    var cards = source.Slots[slotkey].Cards.Select(x => new BattleCard()
                    {
                        Data = cardService.Get(x.Key),
                        Status = x.FaceUp ? BattleCardStatus.FaceUp : slotkey switch
                        {
                            PlayerSlotKey.Hand when target.IsMe => BattleCardStatus.Self,
                            _ => BattleCardStatus.Unknown,
                        },
                    });
                    target.Slots[slotkey].Cards.Clear();
                    target.Slots[slotkey].Cards.AddRange(cards);
                    if (slotkey.IsPokemon())
                    {
                        target.Slots[slotkey].Pokemon = source.Slots[slotkey].Pokemon;
                    }
                }
                else
                {
                    target.Slots[slotkey].Cards.Clear();
                }
                if (slotkey.IsPokemon())
                {
                    target.Slots[slotkey].CheckPokemon();
                }
            }
        }

        e.UpdateProgress = BattleProgress.InBattle;

        e.UpdatePlaymat = true;

        e.WriteMessage("Loaded savedata");
    }
}
