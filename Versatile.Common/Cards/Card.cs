namespace Versatile.Common.Cards;

public class Card
{
    public string Key { get; set; }

    public string Name { get; set; }
    public string Subname { get; set; }

    //public string InternalName { get; set; }

    public CardType Type { get; set; }

    public CardAbility[] Abilities { get; set; }
    public CardRule[] Rules { get; set; }
    public string[] Tags { get; set; }
    public string[] Attributes { get; set; }

    public PokemonCard Pokemon { get; set; }
    public TrainerCard Trainer { get; set; }
    public EnergyCard Energy { get; set; }

    public string Regulation { get; set; }
    public string ProductSymbol { get; set; }
    public string CollectionNumber { get; set; }

    public string Rarity { get; set; }
    public string[] ProductKeys { get; set; }

    public string[] Illustrators { get; set; }
    public string Image { get; set; }

    public ulong UniqueEffectId { get; set; }

    public bool IsBasicPokemon
    {
        get
        {
            return Type == CardType.Pokemon && Pokemon?.Stage == PokemonCardStage.Basic;
        }
    }

    public string DisplayName
    {
        get
        {
            var cardname = Name;
            if (!string.IsNullOrEmpty(Subname))
            {
                cardname += " (" + Subname + ")";
            }
            return cardname;
        }
    }

    public string SortKey
    {
        get
        {
            var sortType = (int)Type;

            var sortPokemon = Type == CardType.Pokemon && Pokemon.DexNumbers?.Length > 0
                ? Pokemon.DexNumbers.Min()
                : 9999;

            var sortStage = Type switch
            {
                CardType.Pokemon => (int)Pokemon.Stage,
                CardType.Trainer => (int)Trainer.Type,
                CardType.Energy => (int)Energy.Type,
                _ => 99,
            };

            var sortColor = Type == CardType.Pokemon
                ? (int)Pokemon.Colors[0]
                : 99;

            return $"{sortType:D1}_{sortPokemon:D4}_{sortStage:D2}_{sortColor:D2}";
        }
    }

    private string? _thumbnailImage;
    public string ThumbnailImage
    {
        get
        {
            _thumbnailImage ??= VersatileApp.GetService<CardDataBaseService>().GetCardImageUri(this) ?? "";

            return _thumbnailImage;
        }
    }

}
