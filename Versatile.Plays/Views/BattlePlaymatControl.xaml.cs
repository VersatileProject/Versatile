using Microsoft.UI.Xaml.Controls;
using Versatile.Common;
using Versatile.Plays.Battles.Commands;
using Versatile.Plays.ViewModels;

namespace Versatile.Plays.Views;

public sealed partial class BattlePlaymatControl : UserControl
{
    private BattleViewModel ViewModel { get; set; }

    public BattlePlaymatControl()
    {
        ViewModel = VersatileApp.GetService<BattleViewModel>();

        this.InitializeComponent();
    }

    private void PlayerPlaymat_CommandCreated(BattleCommand command)
    {
        if (ViewModel.My == null) return;
        ViewModel.Battle.Send(command);
    }

    private void PlayerHandControl_SelectedCardChanged(Common.Cards.Card card)
    {
        ViewModel.SelectedCard = card;
    }
}
