using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Versatile.Common;
using Versatile.Common.Cards;
using Versatile.Plays.Battles;
using Versatile.Plays.Battles.Commands;

namespace Versatile.Plays.Views;

public sealed partial class SlotListControl : UserControl
{
    public event Action<Card> SelectedCardChanged;
    public event Action<BattleCommand> CommandCreated;

    public SlotListControl()
    {
        this.InitializeComponent();

        for (var i = 1; i <= 10; i++)
        {
            var mfi = new MenuFlyoutItem();
            mfi.Click += Mfi_Click;
            mfi.MinWidth = 180;
            mfi.DataContext = i;
            CardAbilitiesMenu.Items.Add(mfi);
        }
    }

    private void CreateCommand(BattleCommand command)
    {
        CommandCreated?.Invoke(command);
    }

    private void Mfi_Click(object sender, RoutedEventArgs e)
    {
        var mfi = (MenuFlyoutItem)sender;
        var cardindex = (int)CardAbilitiesMenu.Tag;
        var abilityindex = (int)mfi.Tag;

        var sourceSlot = DataContext as BattleSlot;
        CreateCommand(new ChooseAbilityCommand(sourceSlot.Type, !sourceSlot.Player.IsMe, cardindex, abilityindex));
    }

    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if(e.AddedItems.Any())
        {
            var card = e.AddedItems.Last() as BattleCard;
            if(card.Status == BattleCardStatus.Unknown)
            {
                return;
            }

            SelectedCardChanged?.Invoke(card.Data);
        }
    }

    private Visibility ToVisibility(bool visible)
    {
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    // https://github.com/microsoft/microsoft-ui-xaml/issues/1652
    private void SlotListMenuFlyout_Opening(object sender, object e)
    {
        if (DataContext is not BattleSlot slot)
        {
            return;
        }

        var content = ((sender as MenuFlyout).Target as ListViewItem).Content;
        if (content is not BattleCard battlecard)
        {
            return;
        }

        var hasAbility = false;
        if (battlecard.Status == BattleCardStatus.FaceUp)
        {
            CardAbilitiesMenu.Tag = CardListView.Items.IndexOf(battlecard);
            for (var i = 0; i < battlecard.Data.Abilities.Length; i++)
            {
                var abilityName = battlecard.Data.Abilities[i].Name;
                if (string.IsNullOrEmpty(abilityName))
                {
                    CardAbilitiesMenu.Items[i].Visibility = Visibility.Collapsed;
                }
                else
                {
                    var type = VersatileApp.Localize(battlecard.Data.Abilities[i].Type, "Card");
                    CardAbilitiesMenu.Items[i].Visibility = Visibility.Visible;
                    (CardAbilitiesMenu.Items[i] as MenuFlyoutItem).Text = $"[{type}] {abilityName}";
                    CardAbilitiesMenu.Items[i].Tag = i;
                    hasAbility = true;
                }
            }

            for (var i = battlecard.Data.Abilities.Length; i < CardAbilitiesMenu.Items.Count; i++)
            {
                CardAbilitiesMenu.Items[i].Visibility = Visibility.Collapsed;
            }

        }

        CardAbilitiesMenu.Visibility = hasAbility ? Visibility.Visible : Visibility.Collapsed;

        CardViewMenu.IsEnabled = slot.Player.IsMe;
        CardRevealMenu.IsEnabled = slot.Player.IsMe;
        CardShowToOpponentMenu.IsEnabled = slot.Player.IsMe;
    }

    private void CardView_Click(object sender, RoutedEventArgs e)
    {
        var sourceSlot = DataContext as BattleSlot;
        var indexes = GetSelectedIndexes();
        CreateCommand(new ViewCardsCommand(sourceSlot.Type, indexes));
    }

    private int[] GetSelectedIndexes()
    {
        var list = new List<int>();
        foreach(var iir in CardListView.SelectedRanges)
        {
            for(var i=iir.FirstIndex;i<= iir.LastIndex; i++)
            {
                list.Add(i);
            }
        }
        return list.ToArray();

    }

    private void CardListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
    {
        var sourceSlot = DataContext as BattleSlot;
        if (!sourceSlot.Player.IsMe)
        {
            e.Cancel = true;
            return;
        }
        if(e.Items.Count == 0)
        {
            e.Cancel = true;
            return;
        }
        var indexes = e.Items.Select(x => CardListView.Items.IndexOf(x)).ToArray();
        e.Data.Properties.Add("selected_card_indexes", indexes);
        e.Data.Properties.Add("source_slot", sourceSlot.Type);
    }

    private void CardReveal_Click(object sender, RoutedEventArgs e)
    {
        var sourceSlot = DataContext as BattleSlot;
        var indexes = GetSelectedIndexes();
        CreateCommand(new RevealCardsCommand(sourceSlot.Type, indexes));
    }

    private void CardChoose_Click(object sender, RoutedEventArgs e)
    {
        var sourceSlot = DataContext as BattleSlot;
        var indexes = GetSelectedIndexes();
        CreateCommand(new ChooseCardsCommand(sourceSlot.Type, !sourceSlot.Player.IsMe, indexes));
    }

    private void CardListView_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
    {
        var ctl = e.OriginalSource as DependencyObject;
        while (ctl != null && ctl != sender)
        {
            if (ctl is ListViewItem lvi)
            {
                if (!CardListView.SelectedItems.Contains(lvi.Content))
                {
                    CardListView.SelectedItem = lvi.Content;
                }
                break;
            }
            ctl = VisualTreeHelper.GetParent(ctl);
        }
    }

    private void ShowToOpponent_Click(object sender, RoutedEventArgs e)
    {
        var sourceSlot = DataContext as BattleSlot;
        var indexes = GetSelectedIndexes();
        CreateCommand(new ShowToOpponentCommand(sourceSlot.Type, indexes));
    }
}
