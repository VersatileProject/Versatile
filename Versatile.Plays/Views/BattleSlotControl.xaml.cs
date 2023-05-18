using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Versatile.Common.Cards;
using Versatile.Plays.Battles;
using Versatile.Plays.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;

namespace Versatile.Plays.Views;

// todo: viewmodel
public sealed partial class BattleSlotControl : UserControl
{
    public PlayerSlotKey SlotKey { get; set; }
    public BattleSlotLayout Layout { get; set; }
    public event Action<SlotDropEventArgs> CardDropped;

    private static BitmapImage BurnedMarker { get; set; }
    private static BitmapImage PoisonedMarker { get; set; }
    private static BitmapImage PlaceHolder { get; set; }

    private bool IsDropForceFaceUp;
    private PossiblePosition DropPosition;

    static BattleSlotControl()
    {
        BurnedMarker = new BitmapImage(new Uri(@"ms-appx:///Versatile.Plays/Assets/Markers/burned_marker.png"))
        {
            DecodePixelHeight = 24,
            DecodePixelWidth = 24,
        };
        PoisonedMarker = new BitmapImage(new Uri(@"ms-appx:///Versatile.Plays/Assets/Markers/poisoned_marker.png"))
        {
            DecodePixelHeight = 24,
            DecodePixelWidth = 24,
        };
        PlaceHolder = new BitmapImage(new Uri(@"ms-appx:///Versatile.Plays/Assets/placeHolder.png"))
        {
        };
    }

    public BattleSlotControl()
    {
        this.InitializeComponent();

    }

    private struct SlotCardInfo
    {
        public string Path;
        public double X;
        public double Y;
        public int Z;
        public double Rotation;
        public Point RotateOrigin;

        public override string ToString() => $"{Path}_{X}_{Y}_{Z}_{Rotation}";
    }

    private static double NextNormal(Random rnd, double min, double max)
    {
        double sum = 0;
        int count = 4;
        for(var i = 0; i < count; i++)
        {
            sum += rnd.NextDouble();
        }
        return (max - min) * sum / count + min;
    }

