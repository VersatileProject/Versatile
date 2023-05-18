using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Versatile.Common;
using Versatile.Common.Cards;
using Versatile.Core.Helpers;
using Windows.ApplicationModel.DataTransfer;

namespace Versatile.Browsers.Cards;

public sealed partial class CardSearchPanel : UserControl
{
    public event Action<CardSearchOptions, SearchScope> SearchRequested;

    private CardSearchViewModel ViewModel { get; set; }

    private bool IsPokemonSelected { get; set; }
    private bool IsTrainerSelected { get; set; }
    private bool IsEnergySelected { get; set; }

    public CardSearchPanel()
    {
        ViewModel = VersatileApp.GetService<CardSearchViewModel>();

        this.InitializeComponent();
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        var options = new CardSearchOptions();

        var valid = false;

        if (!string.IsNullOrWhiteSpace(CardNameTextbox.Text))
        {
            options.KeywordsInName = Regex.Split(CardNameTextbox.Text, @"[\s,]+").Where(x => x.Length > 0).ToArray();
            valid |= options.KeywordsInName.Length > 0;
        }

        if (!string.IsNullOrWhiteSpace(CardTextTextbox.Text))
        {
            options.KeywordsInText = Regex.Split(CardTextTextbox.Text, @"[\s,]+").Where(x => x.Length > 0).ToArray();
            valid |= options.KeywordsInText.Length > 0;
        }

        if (PokemonToggleButton.IsChecked == true)
        {
            options.CardTypes = new[] { CardType.Pokemon };

            var colors = GetToggledValues<CardColor>(CardColorToggleButtons);
            if (colors.Length > 0)
            {
                options.PokemonColors = colors;
                valid = true;
            }

            var pt = GetToggledValues<PokemonCardStage>(PokemonStageToggleButtons);
            if (pt.Length > 0)
            {
                options.PokemonStages = pt;
            }

            var pa = GetToggledValues<AbilityType>(PokemonAbilitiesToggleButtons);
            if (pa.Length > 0)
            {
                options.Abilities = pa;
            }

            valid = true;
        }
        else if (TrainerToggleButton.IsChecked == true)
        {
            options.CardTypes = new[] { CardType.Trainer };

            var tt = GetToggledValues<TrainerCardType>(TrainerTypeToggleButtons);
            if (tt.Length > 0)
            {
                options.TrainerTypes = tt;
            }

            valid = true;
        }
        else if (EnergyToggleButton.IsChecked == true)
        {
            options.CardTypes = new[] { CardType.Energy };

            var et = GetToggledValues<EnergyCardType>(EnergyTypeToggleButtons);
            if (et.Length > 0)
            {
                options.EnergyTypes = et;
            }

            valid = true;
        }

        var tags = GetToggledValues<string>(TagToggleButtons);
        if (tags.Length > 0)
        {
            options.Tags = tags;
            valid = true;
        }

        options.Distinct = DistinctToggleSwitch.IsOn;

        if (!valid) return;

        SearchRequested?.Invoke(options, (SearchScope)ViewModel.SelectedScope);
    }

    private static T[] GetToggledValues<T>(ItemsControl control)
    {
        return control.ItemsPanelRoot.Children
            .Select(x => (ToggleButton)VisualTreeHelper.GetChild(x, 0))
            .Where(x => x.IsChecked == true)
            .Select(x => ((KeyValuePair<T, string>)x.DataContext).Key)
            .ToArray();
    }

    private void CardTypeToggleButton_Checked(object sender, RoutedEventArgs e)
    {
        if (sender != PokemonToggleButton) PokemonToggleButton.IsChecked = false;
        if (sender != TrainerToggleButton) TrainerToggleButton.IsChecked = false;
        if (sender != EnergyToggleButton) EnergyToggleButton.IsChecked = false;
        PokemonOptions.Visibility = PokemonToggleButton.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        TrainerOptions.Visibility = TrainerToggleButton.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        EnergyOptions.Visibility = EnergyToggleButton.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
    }

    private void CardTypeToggleButton_Unchecked(object sender, RoutedEventArgs e)
    {
        PokemonOptions.Visibility = PokemonToggleButton.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        TrainerOptions.Visibility = TrainerToggleButton.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        EnergyOptions.Visibility = EnergyToggleButton.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void FromClipboardButton_Click(object sender, RoutedEventArgs e)
    {
        var package = Clipboard.GetContent();
        var list = new List<string>();
        if (package.Contains(StandardDataFormats.Text))
        {
            var text = await package.GetTextAsync();

            var m1 = Regex.Match(text, @"[A-Za-z0-9]{6}-[A-Za-z0-9]{6}-[A-Za-z0-9]{6}");

            if (m1.Success)
            {
                var url = @"https://www.pokemon-card.com/deck/deck.html?deckID=" + m1.Value;
                var sourcecode = await HtmlUtil.GetSourceCode(url);
                var matches = Regex.Matches(sourcecode, @"id=""deck_.+?"" value=""(.+?)""");
                var results = matches
                    .SelectMany(x => x.Groups[1].Value.Split("-"))
                    .Select(x => x.Split("_"))
                    .ToArray();

                var database = VersatileApp.GetService<CardDataBaseService>();
                foreach (var result in results)
                {
                    var card = database.Get("JP-4-" + result[0]);
                    if (card == null) continue;
                    list.Add(card.Key);
                }
            }

        }

        if (list.Count == 0) return;

        var options = new CardSearchOptions()
        {
            CardKeys = list.ToArray(),
        };

        SearchRequested?.Invoke(options, SearchScope.AllCards);
    }

    private void TextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if(e.Key == Windows.System.VirtualKey.Enter)
        {
            SearchButton_Click(null, null);
        }
    }
}

public enum SearchScope
{
    AllCards,
    SelectedRegulation,
    SelectedSeries,
    SelectedProduct,
}
