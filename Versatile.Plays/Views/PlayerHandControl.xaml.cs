using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Versatile.Common;
using Versatile.Common.Cards;
using Versatile.Plays.Battles;
using Versatile.Plays.Battles.Commands;
using Versatile.Plays.Services;
using Versatile.Plays.ViewModels;
using Windows.ApplicationModel.DataTransfer;

namespace Versatile.Plays.Views;

public sealed partial class PlayerHandControl : UserControl
{
    public event Action<Card> SelectedCardChanged;
    public event Action<BattleCommand> CommandCreated;

    private bool IsDragging { get; set; }

    public static DependencyProperty PlayerProperty = DependencyProperty.Register("Player", typeof(BattlePlayer), typeof(PlayerPlaymat), null);

    public BattlePlayer Player
    {
        get => (BattlePlayer)GetValue(PlayerProperty);
        set => SetValue(PlayerProperty, value);
    }

    public BattleCard[] Cards { get; set; }

    public PlayerHandControl()
    {
        this.InitializeComponent();

        VersatileApp.GetService<BattleService>().SlotUpdated += (args) =>
        {
            if (args.Slot.Player == Player && args.Slot.Type == PlayerSlotKey.Hand)
            {
                Cards = Player.Slots[PlayerSlotKey.Hand].Cards.ToArray();
                this.Bindings.Update();
            }
        };

        VersatileApp.GetService<BattleService>().PlaymatUpdated += () =>
        {
            Cards = Player.Slots[PlayerSlotKey.Hand].Cards.ToArray();
            this.Bindings.Update();
        };
    }

    private void DropAreaFull_DragEnter(object sender, DragEventArgs e)
    {
        DropAreaBorderFull.Visibility = Visibility.Visible;
        e.AcceptedOperation = DataPackageOperation.Move;
        e.DragUIOverride.IsGlyphVisible = false;
    }

    private void DropAreaFull_DragLeave(object sender, DragEventArgs e)
    {
        DropAreaBorderFull.Visibility = Visibility.Collapsed;
    }

    private void DropAreaFull_Drop(object sender, DragEventArgs e)
    {
        DropAreaFull.Visibility = Visibility.Collapsed;

        if (e.DataView == null)
        {
            return;
        }

        if (!e.DataView.Properties.TryGetValue("source_slot", out var _source_slot) || _source_slot is not PlayerSlotKey sourceSlot)
        {
            return;
        }

        var args = new SlotDropEventArgs()
        {
            SourceSlot = sourceSlot,
            TargetSlot = PlayerSlotKey.Hand,
            ForceFaceUp = false,
            IsBottom = true,
        };

        if (e.DataView.Properties.TryGetValue("selected_card_indexes", out var _indexes) && _indexes is int[] indexes)
        {
            CommandCreated?.Invoke(new MoveCardsCommand(args.SourceSlot, indexes, args.TargetSlot, args.IsBottom, args.ForceFaceUp));
        }
        else
        {
            CommandCreated?.Invoke(new MoveCardsCommand(args.SourceSlot, args.TargetSlot, args.IsBottom, args.ForceFaceUp));
        }

    }

    private void UserControl_DragEnter(object sender, DragEventArgs e)
    {
        if (e.DataView == null) return;

        if (!e.DataView.Properties.ContainsKey("selected_card_indexes") && !e.DataView.Properties.ContainsKey("source_slot")) return;

        if (!e.DataView.Properties.TryGetValue("source_slot", out var _source_slot) || _source_slot is not PlayerSlotKey sourceSlot)
        {
            return;
        }

        if (e.DataView?.Properties?.ContainsKey("selected_card_indexes") == true)
        {
            if (sourceSlot is PlayerSlotKey.Hand)
            {
                return;
            }
        }
        else
        {
            if (sourceSlot is PlayerSlotKey.Hand or PlayerSlotKey.Deck or PlayerSlotKey.DiscardPile or PlayerSlotKey.LostZone )
            {
                return;
            }
        }

        //if (Slot.Cards.Count == 0)
        //{
        //    position = PossiblePosition.Top;
        //}

        DropAreaFull.Visibility = Visibility.Visible;
    }

    private void UserControl_DragLeave(object sender, DragEventArgs e)
    {
        DropAreaFull.Visibility = Visibility.Collapsed;
    }

    private void Image_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        var image = (Grid)sender;
        if(image?.DataContext is not BattleCard card)
        {
            return;
        }

        SelectedCardChanged?.Invoke(card.Data);
    }

    private void Image_DragStarting(UIElement sender, DragStartingEventArgs args)
    {
        var image = (Grid)sender;
        if (image?.DataContext is not BattleCard card)
        {
            return;
        }

        var sourceSlot = Player.Slots[PlayerSlotKey.Hand];
        var indexes = new int[] { sourceSlot.Cards.IndexOf(card) };
        args.Data.Properties.Add("selected_card_indexes", indexes);
        args.Data.Properties.Add("source_slot", sourceSlot.Type);
        args.DragUI.SetContentFromBitmapImage(new Microsoft.UI.Xaml.Media.Imaging.BitmapImage());

        sender.Opacity = .5;
        IsDragging = true;
    }

    private void Image_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var image = (Grid)sender;
        image.Margin = new Thickness(0, -image.ActualHeight * .1, 0, 0);
    }

    private void Image_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if(!IsDragging)
        {
            var image = (Grid)sender;
            image.Margin = new Thickness(0, 0, 0, 0);
        }
    }

    private void Image_DropCompleted(UIElement sender, DropCompletedEventArgs args)
    {
        var image = (Grid)sender;
        image.Opacity = 1;
        image.Margin = new Thickness(0, 0, 0, 0);
        IsDragging = false;
    }

}
