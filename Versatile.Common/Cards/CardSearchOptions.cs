namespace Versatile.Common.Cards;

public class CardSearchOptions
{
    public string? CardName;
    public string[]? KeywordsInName;
    public string[]? KeywordsInText;

    public CardType[]? CardTypes;
    public AbilityType[]? Abilities;

    public CardColor[]? PokemonColors;
    public PokemonCardStage[]? PokemonStages;
    public TrainerCardType[]? TrainerTypes;
    public EnergyCardType[]? EnergyTypes;
    public CardColor[]? PokemonWeaknesses;
    public CardColor[]? PokemonResistances;
    public int[]? PokemonRetreatCosts;
    public int[]? PokemonEnergyCounts;
    public CardColor[]? PokemonEnergyColors;

    public string[]? ProductKeys;
    public string[]? Tags;

    public int[]? PokemonDexNumbers;

    public bool Distinct;

    public string[]? CardKeys;
}
