using System;
using Versatile.Common;
using Versatile.Common.Cards;
using Versatile.Plays.ViewModels;

namespace Versatile.Plays.Battles.Commands;

public class ChooseAbilityCommand : BattleCommand
{
    public PlayerSlotKey SlotKey { get; }
    public bool IsOpponent { get; }
    public int CardIndex { get; }
    public int AbilityIndex { get; }

    public ChooseAbilityCommand(PlayerSlotKey slotKey, bool isOpponent, int cardIndex, int abilityIndex)
    {
        SlotKey = slotKey;
        IsOpponent = isOpponent;
        CardIndex = cardIndex;
        AbilityIndex = abilityIndex;
    }

    public override void Execute(BattleCommandArguments e)
    {
        var slot = e.Player.Slots[SlotKey];
        if (CardIndex > slot.Cards.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(CardIndex));
        }

        var card = slot.Cards[CardIndex];
        if (AbilityIndex > card.Data.Abilities.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(AbilityIndex));
        }

        var ability = card.Data.Abilities[AbilityIndex];
        var abitype = VersatileApp.Localize(ability.Type, "Card");
        if (slot.Type == PlayerSlotKey.Active && CardIndex == 0 && !IsOpponent)
        {
            e.WriteMessage("Battle/Command_UseAbility", e.Player.Name, slot.GetName(), abitype, ability.Name);
        }
        else if (IsOpponent)
        {
            var name = string.Join("", e.Battle.GetCardNameList(new[] { card }, new[] { CardIndex }));
            e.WriteMessage("Battle/Command_ChooseOpponentAbility", e.Player.Name, slot.GetName(), name, abitype, ability.Name);
        }
        else
        {
            var name = string.Join("", e.Battle.GetCardNameList(new[] { card }, new[] { CardIndex }));
            e.WriteMessage("Battle/Command_ChooseAbility", e.Player.Name, slot.GetName(), name, abitype, ability.Name);
        }

        if (!IsOpponent)
        {
            if (ability.Type == AbilityType.GX_Attack)
            {
                e.Player.HasGxMarker = true;
                e.UpdatePlaymat = true;
            }
            else if (ability.Type is AbilityType.VSTAR_Ability or AbilityType.VSTAR_Attack)
            {
                e.Player.HasVstarMarker = true;
                e.UpdatePlaymat = true;
            }
        }

        e.WriteSubMessage(ability.Text);
    }
}
