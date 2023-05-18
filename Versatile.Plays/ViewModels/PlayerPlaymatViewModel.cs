using CommunityToolkit.Mvvm.ComponentModel;

namespace Versatile.Plays.ViewModels;

public class PlayerPlaymatViewModel : ObservableObject
{
    public PlayerPlaymatViewModel()
    {
    }

}

public class PlayerSlotInfo
{
    public PlayerSlotKey Type { get; set; }

    public int X { get; set; }
    public int Y { get; set; }
}

public static class PlayerSlotKeyExtensions
{

    public static bool IsActive(this PlayerSlotKey slot)
    {
        return slot == PlayerSlotKey.Active;
    }

    public static bool IsBench(this PlayerSlotKey slot)
    {
        return slot >= PlayerSlotKey.Bench1 && slot <= PlayerSlotKey.Bench10;
    }

    public static bool IsPokemon(this PlayerSlotKey slot)
    {
        return slot.IsActive() || slot.IsBench();
    }

    public static bool IsPrize(this PlayerSlotKey slot)
    {
        return slot >= PlayerSlotKey.Prize1 && slot <= PlayerSlotKey.Prize6;
    }
}