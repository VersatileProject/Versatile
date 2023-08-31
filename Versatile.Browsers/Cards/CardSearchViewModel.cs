using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Versatile.Common;
using Versatile.Common.Cards;
using Versatile.Core.Services;
using Versatile.Core.Settings;

namespace Versatile.Browsers.Cards;

public class CardSearchViewModel : ObservableRecipient
{
    public Dictionary<CardColor, string> SearchColors { get; set; }
    public Dictionary<string, string> SearchTags { get; set; }

    public Dictionary<PokemonCardStage, string> SearchPokemonStages { get; set; }
    public Dictionary<TrainerCardType, string> SearchTrainerTypes { get; set; }
    public Dictionary<EnergyCardType, string> SearchEnergyTypes { get; set; }
    public Dictionary<AbilityType, string> SearchPokemonAbilities { get; set; }
    public Dictionary<int, string> SearchScopes { get; set; }

    private int selectedScope;
    public int SelectedScope { get => selectedScope; set => SetProperty(ref selectedScope, value); }

    public CardSearchViewModel(CardDataBaseService database, ILocalSettingsService settings, HookService hook)
    {
        SearchColors = GetEnumValues<CardColor>().ToDictionary(x => x, x => VersatileApp.Localize(x, "Card"));
        SearchTags = database.AsEnumerable().Where(x => x.Tags != null).SelectMany(x => x.Tags).Distinct().Where(x => !x.StartsWith("_")).ToDictionary(x => x, GetTagText);
        SearchPokemonStages = GetEnumValues<PokemonCardStage>().ToDictionary(x => x, x => VersatileApp.Localize(x, "Card"));
        SearchTrainerTypes = GetEnumValues<TrainerCardType>().ToDictionary(x => x, x => VersatileApp.Localize(x, "Card"));
        SearchEnergyTypes = GetEnumValues<EnergyCardType>().ToDictionary(x => x, x => VersatileApp.Localize(x, "Card"));
        SearchPokemonAbilities = GetEnumValues<AbilityType>().Where(x => x is not AbilityType.Unknown and not AbilityType.Effect and not AbilityType.Rule).ToDictionary(x => x, x => VersatileApp.Localize(x, "Card"));

        SearchScopes = GetEnumValues<SearchScope>(true).ToDictionary(x => (int)x, x => VersatileApp.Localize(x, "CardBrowser"));

        var searchScope = settings.ReadSetting<SearchScope>(nameof(SelectedScope));
        SelectedScope = (int)searchScope;

        hook.Register<MainWindowClosedArguments>(args =>
        {
            var selectScope = (SearchScope)SelectedScope;
            settings.SaveSetting(nameof(SelectedScope), selectScope);
        });
    }

    private static T[] GetEnumValues<T>(bool withDefault = false) where T : struct, Enum
    {
        if(withDefault)
        {
            return Enum.GetValues<T>().ToArray();
        }
        else
        {
            return Enum.GetValues<T>().Where(x => !x.Equals(default(T))).ToArray();
        }
    }

    private static string GetTagText(string tag)
    {
        if (tag.StartsWith("_")) return "";

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
}
