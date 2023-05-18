using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Media;
using Versatile.Common;
using Versatile.CommonUI.Services;
using Versatile.Plays.Battles;
using Versatile.Plays.Battles.Commands;
using Versatile.Plays.Services;
using Versatile.Plays.ViewModels;
using Versatile.Core.Settings;
using System.IO;
using Microsoft.UI;
using System.Linq;
using System.Diagnostics;
using ABI.Windows.ApplicationModel.Activation;
using Versatile.Core.Helpers;
using Versatile.Common.Cards;
using System.Text;

namespace Versatile.Plays.Views;

public sealed partial class BattlePage : Page
{
    private BattleViewModel ViewModel { get; set; }

    private BattleService Battle { get; set; }

    public BattlePage()
    {
        ViewModel = VersatileApp.GetService<BattleViewModel>();

        this.InitializeComponent();

        Battle = VersatileApp.GetService<BattleService>();
        Battle.LogWrote += BattlePage_LogWrote;
        Battle.ProgressChanged += Battle_ProgressChanged;

        var backgroundpath = VersatileApp.GetService<ILocalSettingsService>().ReadSetting<string>("PlaymatBackground");
        if (File.Exists(backgroundpath))
        {
            SetBackground(backgroundpath);
        }
        else
        {
            ViewModel.BackgroundImage = new SolidColorBrush(Colors.Green);
        }

        Battle_ProgressChanged(BattleProgress.Beginning);
    }

    private void Battle_ProgressChanged(BattleProgress progress)
    {
        PreparationPanel.Visibility = progress < BattleProgress.InBattle ? Visibility.Visible : Visibility.Collapsed;

        LoadDeckButton.IsEnabled = true;
        ShuffleAndDrawButton.IsEnabled = progress == BattleProgress.DeckLoaded;
        RevealAndRedrawButton.IsEnabled = progress == BattleProgress.PutActivePokemon && ViewModel.My.Slots[PlayerSlotKey.Active].IsEmpty && !ViewModel.My.Slots[PlayerSlotKey.Hand].Cards.Any(x => x.Data.IsBasicPokemon);
        SetPrizeButton.IsEnabled = progress == BattleProgress.PutActivePokemon && !ViewModel.My.Slots[PlayerSlotKey.Active].IsEmpty;
        RevealPokemonButton.IsEnabled = progress == BattleProgress.DecideFirst;
        
        
        TurnPanel.Visibility = progress >= BattleProgress.InBattle ? Visibility.Visible : Visibility.Collapsed;
        BeginTurnButton.IsEnabled = progress >= BattleProgress.InBattle && Battle.TurnOwner == null;
        EndTurnButton.IsEnabled = progress >= BattleProgress.InBattle && Battle.TurnOwner?.IsMe == true;
    }


    private void BattlePage_LogWrote(LogRun[] obj)
    {
        GameLogBox.AppendText(obj);
    }

    private bool IsMyDeckLoaded => ViewModel.My?.HasDeck == true;

    private void RollDie_Click(object sender, RoutedEventArgs e)
    {
        Battle.Send(new RollDieCommand());
    }

    private void GameLogBox_MessageSended(object sender, ChatMessageEventArgs e)
    {
        Battle.Send(new SayCommand(e.Message));
    }

    private void PlayRPS_Click(object sender, RoutedEventArgs e)
    {
        var tag = ((FrameworkElement)sender).Tag as string;
        if(!Enum.TryParse<RockPaperScissors>(tag, true, out var rps))
        {
            rps = (RockPaperScissors)(new Random().Next(0, 3));
        }
        Battle.Send(new PlayRockPaperScissorsCommand(rps));
    }

    private void SlotListControl_SelectedCardChanged(Common.Cards.Card card)
    {
        ViewModel.SelectedCard = card;
    }

    private void SlotListControl_CommandCreated(BattleCommand command)
    {
        if (ViewModel.My == null) return;
        Battle.Send(command);
    }

    private void FlipCoin_Click(SplitButton sender, SplitButtonClickEventArgs args)
    {
        Battle.Send(new FlipCoinCommand());
    }

    private void FlipCoin_Click(object sender, RoutedEventArgs e)
    {
        Battle.Send(new FlipCoinCommand());
    }

    private void BeginTurn_Click(object sender, RoutedEventArgs e)
    {
        if (!IsMyDeckLoaded) return;
        Battle.Send(new BeginTurnCommand());
        Battle.Send(new DrawCardsCommand(1));
    }

    private void BeginTurn_Click(SplitButton sender, SplitButtonClickEventArgs args)
    {
        if (!IsMyDeckLoaded) return;
        Battle.Send(new BeginTurnCommand());
        Battle.Send(new DrawCardsCommand(1));
    }

