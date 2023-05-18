namespace Versatile.Common.Cards;

public class PokemonCard
{
    public PokemonCardStage Stage { get; set; }

    public int[] DexNumbers { get; set; }
    public CardColor[] Colors { get; set; }
    public int HP { get; set; }
    public ColorModifier Weakness { get; set; }
    public ColorModifier Resistance { get; set; }
    public int RetreatCost { get; set; }
}
