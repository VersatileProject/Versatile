using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Versatile.Common;
using Versatile.CommonUI.Services;
using Versatile.Plays.Battles;
using Versatile.Plays.Battles.Commands;
using Versatile.Plays.Services;
using Versatile.Plays.ViewModels;

namespace Versatile.Plays.Views;

public sealed partial class PlayerPlaymat : UserControl
{
    public event Action<BattleCommand> CommandCreated;

    public static DependencyProperty PlayerProperty = DependencyProperty.Register("Player", typeof(BattlePlayer), typeof(PlayerPlaymat), null);

    public BattlePlayer Player 
    {
        get => (BattlePlayer)GetValue(PlayerProperty);
        set => SetValue(PlayerProperty, value);
    }

    private BattleSlot SelectedSlot;

    private Dictionary<PlayerSlotKey, BattleSlotControl> SlotControls { get; } = new();

    private void CreateCommand(BattleCommand command)
    {
        CommandCreated?.Invoke(command);
    }

    public PlayerPlaymat()
    {
        this.InitializeComponent();

        foreach(var (type, x, y, z, r) in SlotInfo)
        {
            var ctl = new BattleSlotControl();
            Canvas.SetLeft(ctl, x);
            Canvas.SetTop(ctl, y);
            Canvas.SetZIndex(ctl, z);
            ctl.Tapped += BattleSlot_Tapped;
            ctl.RightTapped += BattleSlot_RightTapped;
            ctl.DoubleTapped += BattleSlot_DoubleTapped;
            ctl.SlotKey = type;

            ctl.AllowDrop = true;
            ctl.DragEnter += Ctl_DragEnter;
            ctl.DragLeave += Ctl_DragLeave;
            ctl.CardDropped += Ctl_CardDropped;

            ctl.CanDrag = true;
            ctl.DragStarting += Ctl_DragStarting;
            ctl.DropCompleted += Ctl_DropCompleted;

            ctl.Layout = type switch
            {
                PlayerSlotKey.Deck or PlayerSlotKey.DiscardPile or PlayerSlotKey.LostZone or PlayerSlotKey.Hand => BattleSlotLayout.Stack,
                PlayerSlotKey.Active => BattleSlotLayout.Active,
                >= PlayerSlotKey.Bench1 and <= PlayerSlotKey.Bench10 => BattleSlotLayout.Bench,
                >= PlayerSlotKey.Prize1 and <= PlayerSlotKey.Prize6 => BattleSlotLayout.Prize,
                _ => BattleSlotLayout.None
            };

            if (r != 0)
            {
                ctl.RenderTransformOrigin = new(.5, .5);
                ctl.RenderTransform = new RotateTransform()
                {
                    Angle = r,
                };
            }

            SlotControls.Add(type, ctl);
            PlaymatCanvas.Children.Add(ctl);
        }

        VersatileApp.GetService<BattleService>().SlotUpdated += OnSlotUpdated;
        VersatileApp.GetService<BattleService>().PlaymatUpdated += OnPlaymatUpdated;

        for (var r = 0; r < 8; r++)
        {
            for (var c = 0; c < 5; c++)
            {
                var i = r * 5 + c;
                var button = new Button()
                {
                    Content = (i).ToString(),
                    DataContext = i,
                };
                Grid.SetColumn(button, c);
                Grid.SetRow(button, r);
                button.Click += Button_Click;
                DamageCounterGrid.Children.Add(button);
            }
        }

    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedSlot.Type.IsPokemon())
        {
            var dc = (int)((Button)sender).DataContext;
            CreateCommand(new ChangeDamageCountersCommand(SelectedSlot.Type, dc));
        }
        MyPokemonFlyout.Hide();
    }

    private void Mfi_Click(object sender, RoutedEventArgs e) => throw new NotImplementedException();

    private void Ctl_CardDropped(SlotDropEventArgs args)
    {
        if(args.CardIndexes?.Length > 0)
        {
            CreateCommand(new MoveCardsCommand(args.SourceSlot, args.CardIndexes,args.TargetSlot, args.IsBottom, args.ForceFaceUp));
        }
        else if (args.SourceSlot.IsPokemon() && args.TargetSlot.IsPokemon())
        {
            CreateCommand(new ExchangePokemonCommand(args.SourceSlot, args.TargetSlot));
        }
        else
        {
            CreateCommand(new MoveCardsCommand(args.SourceSlot, args.TargetSlot, args.IsBottom, args.ForceFaceUp));
        }

        if(args.TargetSlot == PlayerSlotKey.Active && VersatileApp.GetService<BattleService>().Progress == BattleProgress.PutActivePokemon)
        {
            CreateCommand(new ChangeProgressCommand(BattleProgress.PutActivePokemon));
        }
    }

    private void Ctl_DragStarting(UIElement sender, DragStartingEventArgs e)
    {
        var slotkey = ((BattleSlotControl)sender).SlotKey;
        var sourceSlot = Player.Slots[slotkey];

        if (sourceSlot.Cards.Count == 0)
        {
            e.Cancel = true;
            return;
        }

        if (!sourceSlot.Player.IsMe)
        {
            e.Cancel = true;
            return;
        }

        e.Data.Properties.Add("source_slot", sourceSlot.Type);
        e.DragUI.SetContentFromBitmapImage(new Microsoft.UI.Xaml.Media.Imaging.BitmapImage());
        sender.Opacity = .5;
    }

    private void Ctl_DropCompleted(UIElement sender, DropCompletedEventArgs args)
    {
        sender.Opacity = 1;
    }

    private void Ctl_DragEnter(object sender, DragEventArgs e)
    {
        if (Player == null || Player.IsMe == false) return;
        if (e.DataView == null) return;

        if (!e.DataView.Properties.ContainsKey("selected_card_indexes") && !e.DataView.Properties.ContainsKey("source_slot")) return;
        
        var targetSlotkey = ((BattleSlotControl)sender).SlotKey;
        (PossiblePosition Position, bool ForceFaceUp)? condition = null;

        if (e.DataView?.Properties?.ContainsKey("selected_card_indexes") == true)
        {
            condition = targetSlotkey switch
            {
                PlayerSlotKey.Deck => (PossiblePosition.TopOrBottom, false),
                PlayerSlotKey.Hand => (PossiblePosition.Bottom, false),
                PlayerSlotKey.DiscardPile => (PossiblePosition.Top, true),
                PlayerSlotKey.Stadium => (PossiblePosition.Top, true),
                PlayerSlotKey.Trainer => (PossiblePosition.Top, true),
                PlayerSlotKey.LostZone => (PossiblePosition.Top, true),
                PlayerSlotKey.Show => (PossiblePosition.Top, true),
                PlayerSlotKey.Hide => (PossiblePosition.Top, false),
                >= PlayerSlotKey.Prize1 and <= PlayerSlotKey.Prize6 => (PossiblePosition.Top, false),
                >= PlayerSlotKey.Bench6 and <= PlayerSlotKey.Bench10 when Player.Slots[PlayerSlotKey.Bench1].IsEmpty || Player.Slots[PlayerSlotKey.Bench2].IsEmpty || Player.Slots[PlayerSlotKey.Bench3].IsEmpty || Player.Slots[PlayerSlotKey.Bench4].IsEmpty || Player.Slots[PlayerSlotKey.Bench5].IsEmpty => null,
                >= PlayerSlotKey.Active and <= PlayerSlotKey.Bench10 when Player.IsTurnBegan => (PossiblePosition.TopOrBottom, true),
                >= PlayerSlotKey.Active and <= PlayerSlotKey.Bench10 => (PossiblePosition.Top, false),
                _ => null,
            };
        }
        else
        {
            if (!e.DataView.Properties.TryGetValue("source_slot", out var _source_slot) || _source_slot is not PlayerSlotKey sourceSlot)
            {
                return;
            }
            if(sourceSlot == targetSlotkey)
            {
                return;
            }

            condition = sourceSlot switch
            {
                PlayerSlotKey.Deck => null,
                PlayerSlotKey.LostZone => null,
                PlayerSlotKey.Hand or PlayerSlotKey.Trainer or PlayerSlotKey.Stadium or PlayerSlotKey.Show or PlayerSlotKey.Hide => targetSlotkey switch
                {
                    PlayerSlotKey.Deck => (PossiblePosition.Top, false),
                    PlayerSlotKey.Hand => (PossiblePosition.Bottom, false),
                    PlayerSlotKey.DiscardPile => (PossiblePosition.Top, true),
                    PlayerSlotKey.Trainer => (PossiblePosition.Top, true),
                    PlayerSlotKey.LostZone => (PossiblePosition.Top, true),
                    PlayerSlotKey.Stadium => (PossiblePosition.Top, true),
                    PlayerSlotKey.Show => (PossiblePosition.Top, true),
                    PlayerSlotKey.Hide => (PossiblePosition.Top, false),
                    _ => null,
                },
                PlayerSlotKey.DiscardPile => targetSlotkey switch
                {
                    PlayerSlotKey.Deck => (PossiblePosition.Top, false),
                    _ => null,
                },
                >= PlayerSlotKey.Prize1 and <= PlayerSlotKey.Prize6 => targetSlotkey switch
                {
                    PlayerSlotKey.Hand => (PossiblePosition.Bottom, false),
                    _ => null,
                },
                >= PlayerSlotKey.Active and <= PlayerSlotKey.Bench10 => targetSlotkey switch
                {
                    PlayerSlotKey.Deck => (PossiblePosition.Top, false),
                    PlayerSlotKey.Hand => (PossiblePosition.Bottom, false),
                    PlayerSlotKey.DiscardPile => (PossiblePosition.Top, true),
                    PlayerSlotKey.LostZone => (PossiblePosition.Top, true),
                    >= PlayerSlotKey.Bench6 and <= PlayerSlotKey.Bench10 when Player.Slots[targetSlotkey].IsEmpty => null,
                    >= PlayerSlotKey.Active and <= PlayerSlotKey.Bench10 => (PossiblePosition.Top, false),
                    _ => null,
                },
                _ => null,
            };
        }
        
        if (condition == null)
        {
            return;
        }

        var ctl = (BattleSlotControl)sender;
        var slot = Player.Slots[targetSlotkey];
        var (position, forceFaceUp) = condition.Value;

        if (slot.Cards.Count == 0)
        {
            position = PossiblePosition.Top;
        }

        ctl.ShowGrid(position, forceFaceUp);
    }

    private void Ctl_DragLeave(object sender, DragEventArgs e)
    {
        var ctl = (BattleSlotControl)sender;
        ctl.HideGrid();
    }

    private void OnSlotUpdated(SlotUpdatedEventArgs args)
    {
        if (Player == null) return;
        if (Player != args.Slot.Player) return;
        SlotControls[args.Slot.Type].Redraw(args.Slot);

        if (args.Slot.Type is PlayerSlotKey.Prize1 or PlayerSlotKey.Prize2 or PlayerSlotKey.Prize3)
        {
            var z = Canvas.GetZIndex(SlotControls[args.Slot.Type + 3]);
            if (args.Slot.IsEmpty)
            {
                z -= 1;
            }
            else
            {
                z += 1;
            }
            Canvas.SetZIndex(SlotControls[args.Slot.Type], z);
        }
    }

    private void OnPlaymatUpdated()
    {
        foreach (var slot in Player.Slots.Values)
        {
            SlotControls[slot.Type].Redraw(slot);
        }
        GxMarker.Visibility = Player.HasGxMarker ? Visibility.Visible : Visibility.Collapsed;
        VstarMarker.Visibility = Player.HasVstarMarker ? Visibility.Visible : Visibility.Collapsed;
    }

    private void BattleSlot_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (Player == null) return;

        var type = ((BattleSlotControl)sender).SlotKey;
        SelectedSlot = Player.Slots[type];
        VersatileApp.GetService<BattleViewModel>().SelectedSlot = SelectedSlot;// todo: to event

        var firstCard = SelectedSlot.Cards.FirstOrDefault();
        if (firstCard != null && firstCard.Status != BattleCardStatus.Unknown)
        {
            VersatileApp.GetService<BattleViewModel>().SelectedCard = firstCard.Data;
        }
    }

    private void BattleSlot_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        if (Player == null) return;
        var control = (BattleSlotControl)sender;

        var type = control.SlotKey;
        SelectedSlot = Player.Slots[type];
        VersatileApp.GetService<BattleViewModel>().SelectedSlot = SelectedSlot;

        var mf = GetMenuFlyout();
        if (mf != null)
        {
            mf.ShowAt(control);
        }
    }

    private void BattleSlot_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (Player == null) return;
        var control = (BattleSlotControl)sender;
        var slotKey = control.SlotKey;
        if (Player.IsMe)
        {
            switch(slotKey)
            {
                case PlayerSlotKey.Deck:
                    DeckDrawButton_Click(null, null); //todo: to command
                    break;
                case >= PlayerSlotKey.Prize1 and <= PlayerSlotKey.Prize6:
                    DrawPrize_Click(null, null);
                    break;
                case PlayerSlotKey.Hand:
                    ShuffleHand_Click(null, null);
                    break;
                default:
                    CreateCommand(new ChooseSlotCommand(slotKey, !Player.IsMe));
                    break;
            };
        }
        else
        {
            CreateCommand(new ChooseSlotCommand(slotKey, !Player.IsMe));
        }
    }


    private FlyoutBase GetMenuFlyout()
    {
        if (SelectedSlot.IsEmpty)
        {
            return null;
        }

        if (Player.IsMe)
        {
            return SelectedSlot.Type switch
            {
                PlayerSlotKey.Deck => MyDeckMenuFlyout,
                PlayerSlotKey.Hand => MyHandMenuFlyout,
                >= PlayerSlotKey.Active and <= PlayerSlotKey.Bench10 => MyPokemonFlyout,
                >= PlayerSlotKey.Prize1 and <= PlayerSlotKey.Prize6 => MyPrizeMenuFlyout,
                PlayerSlotKey.Show or PlayerSlotKey.Hide => MyShowHideMenuFlyout,
                _ => null,
            };
        }
        else
        {
            return ChooseMenuFlyout;
        }
    }

    private static readonly (PlayerSlotKey Type, int X, int Y, int Z, int R)[] SlotInfo = new[]
    {
        (PlayerSlotKey.Deck,        724,  54, 2, 0),
        (PlayerSlotKey.Hand,         16, 257, 2, 0),
        (PlayerSlotKey.DiscardPile, 724, 214, 2, 0),
        (PlayerSlotKey.Stadium,     274, -11, 2, 90),
        (PlayerSlotKey.Trainer,     590,  30, 5, 0),
        (PlayerSlotKey.LostZone,     16,  25, 2, 0),
        (PlayerSlotKey.Show,        844,  54, 3, 0),
        (PlayerSlotKey.Hide,        844, 257, 3, 0),
        (PlayerSlotKey.Prize1,      148,  11, 4, 0),
        (PlayerSlotKey.Prize2,      148, 127, 4, 0),
        (PlayerSlotKey.Prize3,      148, 243, 4, 0),
        (PlayerSlotKey.Prize4,      112,  25, 3, 0),
        (PlayerSlotKey.Prize5,      112, 141, 3, 0),
        (PlayerSlotKey.Prize6,      112, 257, 3, 0),
        (PlayerSlotKey.Active,      430,  30, 4, 0),
        (PlayerSlotKey.Bench1,      250, 227, 3, 0),
        (PlayerSlotKey.Bench2,      340, 227, 3, 0),
        (PlayerSlotKey.Bench3,      430, 227, 3, 0),
        (PlayerSlotKey.Bench4,      520, 227, 3, 0),
        (PlayerSlotKey.Bench5,      610, 227, 3, 0),
        (PlayerSlotKey.Bench6,      610, 347, 2, 0),
        (PlayerSlotKey.Bench7,      520, 347, 2, 0),
        (PlayerSlotKey.Bench8,      430, 347, 2, 0),
        (PlayerSlotKey.Bench9,      340, 347, 2, 0),
        (PlayerSlotKey.Bench10,     250, 347, 2, 0)
    };

    private void DeckDrawButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedSlot.Cards.Count == 0) return;

        CreateCommand(new DrawCardsCommand(1));
    }

    private void ShuffleHand_Click(object sender, RoutedEventArgs e)
    {
        CreateCommand(new ShuffleSlotCommand(PlayerSlotKey.Hand));
    }

    private void RevealHand_Click(object sender, RoutedEventArgs e)
    {
        CreateCommand(new RevealCardsCommand(PlayerSlotKey.Hand));
    }

    private void ViewDeck_Click(object sender, RoutedEventArgs e)
    {
        CreateCommand(new ViewCardsCommand(PlayerSlotKey.Deck));
    }

    private void ShuffleDeck_Click(object sender, RoutedEventArgs e)
    {
        CreateCommand(new ShuffleSlotCommand(PlayerSlotKey.Deck));
    }

    private void RevealDeck_Click(object sender, RoutedEventArgs e)
    {
        CreateCommand(new RevealCardsCommand(PlayerSlotKey.Deck));
    }

    private void DrawPrize_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedSlot.Cards.Count == 0) return;
        CreateCommand(new MoveCardsCommand(SelectedSlot.Type, new[] { 0 },  PlayerSlotKey.Hand, true, false));
    }

    private void ViewPrize_Click(object sender, RoutedEventArgs e)
    {
        CreateCommand(new ViewAllPrizeCommand());
    }

    private void RevealPrize_Click(object sender, RoutedEventArgs e)
    {
        CreateCommand(new RevealAllPrizeCommand());
    }

    private void SetStatusNormalMenuItem_Click(object sender, RoutedEventArgs e)
    {
        CreateCommand(new SetStatusCommand(SelectedSlot.Type, PokemonStatus.Normal));
        MyPokemonFlyout.Hide();
    }

    private void SetStatusParalyzedMenuItem_Click(object sender, RoutedEventArgs e)
    {
        CreateCommand(new SetStatusCommand(SelectedSlot.Type, PokemonStatus.Paralyzed));
        MyPokemonFlyout.Hide();
    }

    private void SetStatusConfusedMenuItem_Click(object sender, RoutedEventArgs e)
    {
        CreateCommand(new SetStatusCommand(SelectedSlot.Type, PokemonStatus.Confused));
        MyPokemonFlyout.Hide();
    }

    private void SetStatusAsleepMenuItem_Click(object sender, RoutedEventArgs e)
    {
        CreateCommand(new SetStatusCommand(SelectedSlot.Type, PokemonStatus.Asleep));
        MyPokemonFlyout.Hide();
    }

    private void SetStatusPoisonedMenuItem_Click(object sender, RoutedEventArgs e)
    {
        CreateCommand(new SetStatusCommand(SelectedSlot.Type, isPoisoned: !SelectedSlot.Pokemon.IsPoisoned));
        MyPokemonFlyout.Hide();
    }

    private void SetStatusBurnedMenuItem_Click(object sender, RoutedEventArgs e)
    {
        CreateCommand(new SetStatusCommand(SelectedSlot.Type, isBurned: !SelectedSlot.Pokemon.IsBurned));
        MyPokemonFlyout.Hide();
    }

    private void MyPokemonFlyout_Opening(object sender, object e)
    {
        if (!SelectedSlot.Type.IsPokemon())
        {
            return;
        }

        var battlecard = SelectedSlot.Cards.FirstOrDefault();
        if (battlecard?.Status == BattleCardStatus.FaceUp)
        {
            var flyout = AbilitiesMenuItem.Flyout as CommandBarFlyout;
            flyout.SecondaryCommands.Clear();

            for (var i = 0; i < battlecard.Data.Abilities.Length; i++)
            {
                var name = battlecard.Data.Abilities[i].Name;
                if (string.IsNullOrEmpty(name)) continue;

                var type = VersatileApp.Localize($"Card/AbilityType_{battlecard.Data.Abilities[i].Type}");
                var button = new AppBarButton()
                {
                    Label = $"[{type}] {battlecard.Data.Abilities[i].Name}",
                    Tag = i,
                };
                button.Click += Button_Click1;

                flyout.SecondaryCommands.Add(button);
            }

            AbilitiesMenuItem.IsEnabled = flyout.SecondaryCommands.Any();
            

            var dcbuttons = DamageCounterGrid.Children;
            var pokemondc = SelectedSlot.Pokemon.DamageCounters;
            for(var i = 0; i < dcbuttons.Count; i++)
            {
                var button = (Button)dcbuttons[i];
                button.IsEnabled = i != pokemondc;
                var tooltip = i > pokemondc ? $"+{i - pokemondc}"
                    : i < pokemondc ? $"-{pokemondc - i}"
                    : "-";
                var text = i.ToString();
                ToolTipService.SetToolTip(button, text);
                button.Content = tooltip;
            }
            ChangeDCMenuItem.IsEnabled = true;
            SetStatusMenuItem.IsEnabled = true;
        }
        else
        {
            AbilitiesMenuItem.IsEnabled = false;
            ChangeDCMenuItem.IsEnabled = false;
            SetStatusMenuItem.IsEnabled = false;
        }

        // todo: mvvm
        SetStatusMenuItem.Visibility = SelectedSlot.Type == PlayerSlotKey.Active ? Visibility.Visible : Visibility.Collapsed;
        SetStatusNormalMenuItem.IsEnabled = SelectedSlot.Pokemon.Rotation != PokemonStatus.Normal;
        SetStatusParalyzedMenuItem .IsEnabled = SelectedSlot.Pokemon.Rotation != PokemonStatus.Paralyzed;
        SetStatusConfusedMenuItem.IsEnabled = SelectedSlot.Pokemon.Rotation != PokemonStatus.Confused;
        SetStatusAsleepMenuItem.IsEnabled = SelectedSlot.Pokemon.Rotation != PokemonStatus.Asleep;
        SetStatusNormalMenuItem.Icon.Visibility = SelectedSlot.Pokemon.Rotation == PokemonStatus.Normal ? Visibility.Visible : Visibility.Collapsed;
        SetStatusParalyzedMenuItem.Icon.Visibility = SelectedSlot.Pokemon.Rotation == PokemonStatus.Paralyzed ? Visibility.Visible : Visibility.Collapsed;
        SetStatusConfusedMenuItem.Icon.Visibility = SelectedSlot.Pokemon.Rotation == PokemonStatus.Confused ? Visibility.Visible : Visibility.Collapsed;
        SetStatusAsleepMenuItem.Icon.Visibility = SelectedSlot.Pokemon.Rotation == PokemonStatus.Asleep ? Visibility.Visible : Visibility.Collapsed;

        SetStatusPoisonedMenuItem.Icon.Visibility = SelectedSlot.Pokemon.IsPoisoned ? Visibility.Visible : Visibility.Collapsed;
        SetStatusBurnedMenuItem.Icon.Visibility = SelectedSlot.Pokemon.IsBurned ? Visibility.Visible : Visibility.Collapsed;

    }

    private void Button_Click1(object sender, RoutedEventArgs e)
    {
        var abb = (AppBarButton)sender;
        var abilityindex = (int)abb.Tag;

        CreateCommand(new ChooseAbilityCommand(SelectedSlot.Type, !SelectedSlot.Player.IsMe, 0, abilityindex));
        MyPokemonFlyout.Hide();
    }

    private void DeckDrawMoreButton_Click(object sender, RoutedEventArgs e)
    {
        var count = int.Parse(((FrameworkElement)sender).Tag.ToString());

        CreateCommand(new DrawCardsCommand(count));
    }

    private void MyDeckMenuFlyout_Opening(object sender, object e)
    {
        var slotCount = SelectedSlot.Cards.Count;
        for (var i = 0; i < DeckDrawMoreMenu.Items.Count; i++)
        {
            var count = int.Parse(DeckDrawMoreMenu.Items[i].Tag.ToString());
            DeckDrawMoreMenu.Items[i].Visibility = count <= slotCount ? Visibility.Visible : Visibility.Collapsed;
        }
        DeckDrawMoreMenu.IsEnabled = slotCount >= 2;
    }

    private void ShufflePrize_Click(object sender, RoutedEventArgs e)
    {
        CreateCommand(new ShuffleAllPrizeCommand());
    }

    private void ChooseButton_Click(object sender, RoutedEventArgs e)
    {
        CreateCommand(new ChooseSlotCommand(SelectedSlot.Type, !SelectedSlot.Player.IsMe));
    }

    private void ChoosePokemon_Click(object sender, RoutedEventArgs e)
    {
        CreateCommand(new ChooseSlotCommand(SelectedSlot.Type, !SelectedSlot.Player.IsMe));
    }

    private void ShowToOpponent_Click(object sender, RoutedEventArgs e)
    {
        CreateCommand(new ShowToOpponentCommand(SelectedSlot.Type));
    }

    private async void AppBarButton_Click(object sender, RoutedEventArgs e)
    {
        var slot = SelectedSlot;
        if (!slot.Player.IsMe)
        {
            return;
        }
        if (!slot.Type.IsPokemon())
        {
            return;
        }

        var slotname = slot.GetName();
        var dc = slot.Pokemon.DamageCounters;

        var dialogService = VersatileApp.GetService<DialogService>();
        var ctl = new ChangeDcControl() {
            Card = slot.Cards.FirstOrDefault(),
            DamageCounters = dc,
        };
        var dialogResult = await dialogService.ShowOkCancel(ctl, slotname);

        if (dialogResult != ContentDialogResult.Primary)
        {
            return;
        }
        if (dc == ctl.DamageCounters)
        {
            return;
        }

        CreateCommand(new ChangeDamageCountersCommand(slot.Type, ctl.DamageCounters));
    }

    private void ShowHideShuffle_Click(object sender, RoutedEventArgs e)
    {
        if (!SelectedSlot.Player.IsMe)
        {
            return;
        }

        if (SelectedSlot.Type is not PlayerSlotKey.Show and not PlayerSlotKey.Hide)
        {
            return;
        }

        CreateCommand(new ShuffleSlotCommand(SelectedSlot.Type));
    }

    private void ShowHideChoose_Click(object sender, RoutedEventArgs e)
    {
        if (!SelectedSlot.Player.IsMe)
        {
            return;
        }

        if (SelectedSlot.Type is not PlayerSlotKey.Show and not PlayerSlotKey.Hide)
        {
            return;
        }

        CreateCommand(new ChooseSlotCommand(SelectedSlot.Type, false));
    }
}

public enum BattleSlotLayout
{
    None,
    Stack, // deck, discards, hands
    Active,
    Bench,
    Prize,
}

public enum PossiblePosition
{
    TopOrBottom,
    Top,
    Bottom,
}
