namespace Versatile.Common.Cards;

public class DeckCheckResult
{
    public bool Success { get; set; }

    public int BasicPokemon { get; set; } = 0;
    public double BasicPokemonProbability { get; set; } = 0;
    public bool InvalidCardAmount { get; set; } = false;
    public bool NoBasicPokemon { get; set; } = false;
    public (Card card, string Error)[] ErrorCards { get; set; }
}
