namespace Versatile.Common.Cards;

public class CardAbility
{
    public AbilityType Type { get; set; }
    public string? Name { get; set; }
    public string? Text { get; set; }

    public CardColor[]? Energy { get; set; }
    public MathModifier? EnergyModifier { get; set; }
    public int? Damage { get; set; }
    public MathModifier? DamageModifier { get; set; }

    public bool IsAttack => Type is AbilityType.Attack or AbilityType.GX_Attack or AbilityType.VSTAR_Attack;
}
