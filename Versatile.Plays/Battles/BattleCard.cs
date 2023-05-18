using Versatile.Common.Cards;

namespace Versatile.Plays.Battles;

public class BattleCard
{
    public Card Data { get; set; }
    public BattleCardStatus Status { get; set; }

    public string GetCardName()
    {
        if (Status == BattleCardStatus.FaceUp)
        {
            return Data.DisplayName;
        }
        else
        {
            return "?????";
        }
    }

    public void View()
    {
        if (Status == BattleCardStatus.Unknown)
        {
            Status = BattleCardStatus.Self;
        }
    }
}
