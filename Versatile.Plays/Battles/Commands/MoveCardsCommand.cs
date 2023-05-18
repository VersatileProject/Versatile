using System.Linq;
using System.Text.Json.Serialization;
using Versatile.Plays.ViewModels;

namespace Versatile.Plays.Battles.Commands;

public class MoveCardsCommand : BattleCommand
{
    public PlayerSlotKey SourceSlotKey { get; }
    public PlayerSlotKey TargetSlotKey { get; }
    public bool ToBottom { get; }
    public bool ForceFaceup { get; }
    public int[] Indexes { get; }

    public MoveCardsCommand(PlayerSlotKey sourceSlotKey, PlayerSlotKey targetSlotKey, bool toBottom, bool forceFaceup)
    {
        SourceSlotKey = sourceSlotKey;
        TargetSlotKey = targetSlotKey;
        ToBottom = toBottom;
        ForceFaceup = forceFaceup;
        Indexes = null;
    }

    [JsonConstructor]
    public MoveCardsCommand(PlayerSlotKey sourceSlotKey, int[] indexes, PlayerSlotKey targetSlotKey, bool toBottom, bool forceFaceup)
    {
        SourceSlotKey = sourceSlotKey;
        TargetSlotKey = targetSlotKey;
        ToBottom = toBottom;
        ForceFaceup = forceFaceup;
        Indexes = indexes;
    }

    public override void Execute(BattleCommandArguments e)
    {
        var sourceSlot = e.Player.Slots[SourceSlotKey];
        var targetSlot = e.Player.Slots[TargetSlotKey];
        BattleCardStatus? status = ForceFaceup ? BattleCardStatus.FaceUp : null;
        var oldSourceSlotName = sourceSlot.GetName();
        var oldTargetSlotName = targetSlot.GetName();

        if (Indexes?.Length > 0)
        {
            var movedCards = e.Battle.MoveCards(sourceSlot, Indexes, targetSlot, ToBottom, status);
            var cardnames = e.Battle.GetCardNameList(movedCards, Indexes);
            e.WriteMessage("Battle/Command_MoveCards", e.Player.Name, oldSourceSlotName, oldTargetSlotName, cardnames);
        }
        else
        {
            e.Battle.MoveCards(sourceSlot, null, targetSlot, ToBottom, status);
            e.WriteMessage("Battle/Command_MoveAllCards", e.Player.Name, oldSourceSlotName, oldTargetSlotName);
        }

        if (sourceSlot.Type.IsPokemon())
        {
            sourceSlot.CheckPokemon();
        }
        e.UpdateSlot(SourceSlotKey);
        e.UpdateSlot(TargetSlotKey);

        if (targetSlot.Type is PlayerSlotKey.Trainer or PlayerSlotKey.Stadium
            && targetSlot.Cards.Any()
            && targetSlot.Cards[0].Status == BattleCardStatus.FaceUp
            && targetSlot.Cards[0].Data.Abilities?.Any() == true
            )
        {
            e.WriteSubMessage(targetSlot.Cards[0].Data.Abilities[0].Text);
        }
    }
}
