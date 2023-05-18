using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Versatile.Common.Cards;

namespace Versatile.Browsers.Decks;

public sealed partial class DeckTestPanel : UserControl
{
    public event Action<Card> CardSelected;

    public DeckEditorViewModel ViewModel
    {
        get => (DeckEditorViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty
        = DependencyProperty.Register(
              "ViewModel",
              typeof(DeckEditorViewModel),
              typeof(DeckListPanel),
              new PropertyMetadata(null)
          );

    private class TestHand
    {
        public Card[] InitialHands { get; set; }
        public Card[] FirstDraw { get; set; }
        public Card[] PrizeCards { get; set; }
    }

    private ObservableCollection<TestHand> TestHands { get; set; } = new();

    public DeckTestPanel()
    {
        this.InitializeComponent();
    }

    private void AppBarButton_Click_1(object sender, RoutedEventArgs e)
    {
        TestHands.Clear();
        var cardlist = ViewModel.CardSource
            .OrderBy(x => x.Card.SortKey)
            .ThenBy(x => x.Key)
            .SelectMany(x => Enumerable.Repeat(x, x.Quantity))
            .Select(x => x.Card)
            .ToArray();
        if (cardlist.Length < 7 + 1 + 6) return;
        if (!cardlist.Any(x => x.IsBasicPokemon)) return;

        var rnd = new Random();
        while (TestHands.Count < 10)
        {
            var shuffled = cardlist.OrderBy(x => rnd.Next()).ToArray();
            var initialHands = shuffled.Take(7).OrderBy(x => x.SortKey).ToArray();
            var firstDraw = shuffled.Skip(7).Take(1).ToArray();
            var prizeCards = shuffled.Skip(7 + 1).Take(6).OrderBy(x => x.SortKey).ToArray();

            var isvalid = initialHands.Any(x => x.IsBasicPokemon);

            //if (!isvalid) continue;

            TestHands.Add(new TestHand()
            {
                InitialHands = initialHands,
                FirstDraw = isvalid ? firstDraw : null,
                PrizeCards = isvalid ? prizeCards : null,
            });
        }
    }

    private void CardPanel_CardSelected(Card card)
    {
        CardSelected?.Invoke(card);
    }

}
