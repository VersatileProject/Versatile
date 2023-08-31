using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Versatile.Plays.Battles;

namespace Versatile.Plays.Views;

public sealed partial class ChangeDcControl : UserControl
{
    public int DamageCounters
    {
        get => (int)GetValue(DamageCountersProperty);
        set
        {
            SetValue(DamageCountersProperty, value);
            InitialDamageCounters ??= value;
        }
    }
    public static DependencyProperty DamageCountersProperty = DependencyProperty.Register("DamageCounters", typeof(ChangeDcControl), typeof(int), new PropertyMetadata(0));

    private int? InitialDamageCounters { get; set; }

    public BattleCard Card
    {
        get => (BattleCard)GetValue(CardProperty);
        set => SetValue(CardProperty, value);
    }
    public static DependencyProperty CardProperty = DependencyProperty.Register("Card", typeof(ChangeDcControl), typeof(BattleCard), new PropertyMetadata(null));


    public ChangeDcControl()
    {
        this.InitializeComponent();
    }

    private void NumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        var changed = args.NewValue - InitialDamageCounters;
        var text = changed >= 0 ? $"(+{changed})" : $"({changed})";
        DCChangedTextBlock.Text = text;
    }
}
