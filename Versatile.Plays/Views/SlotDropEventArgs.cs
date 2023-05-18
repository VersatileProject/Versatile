using Versatile.Plays.ViewModels;

namespace Versatile.Plays.Views;

public class SlotDropEventArgs
{
    public PlayerSlotKey SourceSlot { get; set; }
    public PlayerSlotKey TargetSlot { get; set; }
    public int[] CardIndexes { get; set; } // null for whole slot
    public bool IsBottom { get; set; } // insert 0 / insert -1
    public bool ForceFaceUp { get; set; }
}
