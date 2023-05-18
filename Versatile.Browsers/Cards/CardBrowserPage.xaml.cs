using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Versatile.Browsers.Decks;
using Versatile.Common;
using Versatile.Common.Cards;

namespace Versatile.Browsers.Cards;

public sealed partial class CardBrowserPage : Page
{
    public CardBrowserViewModel ViewModel { get; }
    public DeckEditorViewModel DeckVM { get; set; }

    public CardBrowserPage()
    {
        ViewModel = VersatileApp.GetService<CardBrowserViewModel>();

        InitializeComponent();
    }

    private void SearchResultListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        var ctl = e.OriginalSource as DependencyObject;
        while (ctl != null && ctl != sender)
        {
            if (ctl is ListViewItem lvi)
            {
                var card = lvi.Content as Card;
                DeckVM ??= VersatileApp.GetService<DeckEditorViewModel>();
                DeckVM.AddCard(card);
            }
            ctl = VisualTreeHelper.GetParent(ctl);
        }
    }

    private void OnSearchRequested(CardSearchOptions options, SearchScope scope)
    {
        ViewModel.ApplySearchScope(options, scope);
        ViewModel.Search(options);
    }

    private void LoadPokemonFamily()
    {

    }

    private void CardListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        var ctl = e.OriginalSource as DependencyObject;
        while (ctl != null && ctl != sender)
        {
            if (ctl is ListViewItem lvi)
            {
                var card = lvi.Content as Card;
                if (!CardListView.SelectedItems.Contains(card))
                {
                    CardListView.SelectedItem = card;
                }

                CardMenuFlyout.ShowAt(lvi, e.GetPosition(lvi));
                break;
            }
            ctl = VisualTreeHelper.GetParent(ctl);
        }
    }

    private void AddToDeckMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedCard == null) return;

        DeckVM ??= VersatileApp.GetService<DeckEditorViewModel>();
        DeckVM.AddCard(ViewModel.SelectedCard);
    }

    private void AddToFavouriteMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        // todo
    }

}
