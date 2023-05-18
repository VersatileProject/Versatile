using System;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Versatile.Common;
using Versatile.Common.Cards;
using Versatile.CommonUI.Services;
using Versatile.Core.Settings;
using static Versatile.Browsers.Decks.DeckPreviewViewModel;

namespace Versatile.Browsers.Decks;

public sealed partial class DeckPreviewPanel : UserControl
{
    public DeckPreviewViewModel ViewModel { get; set; }

    public event Action<Card> CardSelected;

    public DeckPreviewPanel()
    {
        ViewModel = new();
        this.InitializeComponent();
    }

    private void WrapPanel_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        //var panel = (WrapPanel)sender;

        //var ah = panel.ActualHeight;
        //var vs = panel.VerticalSpacing;
        //var ih = ViewModel.ItemHeight;
        //if (ah < ih)
        //{
        //    panel.VerticalSpacing = 0;
        //    return;
        //}
        //var line = (int)((ah + vs) / (ih + vs));

        //var bh = BackgroundCanvas.ActualHeight;
        //vs = (bh - ih * line) / (line + 2);
        //if (vs < 0)
        //{
        //    vs = (bh * .9 - ih * line) / (line - 1);
        //}
        //ViewModel.VerticalSpacing = vs;
    }

    private void CaptureButton_Click(object sender, RoutedEventArgs e)
    {
        WindowsService.CaptureElement(BackgroundCanvas);
    }

    public void UpdateCardList()
    {
        ViewModel.UpdateCardList();
    }

    public void UpdateBackground()
    {
        ViewModel.UpdateBackground();
    }

    private void CardPanel_CardSelected(Card card)
    {
        CardSelected?.Invoke(card);
    }

    private async void ChangeBackgroundButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = VersatileApp.GetService<DialogService>();
        await dialog.TryOpen(new[] { ".png", ".jpg", ".bmp" }, (filepath) =>
        {
            ViewModel.DeckEditorVM.PreviewBackgroundPath = filepath;
            UpdateBackground();
        });
    }

    private void TiledLayoutButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Layout = PreviewLayoutMode.Tiled;
        VersatileApp.GetService<ILocalSettingsService>().SaveSetting("DeckPreviewLayout", ViewModel.Layout);
        UpdateCardList();
    }

    private void StackedLayoutButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Layout = PreviewLayoutMode.Stacked;
        VersatileApp.GetService<ILocalSettingsService>().SaveSetting("DeckPreviewLayout", ViewModel.Layout);
        UpdateCardList();
    }

    private void CountedLayoutButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Layout = PreviewLayoutMode.Counted;
        VersatileApp.GetService<ILocalSettingsService>().SaveSetting("DeckPreviewLayout", ViewModel.Layout);
        UpdateCardList();
    }

    private void Border_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if(((FrameworkElement)e.OriginalSource).DataContext is CardEntry entry)
        {
            CardSelected?.Invoke(entry.Cards[0]);
        }
    }
}