    private void BeginTurnWithoutDrawing_Click(object sender, RoutedEventArgs e)
    {
        if (!IsMyDeckLoaded) return;
        Battle.Send(new BeginTurnCommand());
    }

    private void EndTurn_Click(object sender, RoutedEventArgs e)
    {
        if (!IsMyDeckLoaded) return;
        Battle.Send(new MoveCardsCommand(PlayerSlotKey.Trainer, PlayerSlotKey.DiscardPile, false, true));
        Battle.Send(new EndTurnCommand());
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Battle.Send(new ExitCommand());
        Battle.Close();
        VersatileApp.NavigateTo(PageKey.Connection);
        GameLogBox.Clear();
    }

    private void CopyPlaymat_Click(object sender, RoutedEventArgs e)
    {
        WindowsService.CaptureElement(PlaymatViewbox);
    }

    private void RedrawHand_Click(object sender, RoutedEventArgs e)
    {
        if (!IsMyDeckLoaded) return;
        var handCount = ViewModel.My.Slots[PlayerSlotKey.Hand].Cards.Count;
        if (handCount == 0) return;

        Battle.Send(new RevealCardsCommand(PlayerSlotKey.Hand));
        Battle.Send(new MoveCardsCommand(PlayerSlotKey.Hand, PlayerSlotKey.Deck, false, true));
        Battle.Send(new ShuffleSlotCommand(PlayerSlotKey.Deck));
        Battle.Send(new DrawCardsCommand(handCount));

        ViewModel.SelectedSlot = ViewModel.My.Slots[PlayerSlotKey.Hand];
    }

    private void ShuffleDeckAndDraw_Click(object sender, RoutedEventArgs e)
    {
        if (!IsMyDeckLoaded) return;
        Battle.Send(new ShuffleSlotCommand(PlayerSlotKey.Deck));
        Battle.Send(new DrawCardsCommand(7));

        ViewModel.SelectedSlot = ViewModel.My.Slots[PlayerSlotKey.Hand];
    }

    private void SetPrize_Click(object sender, RoutedEventArgs e)
    {
        if (!IsMyDeckLoaded) return;
        Battle.Send(new SetPrizeCommand(6));
    }

    private void Forfeit_Click(object sender, RoutedEventArgs e)
    {
        if (!IsMyDeckLoaded) return;
        Battle.Send(new ForfeitCommand());
    }

    private void RevealPokemon_Click(object sender, RoutedEventArgs e)
    {
        if (!IsMyDeckLoaded) return;
        Battle.Send(new RevealAllPokemonCommand());
    }

    private void GameLogBox_LogBoxDoubleClicked(object sender, EventArgs e)
    {
        var row = Grid.GetRow(GameLogBox);
        if(row == 1)
        {
            Grid.SetRow(GameLogBox, 2);
            Grid.SetRowSpan(GameLogBox, 1);
            BattlePlaymat.Opacity = 1;
        }
        else
        {
            Grid.SetRow(GameLogBox, 1);
            Grid.SetRowSpan(GameLogBox, 2);
            BattlePlaymat.Opacity = 0;
        }
    }

    private void SwitchSide_Click(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.IsLocal) return;