    public void Redraw(BattleSlot slot)
    {
        if(SlotKey == PlayerSlotKey.Active && MarkerLayer.Children.Count == 0)
        {
            MarkerLayer.Children.Add(new Image()
            {
                Source = PoisonedMarker,
                Width = 24,
                Height = 24,
            });
            MarkerLayer.Children.Add(new Image()
            {
                Source = BurnedMarker,
                Width = 24,
                Height = 24,
            });
        }

        if (SlotKey == PlayerSlotKey.Active)
        {
            MarkerLayer.Children[0].Opacity = (!slot.IsEmpty && slot.Pokemon.IsPoisoned)
                ? 1 : 0;
            MarkerLayer.Children[1].Opacity = (!slot.IsEmpty && slot.Pokemon.IsBurned)
                ? 1 : 0;
        }


        if (slot.Cards.Count == 0)
        {
            CountBorder.Visibility = Visibility.Collapsed;
            CardListCanvas.Children.Clear();
            return;
        }
        else
        {
            if (Layout is BattleSlotLayout.Stack) {
                CountBorder.Visibility = Visibility.Visible;
                CountTextBlock.Text = slot.Cards.Count.ToString();
            }
            else if (Layout is BattleSlotLayout.Active or BattleSlotLayout.Bench)
            {
                CountBorder.Visibility = slot.Pokemon.DamageCounters > 0 ? Visibility.Visible : Visibility.Collapsed;
                CountTextBlock.Text = $"{slot.Pokemon.DamageCounters} DC";
            }
        }

        var z_ = 0;
        double x_ = 1, y_ = 1;
        var hasRandom = true;
        var isJumbled = false;
        var isReversed = false;
        BattleCard[] cards;

        switch (slot.Type)
        {
            case PlayerSlotKey.Deck:
                x_ = 1.2;
                y_ = -.8;
                z_ = 1;
                isReversed = true;
                hasRandom = false;
                cards = slot.Cards.Take(6).ToArray();
                break;
            case PlayerSlotKey.Hand:
                hasRandom = false;
                cards = slot.Cards.Take(1).ToArray();
                break;
            case PlayerSlotKey.Trainer:
            case PlayerSlotKey.DiscardPile:
            case PlayerSlotKey.LostZone:
            case PlayerSlotKey.Hide:
                z_ = 1;
                cards = slot.Cards.AsEnumerable().Take(10).Reverse().ToArray();
                isJumbled = true;
                isReversed = true;
                break;
            case >= PlayerSlotKey.Prize1 and <=  PlayerSlotKey.Prize6:
                cards = slot.Cards.Take(1).ToArray();
                break;
            case PlayerSlotKey.Show:
                y_ = 32;
                z_ = 1;
                cards = slot.Cards.AsEnumerable().Reverse().ToArray();
                isReversed = true;
                break;
            case PlayerSlotKey.Active:
                x_ = slot.Cards.Count switch
                {
                    int i when i <= 4 => 20 + i,
                    _ => 16,
                };
                y_ = slot.Cards.Count switch
                {
                    int i when i <= 4 => -16 + i,
                    _ => -12,
                };
                z_ = -1;
                cards = slot.Cards.ToArray();
                break;
            case >= PlayerSlotKey.Bench1 and <= PlayerSlotKey.Bench10:
                y_ = slot.Cards.Count switch
                {
                    int i when i <= 10 => -22 + i,
                    _ => -12,
                };
                z_ = -1;
                cards = slot.Cards.ToArray();
                break;
            default:
                cards = slot.Cards.Take(1).ToArray();
                break;
        }

        Random rnd = null;
        if (hasRandom)
        {
            var rndkey = slot.Type + "_";
            if(isJumbled || isReversed)
            {
                rndkey += slot.Cards.Last().Data.Key;
            }
            else
            {
                rndkey += slot.Cards.First().Data.Key;
            }

            rnd = new Random(rndkey.GetHashCode());
        }

        if(isJumbled)
        {
            var leftcount = slot.Cards.Skip(10).Count();
            for(var i = 0; i < leftcount * 3; i++)
            {
                NextNormal(rnd, 0, 0);
            }
        }

        var list = new List<SlotCardInfo>();

        // todo: rotatable stadium
        int baseIndex = 0;

        for (var i = 0; i < cards.Length; i++)
        {
            var card = cards[i];

            int ix = 0, iy = 0;
            var t = 0;
            var ro = new Point(.5, .5);

            if(slot.Type.IsPokemon() && i == 0 && card.Data.Pokemon?.Stage == PokemonCardStage.BREAK)
            {
                t += 90;
                iy += -32;
                baseIndex = 1;
                y_ += -16;
            }
            else if (slot.Type.IsPokemon() && i <= 1 && card.Data.Pokemon?.Stage == PokemonCardStage.LEGEND)
            {
                baseIndex = 1;
                y_ += -12;
                t += 90;
                if (card.Data.Attributes?.Contains("POKEMON_LEGEND_TOP") == true)
                {
                    ix += -40;
                    ro = new(1, .5);
                }
                else if (card.Data.Attributes?.Contains("POKEMON_LEGEND_BOTTOM") == true)
                {
                    ix += +40;
                    ro = new(0, .5);
                }
            }
            else if (slot.Type.IsPokemon() && i <= 3 && card.Data.Pokemon?.Stage == PokemonCardStage.VUnion)
            {
                if (card.Data.Attributes?.Contains("POKEMON_VUNION_TOPLEFT") == true)
                {
                    ix += -40;
                    iy += -56;
                    ro = new(1, 1);
                }
                else if (card.Data.Attributes?.Contains("POKEMON_VUNION_TOPRIGHT") == true)
                {
                    ix += +40;
                    iy += -56;
                    ro = new(0, 1);
                }
                else if (card.Data.Attributes?.Contains("POKEMON_VUNION_BOTTOMLEFT") == true)
                {
                    ix += -40;
                    iy += +56;
                    ro = new(1, .5);
                }
                else if (card.Data.Attributes?.Contains("POKEMON_VUNION_BOTTOMRIGHT") == true)
                {
                    ix += +40;
                    iy += +56;
                    ro = new(0, 0);
                }

                if (slot.Type.IsBench())
                {
                    ix /= 2;
                    iy /= 2;
                }
                baseIndex = 3;
                if (slot.Type.IsActive()) x_ += 12;
                if (slot.Type.IsBench()) y_ -= 8;
            }

            if (i <= baseIndex && slot.Pokemon.Rotation != PokemonStatus.Normal)
            {
                t += (int)slot.Pokemon.Rotation;
            }

            var ox = hasRandom ? NextNormal(rnd, -6, 6) : 0;
            var oy = hasRandom ? NextNormal(rnd, -8, 8) : 0;
            var ot = hasRandom ? NextNormal(rnd, -6, 6) : 0;

            list.Add(new()
            {
                Path = GetImageUri(card),
                X = (i < baseIndex ? 0 : (i - baseIndex)) * x_ + ix + ox,
                Y = (i < baseIndex ? 0 : (i - baseIndex)) * y_ + iy + oy,
                Z = i * z_,
                Rotation = t + ot,
                RotateOrigin = ro,
            });
        }

        var c1 = CardListCanvas.Children.Count;
        var c2 = list.Count;
        var c = Math.Max(c1, c2);
        for (var i = c - 1; i >= 0; i--)
        {
            if (i >= c2)
            {
                CardListCanvas.Children.RemoveAt(i);
                continue;
            }
            if (i < c1 && CardListCanvas.Children[i] is Border b && b.Tag is string s && s == list[i].ToString())
            {
                continue;
            }

            var uri = new Uri(list[i].Path);
            var bmp = new BitmapImage(uri)
            {
                DecodePixelHeight = 112,
                DecodePixelWidth = 80,
            };

            var border = new Border()
            {
                CornerRadius = new(4),
                Child = new Image()
                {
                    Source = bmp,
                },
                Tag = list[i].ToString(),
            };
            UIElementExtensions.SetClipToBounds(border, true);
            if (list[i].Z != 0) Canvas.SetZIndex(border, list[i].Z);
            if (list[i].X != 0) Canvas.SetLeft(border, list[i].X);
            if (list[i].Y != 0) Canvas.SetTop(border, list[i].Y);
            if (list[i].Rotation != 0)
            {
                border.RenderTransformOrigin = list[i].RotateOrigin;
                border.RenderTransform = new RotateTransform()
                {
                    Angle = list[i].Rotation,
                };
            }

            if(i >= c1)
            {
                CardListCanvas.Children.Insert(c1, border);
            }
            else
            {
                CardListCanvas.Children[i] = border;
            }
        }
    }

