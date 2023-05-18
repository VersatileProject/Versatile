using System;

namespace Versatile.Plays.Battles;

public class SlotUpdatedEventArgs : EventArgs
{
    public BattleSlot Slot { get; set; }

    public SlotUpdatedEventArgs(BattleSlot slot)
    {
        ;
        Slot = slot;
    }
}