        foreach(var key in Enum.GetValues<PlayerSlotKey>())
        {
            (Battle.Player1.Slots[key].Cards, Battle.Player2.Slots[key].Cards) = (Battle.Player2.Slots[key].Cards, Battle.Player1.Slots[key].Cards);
            (Battle.Player1.Slots[key].Pokemon, Battle.Player2.Slots[key].Pokemon) = (Battle.Player2.Slots[key].Pokemon, Battle.Player1.Slots[key].Pokemon);
            (Battle.Player1.HasGxMarker, Battle.Player2.HasGxMarker) = (Battle.Player2.HasGxMarker, Battle.Player1.HasGxMarker);
            (Battle.Player1.HasVstarMarker, Battle.Player2.HasVstarMarker) = (Battle.Player2.HasVstarMarker, Battle.Player1.HasVstarMarker);
        }
        Battle.RefreshPlaymat();
    }

    private async void ChangeBackground_Click(object sender, RoutedEventArgs e)
    {
        var dialog = VersatileApp.GetService<DialogService>();
        await dialog.TryOpen(new[] { ".png", ".jpg", ".bmp" }, (filepath) =>
        {
            SetBackground(filepath);
            VersatileApp.GetService<ILocalSettingsService>().SaveSetting("PlaymatBackground", filepath);
        });
    }

    private void SetBackground(string filepath)
    {
        var uri = new Uri(filepath);
        var bmp = new BitmapImage(uri);
        var brush = new ImageBrush()
        {
            ImageSource = bmp,
            Stretch = Stretch.UniformToFill,
            AlignmentX = AlignmentX.Center,
            AlignmentY = AlignmentY.Center,
        };
        ViewModel.BackgroundImage = brush;
    }

    private async void LoadDeck(string path)
    {
        var deck = Deck.Load(path);

        if (deck == null)
        {
            return;
        }

        var list = new List<LoadDeckCommand.CardEntry>();
        var cardService = VersatileApp.GetService<CardDataBaseService>();
        var unknownCards = new StringBuilder();
        foreach (var entry in deck.Cards)
        {
            var card = cardService.Get(entry.Key);
            if(card != null)
            {
                list.Add(new(card, entry.Quantity));
            }
            else
            {
                unknownCards.AppendLine($"- {entry.Key}({entry.Name})");
            }
        }

        if(unknownCards.Length > 0)
        {
            var dialog = VersatileApp.GetService<DialogService>();
            await dialog.ShowOk("Unknown cards", unknownCards.ToString());
        }
        else
        {
            Battle.Send(new LoadDeckCommand(list.ToArray()));
            ViewModel.SelectedSlot = ViewModel.My.Slots[PlayerSlotKey.Deck];
        }
    }

    private async void LoadDeckButton_Click(SplitButton sender, SplitButtonClickEventArgs args)
    {
        var fs = VersatileApp.GetService<DialogService>();
        await fs.TryOpen(new[] { ".deck" }, LoadDeck);
    }

    private async void LoadDeckButton_Click_1(object sender, RoutedEventArgs e)
    {
        var fs = VersatileApp.GetService<DialogService>();
        await fs.TryOpen(new[] { ".deck" }, LoadDeck);
    }

    private void ShuffleAndDrawButton_Click(object sender, RoutedEventArgs e)
    {
        Battle.Send(new ShuffleSlotCommand(PlayerSlotKey.Deck));
        Battle.Send(new DrawCardsCommand(7));
        Battle.Send(new ChangeProgressCommand(BattleProgress.PutActivePokemon));

        ViewModel.SelectedSlot = ViewModel.My.Slots[PlayerSlotKey.Hand];
    }

    private void RevealAndRedrawButton_Click(object sender, RoutedEventArgs e)
    {
        var handCount = ViewModel.My.Slots[PlayerSlotKey.Hand].Cards.Count;

        Battle.Send(new RevealCardsCommand(PlayerSlotKey.Hand));
        Battle.Send(new MoveCardsCommand(PlayerSlotKey.Hand, PlayerSlotKey.Deck, false, true));
        Battle.Send(new ShuffleSlotCommand(PlayerSlotKey.Deck));
        Battle.Send(new DrawCardsCommand(handCount));
        Battle.Send(new ChangeProgressCommand(BattleProgress.PutActivePokemon));

        ViewModel.SelectedSlot = ViewModel.My.Slots[PlayerSlotKey.Hand];
    }

    private void SetPrizeButton_Click(object sender, RoutedEventArgs e)
    {
        Battle.Send(new SetPrizeCommand(6));
        Battle.Send(new ChangeProgressCommand(BattleProgress.DecideFirst));
    }

    private void RevealPokemonButton_Click(object sender, RoutedEventArgs e)
    {
        Battle.Send(new RevealAllPokemonCommand());
        Battle.Send(new ChangeProgressCommand(BattleProgress.InBattle));
    }

    private void BeginTurnButton_Click(SplitButton sender, SplitButtonClickEventArgs args)
    {
        Battle.Send(new BeginTurnCommand());
        Battle.Send(new DrawCardsCommand(1));

        ViewModel.SelectedSlot = ViewModel.My.Slots[PlayerSlotKey.Hand];
    }

    private void BeginTurnMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        Battle.Send(new BeginTurnCommand());

        ViewModel.SelectedSlot = ViewModel.My.Slots[PlayerSlotKey.Hand];
    }

    private void EndTurnButton_Click(object sender, RoutedEventArgs e)
    {
        Battle.Send(new MoveCardsCommand(PlayerSlotKey.Trainer, PlayerSlotKey.DiscardPile, false, true));
        Battle.Send(new EndTurnCommand());
    }

    private void ForfeitButton_Click(object sender, RoutedEventArgs e)
    {
        Battle.Send(new ForfeitCommand());
    }

    private async void LoadSavedata_Click(object sender, RoutedEventArgs e)
    {
        var fs = VersatileApp.GetService<DialogService>();
        await fs.TryOpen(new[] { ".savedata" }, (path) =>
        {
            var text = File.ReadAllText(path);
            var savedata = Json.ToObject<PlaymatSavedata>(text);
            Battle.Send(new LoadPlaymatCommand(savedata));
        });
    }
}