    private static string GetImageUri(BattleCard card)
    {
        if (card.Status is BattleCardStatus.Unknown or BattleCardStatus.Self)
        {
            return @"ms-appx:///Versatile.Plays/Assets/back.jpg";
        }
        else if (!string.IsNullOrWhiteSpace(card.Data.ThumbnailImage))
        {
            return card.Data.ThumbnailImage;
        }
        else
        {
            return @"ms-appx:///Versatile.Plays/Assets/placeholder.jpg";
        }
    }

    private static BitmapImage GetImage(BattleCard card)
    {
        if (card.Status is BattleCardStatus.Unknown or BattleCardStatus.Self)
        {
            var uri = new Uri(@"ms-appx:///Versatile.Plays/Assets/back.jpg");
            return new BitmapImage(uri)
            {
                DecodePixelHeight = 112,
                DecodePixelWidth = 80,
            };
        }
        else if (!string.IsNullOrWhiteSpace(card.Data.Image))
        {
            var uri = new Uri(card.Data.Image);
            return new BitmapImage(uri)
            {
                DecodePixelHeight = 112,
                DecodePixelWidth = 80,
            };
        }
        else
        {
            var uri = new Uri(@"ms-appx:///Versatile.Plays/Assets/back.jpg");
            return new BitmapImage(uri)
            {
                DecodePixelHeight = 112,
                DecodePixelWidth = 80,
            };
        }
    }

