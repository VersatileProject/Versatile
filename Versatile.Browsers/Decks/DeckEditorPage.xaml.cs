using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Versatile.Common;
using Versatile.Common.Cards;
using Versatile.CommonUI.Services;
using Versatile.Core.Helpers;

namespace Versatile.Browsers.Decks;

public sealed partial class DeckEditorPage : Page
{
    public DeckEditorViewModel ViewModel { get; }

    public DeckEditorPage()
    {
        ViewModel = VersatileApp.GetService<DeckEditorViewModel>();

        this.InitializeComponent();
    }

    private void AppBarButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

    }

    private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0) return;
        if ((string)((FrameworkElement)e.AddedItems[0])?.Tag == "info")
        {
            ViewModel.UpdateCardList();
        }
        else if ((string)((FrameworkElement)e.AddedItems[0])?.Tag == "preview")
        {
            DeckPreview.UpdateCardList();
            DeckPreview.UpdateBackground();
        }
    }

    private void OnCardSelected(Card card)
    {
        ViewModel.SelectedCard = card;
    }

    private async void ImportFromDeckCodeButton_Click(object sender, RoutedEventArgs e)
    {
        if (await ViewModel.CheckChange() == false)
        {
            return;
        }

        ViewModel.Clear();
        ViewModel.CheckCount();

        var dialog = VersatileApp.GetService<DialogService>();
        var code = await dialog.ShowInput("Deck code", "");
        if (code == null) return;

        var m1 = Regex.Match(code, @"[A-Za-z0-9]{6}-[A-Za-z0-9]{6}-[A-Za-z0-9]{6}");

        if (m1.Success)
        {
            var url = @"https://www.pokemon-card.com/deck/deck.html?deckID=" + m1.Value;
            var sourcecode = await HtmlUtil.GetSourceCode(url);
            var matches = Regex.Matches(sourcecode, @"id=""deck_.+?"" value=""(.+?)""");
            var results = matches
                .SelectMany(x => x.Groups[1].Value.Split("-"))
                .Select(x => x.Split("_"))
                .ToArray();

            var database = VersatileApp.GetService<CardDataBaseService>();
            foreach (var result in results)
            {
                var card = database.Get("CARD_JP_4_" + result[0]);
                if (card == null) continue;
                ViewModel.AddCard(card, int.Parse(result[1]));
            }
        }
        
    }
}
