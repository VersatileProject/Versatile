namespace Versatile.Common.Cards;

public class DeckChecker
{
    public int DeckCount = 60;
    public int SameNameLimit = 4;
    public int HandCount = 7;

    public DeckChecker()
    {
    }

    private long Factorial(int number)
    {
        var value = 1L;
        for (var i = 2; i < number; i++) value *= i;
        return value;
    }

    public record Entry(Card Card, int Quantity);

    public DeckCheckResult Check(Entry[] cards)
    {
        var errorCards = new List<(Card, string)>();
        var sameCards = new Dictionary<string, (int count, int max)>();
        var basicCount = 0;
        var totalCount = 0;
        var acespecs = 0;

        foreach (var (card, qty) in cards)
        {
            totalCount += qty;
            if (IsBasicPokemon(card))
            {
                basicCount += qty;
            }

            var max = IsBasicEnergy(card) ? -1
                : IsUnlimitedCard(card) ? -1
                : IsPrismStar(card) ? 1
                : 4;
            var name = card.Name;
            if (sameCards.ContainsKey(name))
            {
                sameCards[name] = (sameCards[name].count + qty, max);
            }
            else
            {
                sameCards.Add(name, (qty, max));
            }

            if (IsAceSpec(card))
            {
                acespecs += qty;
            }
        }

        var outNames = sameCards.Where(x => x.Value.max > -1 && x.Value.count > x.Value.max).ToDictionary(x => x.Key, x=>x.Value.max);

        foreach (var (card, qty) in cards)
        {
            var name = card.Name;
            if (outNames.TryGetValue(name, out var max))
            {
                errorCards.Add((card, VersatileApp.Localize("DeckEditor/CheckDeck_SameName", name, max)));
            }
            if (acespecs > 1 && IsAceSpec(card))
            {
                errorCards.Add((card, VersatileApp.Localize("DeckEditor/CheckDeck_AceSpec", name, 4)));
            }
        }

        var result = new DeckCheckResult()
        {
            BasicPokemon = basicCount,
            BasicPokemonProbability = (basicCount >=0 && basicCount< BasicPokemonProbabilities.Length) ? BasicPokemonProbabilities[basicCount] : 0,
            InvalidCardAmount = totalCount != DeckCount,
            NoBasicPokemon = basicCount == 0,
            ErrorCards = errorCards.ToArray(),
        };
        return result;
    }

    private static bool IsBasicPokemon(Card card)
    {
        return card.Type == CardType.Pokemon && card.Pokemon?.Stage == PokemonCardStage.Basic;
    }

    private static bool IsBasicEnergy(Card card)
    {
        return card.Type == CardType.Energy && card.Energy?.Type == EnergyCardType.Basic;
    }

    private static bool IsAceSpec(Card card)
    {
        return card.Tags?.Contains("ACE_SPEC") == true;
    }

    private static bool IsPrismStar(Card card)
    {
        return card.Tags?.Contains("PRISM_STAR") == true;
    }

    private static bool IsUnlimitedCard(Card card)
    {
        if (card.Tags?.Contains("POKEMON_ARCEUS") == true) return true;
        return false;
    }

    // 1 - HYPGEOM.DIST(7, 7, 60 - ROW() - 1, 60, FALSE)
    private static readonly double[] BasicPokemonProbabilities = new[]
    {
        0,
        0.116666666666666,
        0.221468926553672,
        0.315429573348919,
        0.399499625744665,
        0.474562172526582,
        0.541436077841381,
        0.600879549232313,
        0.653593571031819,
        0.700225205700612,
        0.741370765702489,
        0.777578858504141,
        0.809353307289264,
        0.837155949976246,
        0.86140931912872,
        0.882499205348263,
        0.900777106738533,
        0.91656256703013,
        0.930145404955457,
        0.941787837462881,
        0.951726499359463,
        0.960174361971557,
        0.967322553412559,
        0.973342083047088,
        0.978385472740882,
        0.982588297485711,
        0.986070637988568,
        0.988938447814451,
        0.991284837671992,
        0.993191279431244,
        0.994728732462898,
        0.995958694888222,
        0.996934182328996,
        0.997700636746747,
        0.998296767960553,
        0.998755330432712,
        0.999103837911553,
        0.999365218520683,
        0.999558412883954,
        0.999698917875423,
        0.999799278583615,
        0.99986953107935,
        0.999917598576431,
        0.999949643574486,
        0.999970378573227,
        0.99998333794744,
        0.999991113571968,
        0.999995556785984,
        0.999997949285839,
        0.999999145535766,
        0.999999689285733,
        0.99999990678572,
        0.999999979285715,
        0.999999997410714,
        1,
        1,
        1,
        1,
        1,
        1,
        1
    };
}
