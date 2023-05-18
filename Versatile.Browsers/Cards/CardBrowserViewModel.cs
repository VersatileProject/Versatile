using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Versatile.Common;
using Versatile.Common.Cards;
using Versatile.CommonUI.Helpers;
using Versatile.Core.Services;
using Versatile.Core.Settings;
using Versatile.Localization.Services;

namespace Versatile.Browsers.Cards;

public class CardBrowserViewModel : ObservableRecipient
{
    private Card[] _searchResult;
    public Card[] SearchResult { get => _searchResult; set => SetProperty(ref _searchResult, value); }

    private Card[] _filteredResult;
    public Card[] FilteredResult { get => _filteredResult; set => SetProperty(ref _filteredResult, value); }

    private Card _selectedCard;
    public Card SelectedCard { get => _selectedCard; set => SetProperty(ref _selectedCard, value); }

    private string _searchResultHeader;
    public string SearchResultHeader { get => _searchResultHeader; set => SetProperty(ref _searchResultHeader, value); }

    public ICommand FindSameNameCommand { get; }
    public ICommand FindSamePokemonCommand { get; }
    public ICommand FindSameFamilyCommand { get; }

    private CardDataBaseService CardDataBase { get; set; }
    private LocalizationService Localization { get; set; }

    public Dictionary<int, int[]> PokemonFamilies { get; set; }

    private Dictionary<string, string> _searchResultTags;
    public Dictionary<string, string> SearchResultTags { get => _searchResultTags; set => SetProperty(ref _searchResultTags, value); }

    public CardBrowserViewModel(CardDataBaseService cardService, LocalizationService localizationService, ILocalSettingsService settings)
    {
        CardDataBase = cardService;
        Localization = localizationService;

        FindSameNameCommand = new RelayCommand<Card>(FindSameName, x => x != null);
        FindSamePokemonCommand = new RelayCommand<Card>(FindSamePokemon, x => x?.Type == CardType.Pokemon);
        FindSameFamilyCommand = new AsyncRelayCommand<Card>(FindSameFamily, x => x?.Type == CardType.Pokemon);


        var options = new CardSearchOptions();
        var searchScope = settings.ReadSetting<SearchScope>("SelectedScope");
        ApplySearchScope(options, searchScope);
        var result = CardDataBase.Search(options);
        var r = new Random();
        result = result.OrderBy(x => r.Next()).Take(30).ToArray();
        ListCards(result);

    }

    private void FindSameName(Card card)
    {
        var options = new CardSearchOptions()
        {
            CardName = card.Name,
        };
        ApplySearchScope(options, SearchScope.SelectedRegulation);
        Search(options);
    }

    private void FindSamePokemon(Card card)
    {
        if (card.Type != CardType.Pokemon || card.Pokemon == null || card.Pokemon.DexNumbers == null || card.Pokemon.DexNumbers.Length == 0)
        {
            return;
        }
        var options = new CardSearchOptions()
        {
            PokemonDexNumbers = card.Pokemon.DexNumbers.ToArray(),
        };
        ApplySearchScope(options, SearchScope.SelectedRegulation);
        Search(options);
    }

    private async Task FindSameFamily(Card card)
    {
        if (card.Type != CardType.Pokemon || card.Pokemon == null || card.Pokemon.DexNumbers == null || card.Pokemon.DexNumbers.Length == 0)
        {
            return;
        }

        if (PokemonFamilies == null)
        {
            await LoadFamilies();
        }

        var families = new List<int>();
        foreach (var num in card.Pokemon.DexNumbers)
        {
            if (PokemonFamilies.TryGetValue(num, out var family))
            {
                families.AddRange(family);
            }
        }

        var options = new CardSearchOptions()
        {
            PokemonDexNumbers = families.Distinct().ToArray(),
        };
        ApplySearchScope(options, SearchScope.SelectedRegulation);
        Search(options);
    }

    private async Task LoadFamilies()
    {
        if (PokemonFamilies != null)
        {
            return;
        }

        PokemonFamilies = new();
        using var stream = await FileService.GetStream(@"ms-appx:///Versatile.Browsers/Assets/families.json");
        using var reader = new StreamReader(stream);
        var text = reader.ReadToEnd();
        var json = JsonSerializer.Deserialize<JsonArray>(text);
        foreach (JsonObject jo in json)
        {
            PokemonFamilies.Add((int)jo["Number"], jo["Family"].AsArray().Select(x => (int)x).ToArray());
        }
    }

    public void Search(CardSearchOptions options)
    {
        var result = CardDataBase.Search(options);
        ListCards(result);
    }

    public void ListCards(Card[] cards)
    {
        if (cards.Length > 999)
        {
            cards = cards[..999];
        }

        SearchResult = cards;
        SearchResultHeader = Localization.GetString("CardBrowser/Search_ResultCount", cards.Length);

        SearchResultTags = SearchResult
            .Where(x => x.Tags != null)
            .SelectMany(x => x.Tags)
            .GroupBy(x => x)
            .ToDictionary(x => x.Key, x => $"{GetTagText(x.Key)}({x.Count()})");
    }

    private static string GetTagText(string tag)
    {
        var text = VersatileApp.Localize($"Card/Tag_{tag}");
        if (string.IsNullOrEmpty(text))
        {
            return tag;
        }
        else
        {
            return text;
        }
    }

    public void ApplySearchScope(CardSearchOptions options, SearchScope scope)
    {
        switch (scope)
        {
            case SearchScope.SelectedRegulation:
                {
                    var resulation = VersatileApp.GetService<CardProductViewModel>().SelectedRegulation;
                    var series = VersatileApp.GetService<CardDataBaseService>().GetSeries(resulation);
                    var products = series.SelectMany(x => x.Products);
                    options.ProductKeys = products.Select(x => x.Key).ToArray();
                }
                break;
            case SearchScope.SelectedSeries:
                {
                    var resulation = VersatileApp.GetService<CardProductViewModel>().SelectedRegulation;
                    var series = VersatileApp.GetService<CardProductViewModel>().SelectedSeries;
                    var products = VersatileApp.GetService<CardDataBaseService>().GetProducts(resulation, series);
                    options.ProductKeys = products.Select(x => x.Key).ToArray();
                }
                break;
            case SearchScope.SelectedProduct:
                {
                    var product = VersatileApp.GetService<CardProductViewModel>().SelectedProduct;
                    if (product != null)
                    {
                        options.ProductKeys = new[] { product.Key };
                    }
                }
                break;
            default:
                break;
        }
    }
}
