namespace Versatile.Common.Cards;

public class ColorModifier
{
    public CardColor[] Colors { get; set; }
    public MathModifier Modifier { get; set; }
    public int Value { get; set; }

    public bool HasValue => Colors?.Length > 0 && Modifier != MathModifier.None && Value != 0;
}
