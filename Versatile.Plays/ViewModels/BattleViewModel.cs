using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Versatile.Common;
using Versatile.Common.Cards;
using Versatile.CommonUI.Services;
using Versatile.Core.Services;
using Versatile.Plays.Battles;
using Versatile.Plays.Clients;
using Versatile.Plays.Services;

namespace Versatile.Plays.ViewModels;

public class BattleViewModel : ObservableRecipient
{
    public Card _selectedCard;
    public Card SelectedCard { get => _selectedCard; set => SetProperty(ref _selectedCard, value); }

    private BattlePlayer upperPlayer;
    public BattlePlayer UpperPlayer { get => upperPlayer; set => SetProperty(ref upperPlayer, value); }

    private BattlePlayer lowerPlayer;
    public BattlePlayer LowerPlayer { get => lowerPlayer; set => SetProperty(ref lowerPlayer, value); }

    public string GameId { get; set; }


    private bool _isInBattle;
    public bool IsInPlay { get => _isInBattle; set => SetProperty(ref _isInBattle, value); }

    private bool _isLocal;
    public bool IsLocal { get => _isLocal; set => SetProperty(ref _isLocal, value); }

    public BattleService Battle { get; set; }

    public string MyUid { get; set; }

    public BattlePlayer My { get; set; }

    public BattleSlot _selectedSlot;
    public BattleSlot SelectedSlot { get => _selectedSlot; set => SetProperty(ref _selectedSlot, value); }

    public bool _isLogExpanded;
    public bool IsLogExpanded { get => _isLogExpanded; set => SetProperty(ref _isLogExpanded, value); }

    public Brush _backgroundImage;
    public Brush BackgroundImage { get => _backgroundImage; set => SetProperty(ref _backgroundImage, value); }

    public BattleViewModel(BattleService battle, HookService hookService)
    {
        Battle = battle;

        battle.SlotUpdated += Battle_SlotUpdated;

        hookService.Register<MainWindowClosingArguments>(async (args) =>
        {
            if(VersatileApp.GetService<BattleService>().IsEnabled)
            {
                args.Cancel = true;

                var title = VersatileApp.Localize("Battle/Dialog_ClosingTitle");
                var text = VersatileApp.Localize("Battle/Dialog_ClosingText");
                await VersatileApp.GetService<DialogService>().ShowOk(title, text);
            }
        });
    }

    private void Battle_SlotUpdated(SlotUpdatedEventArgs args)
    {
        if (SelectedSlot == args.Slot)
        {
            SelectedSlot = null;
            SelectedSlot = args.Slot;
        }
    }

    public void InitializeGame(ClientService client, BattlePlayer player1, BattlePlayer player2)
    {
        SelectedSlot = null;
        SelectedCard = null;

        Battle.Player1 = player1;
        Battle.Player2 = player2;

        if (Battle.Player1.IsMe)
        {
            LowerPlayer = Battle.Player1;
            UpperPlayer = Battle.Player2;
            My = Battle.Player1;
            IsInPlay = true;
        }
        else if (Battle.Player2.IsMe)
        {
            LowerPlayer = Battle.Player2;
            UpperPlayer = Battle.Player1;
            My = Battle.Player2;
            IsInPlay = true;
        }
        else
        {
            LowerPlayer = Battle.Player2;
            UpperPlayer = Battle.Player1;
        }

        Battle.Launch(client);

        VersatileApp.NavigateTo(PageKey.Battle);
        VersatileApp.GetService<HookService>().Raise(new BattleStatusChangedArguments() { 
            IsInBattle = true
        });
    }

}
