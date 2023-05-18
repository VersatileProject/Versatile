using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Versatile.Common;
using Versatile.Common.Cards;
using Versatile.Core.Settings;

namespace Versatile.Browsers.Decks;

public class DeckPreviewViewModel : ObservableRecipient
{
    public ObservableCollection<CardEntry> PreviewCards { get; set; } = new();

    public PreviewLayoutMode Layout { get ; set; }

    private int _ItemHeight = 196;
    public int ItemHeight { get => _ItemHeight; set => SetProperty(ref _ItemHeight, value); }

    private Brush _Background;
    public Brush Background { get => _Background; set => SetProperty(ref _Background, value); }

    private double _VerticalSpacing;
    public double VerticalSpacing { get => _VerticalSpacing; set => SetProperty(ref _VerticalSpacing, value); }

    private double _HorizontalSpacing;
    public double HorizontalSpacing { get => _HorizontalSpacing; set => SetProperty(ref _HorizontalSpacing, value); }

    public DeckEditorViewModel DeckEditorVM { get; }

    public DeckPreviewViewModel()
    {
        DeckEditorVM = VersatileApp.GetService<DeckEditorViewModel>();
        Layout = VersatileApp.GetService<ILocalSettingsService>().ReadSetting<PreviewLayoutMode>("DeckPreviewLayout");
    }

    public void UpdateCardList()
    {
        PreviewCards.Clear();

        switch (Layout)
        {
            case PreviewLayoutMode.Tiled:
                {
                    ItemHeight = 160;
                    HorizontalSpacing = 16;
                    VerticalSpacing = 8;
                    var stacks = DeckEditorVM.CardSource
                        .OrderBy(x => x.Card.SortKey)
                        .SelectMany(x => Enumerable.Repeat(x.Card, x.Quantity))
                        .Take(60)
                        ;
                    foreach (var stack in stacks)
                    {
                        PreviewCards.Add(new() { Cards = new[] { stack } });
                    }
                }
                break;
            case PreviewLayoutMode.Stacked:
                {
                    ItemHeight = 196;
                    HorizontalSpacing = 32;
                    VerticalSpacing = 16;
                    var stacks = DeckEditorVM.CardSource
                        .OrderBy(x => x.Card.SortKey)
                        .SelectMany(x => Enumerable.Repeat(x.Card, x.Quantity))
                        .GroupBy(x => x.UniqueEffectId)
                        .Select(x => x.ToArray())
                        ;
                    foreach (var stack in stacks)
                    {
                        PreviewCards.Add(new() { Cards = stack });
                    }
                }
                break;
            case PreviewLayoutMode.Counted:
                {
                    ItemHeight = 224;
                    HorizontalSpacing = 32;
                    VerticalSpacing = 32;
                    var stacks = DeckEditorVM.CardSource
                        .OrderBy(x => x.Card.SortKey)
                        .SelectMany(x => Enumerable.Repeat(x.Card, x.Quantity))
                        .GroupBy(x => x.UniqueEffectId)
                        .Select(x => x.ToArray())
                        ;
                    foreach (var stack in stacks)
                    {
                        PreviewCards.Add(new() { Cards = new[] { stack.First() }, Text = stack.Length.ToString() });
                    }
                }
                break;
        }
    }

    public void UpdateBackground()
    {
        var path = DeckEditorVM.PreviewBackgroundPath;

        var uriString = (string.IsNullOrEmpty(path) || !File.Exists(path))
            ? @"ms-appx:///Versatile.Browsers/Assets/background.png"
            : path;
        var uri = new Uri(uriString);
        var bmp = new BitmapImage(uri);
        var brush = new ImageBrush()
        {
            ImageSource = bmp,
            Stretch = Stretch.UniformToFill,
            AlignmentX = AlignmentX.Center,
            AlignmentY = AlignmentY.Center,
        };
        Background = brush;
    }

    public enum PreviewLayoutMode
    {
        Stacked,
        Tiled,
        Counted,
    }

    public class CardEntry
    {
        public Card[] Cards { get; set; }
        public string Text { get; set; }
    }

}