    public void ShowGrid(PossiblePosition position, bool forceFaceup)
    {
        IsDropForceFaceUp = forceFaceup;
        DropPosition = position;

        DropAreaGrid.Visibility = Visibility.Visible;
        if (position == PossiblePosition.TopOrBottom)
        {
            DropAreaTop.Visibility = Visibility.Visible;
            DropAreaBottom.Visibility = Visibility.Visible;
        }
        else
        {
            DropAreaFull.Visibility = Visibility.Visible;
        }
    }

    public void HideGrid()
    {
        DropAreaGrid.Visibility = Visibility.Collapsed;
        DropAreaTop.Visibility = Visibility.Collapsed;
        DropAreaBottom.Visibility = Visibility.Collapsed;
        DropAreaFull.Visibility = Visibility.Collapsed;
    }

    private void DropAreaTop_DragEnter(object sender, DragEventArgs e)
    {
        DropAreaBorderTop.Visibility = Visibility.Visible;
        DropAreaBorderBottom.Visibility = Visibility.Collapsed;
        e.AcceptedOperation = DataPackageOperation.Move;
        //e.DragUIOverride.IsCaptionVisible = false;
        e.DragUIOverride.IsGlyphVisible = false;
        e.DragUIOverride.Caption = "To top";
    }

    private void DropAreaBottom_DragEnter(object sender, DragEventArgs e)
    {
        DropAreaBorderBottom.Visibility = Visibility.Visible;
        DropAreaBorderTop.Visibility = Visibility.Collapsed;
        e.AcceptedOperation = DataPackageOperation.Move;
        //e.DragUIOverride.IsCaptionVisible = false;
        e.DragUIOverride.IsGlyphVisible = false;
        e.DragUIOverride.Caption = "To bottom";
    }

    private void DropAreaFull_DragEnter(object sender, DragEventArgs e)
    {
        DropAreaBorderFull.Visibility = Visibility.Visible;
        e.AcceptedOperation = DataPackageOperation.Move;
        //e.DragUIOverride.IsCaptionVisible = false;
        e.DragUIOverride.IsGlyphVisible = false;
    }

    private void DropAreaTop_DragLeave(object sender, DragEventArgs e)
    {
        DropAreaBorderTop.Visibility = Visibility.Collapsed;
    }

    private void DropAreaBottom_DragLeave(object sender, DragEventArgs e)
    {
        DropAreaBorderBottom.Visibility = Visibility.Collapsed;
    }

    private void DropAreaFull_DragLeave(object sender, DragEventArgs e)
    {
        DropAreaBorderTop.Visibility = Visibility.Collapsed;
    }

    private void DropAreaTop_Drop(object sender, DragEventArgs e)
    {
        CreateDropEvent(e, false);
        HideGrid();
    }

    private void DropAreaBottom_Drop(object sender, DragEventArgs e)
    {
        CreateDropEvent(e, true);
        HideGrid();
    }

    private void DropAreaFull_Drop(object sender, DragEventArgs e)
    {
        CreateDropEvent(e, DropPosition == PossiblePosition.Bottom);
        HideGrid();
    }

    private void CreateDropEvent(DragEventArgs e, bool isBottom)
    {
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
            TargetSlot = SlotKey,
            ForceFaceUp = IsDropForceFaceUp,
            IsBottom = isBottom,
        };

        if (e.DataView.Properties.TryGetValue("selected_card_indexes", out var _indexes) && _indexes is int[] indexes)
        {
            args.CardIndexes = indexes;
        }

        CardDropped?.Invoke(args);
    }
}
