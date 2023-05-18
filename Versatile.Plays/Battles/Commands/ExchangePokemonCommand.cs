using System.Text.Json.Serialization;
using Versatile.Plays.ViewModels;

namespace Versatile.Plays.Battles.Commands;

public class ExchangePokemonCommand : BattleCommand
{
    public PlayerSlotKey SourceSlotKey { get; }
    public PlayerSlotKey TargetSlotKey { get; }

    public ExchangePokemonCommand(PlayerSlotKey sourceSlotKey, PlayerSlotKey targetSlotKey)
    {
        SourceSlotKey = sourceSlotKey;
        TargetSlotKey = targetSlotKey;
    }

    public override void Execute(BattleCommandArguments e)
    {
        var sourceSlot = e.Player.Slots[SourceSlotKey];
        var targetSlot = e.Player.Slots[TargetSlotKey];
        var oldSourceSlotName = sourceSlot.GetName();
        var oldTargetSlotName = targetSlot.GetName();

        var sourceCards = sourceSlot.Cards.ToArray();
        var targetCards = targetSlot.Cards.ToArray();
        var sourcePokemon = sourceSlot.Pokemon;
        var targetPokemon = targetSlot.Pokemon;

        sourceSlot.Cards.Clear();
        sourceSlot.Cards.AddRange(targetCards);
        targetSlot.Cards.Clear();
        targetSlot.Cards.AddRange(sourceCards);

        sourceSlot.Pokemon = null;
        sourceSlot.Pokemon = targetPokemon;
        targetSlot.Pokemon = null;
        targetSlot.Pokemon = sourcePokemon;

        sourceSlot.CheckPokemon();
        targetSlot.CheckPokemon();

        e.UpdateSlot(SourceSlotKey);
        e.UpdateSlot(TargetSlotKey);

        e.WriteMessage("Battle/Command_ExchangePokemon", e.Player.Name, oldSourceSlotName, oldTargetSlotName);
    }

}
