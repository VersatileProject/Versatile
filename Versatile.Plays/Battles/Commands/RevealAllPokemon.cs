using Versatile.Plays.ViewModels;

namespace Versatile.Plays.Battles.Commands;

public class RevealAllPokemonCommand : BattleCommand
{
    public override void Execute(BattleCommandArguments e)
    {
        for (var targetSlotkey = PlayerSlotKey.Active; targetSlotkey < PlayerSlotKey.Bench10; targetSlotkey++)
        {
            var targetSlot = e.Player.Slots[targetSlotkey];
            if (targetSlot.Cards.Count > 0)
            {
                foreach (var card in targetSlot.Cards)
                {
                    card.Status = BattleCardStatus.FaceUp;
                }
                e.UpdateSlot(targetSlotkey);
            }
        }

        e.WriteMessage("Battle/Command_RevealAllPokemon", e.Player.Name);
    }
}
