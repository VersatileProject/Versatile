using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Common.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Versatile.Common.Cards;

namespace Versatile.Browsers.Decks;

public class DeckListViewModel : ObservableRecipient
{
    public DeckCardModel _selectedCard;
    public DeckCardModel SelectedCardEntry
    {
        get => _selectedCard;
        set => SetProperty(ref _selectedCard, value);
    }

    public DeckCardModel _selectedPokemonCard;
    public DeckCardModel SelectedPokemonCardEntry
    {
        get => _selectedPokemonCard;
        set
        {
            SetProperty(ref _selectedPokemonCard, value);
            if (value != null)
            {
                SelectedTrainerCardEntry = null;
                SelectedEnergyCardEntry = null;
                SelectedCardEntry = value;
            }
        }
    }

    public DeckCardModel _selectedTrainerCard;
    public DeckCardModel SelectedTrainerCardEntry
    {
        get => _selectedTrainerCard;
        set
        {
            SetProperty(ref _selectedTrainerCard, value);
            if (value != null)
            {
                SelectedPokemonCardEntry = null;
                SelectedEnergyCardEntry = null;
                SelectedCardEntry = value;
            }
        }
    }

    public DeckCardModel _selectedEnergyCard;
    public DeckCardModel SelectedEnergyCardEntry
    {
        get => _selectedEnergyCard;
        set
        {
            SetProperty(ref _selectedEnergyCard, value);
            if (value != null)
            {
                SelectedPokemonCardEntry = null;
                SelectedTrainerCardEntry = null;
                SelectedCardEntry = value;
            }
        }
    }

    public List<DeckCardModel> CardSource { get; set; }

    public int _pokemonCount;
    public int PokemonCount { get => _pokemonCount; set => SetProperty(ref _pokemonCount, value); }

    public int _trainerCount;
    public int TrainerCount { get => _trainerCount; set => SetProperty(ref _trainerCount, value); }

    public int _energyCount;
    public int EnergyCount { get => _energyCount; set => SetProperty(ref _energyCount, value); }

    public int _totalCount;
    public int TotalCount { get => _totalCount; set => SetProperty(ref _totalCount, value); }

    public int _basicPokemonCount;
    public int BasicPokemonCount { get => _basicPokemonCount; set => SetProperty(ref _basicPokemonCount, value); }

    public string _basicPokemonProbability;
    public string BasicPokemonProbability { get => _basicPokemonProbability; set => SetProperty(ref _basicPokemonProbability, value); }

    public string DeckPath { get; set; }

    public ICommand IncreaseCardCommand { get; }
    public ICommand DecreaseCardCommand { get; }
    public ICommand DeleteCardCommand { get; }

    private CardDataBaseService CardService { get; }

    public class CardGroup<T> where T : struct, Enum
    {
        public ObservableGroupedCollection<T, DeckCardModel> ContactSource { get; set; }
        public ReadOnlyObservableGroupedCollection<T, DeckCardModel> Contact { get; set; }

        public int Count { get; set; }

        private readonly Func<Card, T> KeySelector;
        private List<DeckCardModel> CardSource;

        public CardGroup(List<DeckCardModel> source, Func<Card, T> keySelector)
        {
            ContactSource = new();
            Contact = new(ContactSource);

            KeySelector = keySelector;
            CardSource = source;
        }

        public void Clear()
        {
            ContactSource.Clear();
            Count = 0;
        }

        public void Add(Card card, int number)
        {
            var entry = CardSource.Find(x => x.Card == card);
            if (entry != null)
            {
                entry.Quantity += number;
            }
            else
            {
                entry = new DeckCardModel()
                {
                    Card = card,
                    Quantity = number,
                    SortKey = card.SortKey,
                };
                var groupkey = KeySelector(card);
                var group = ContactSource.FirstOrDefault(group => group.Key.Equals(groupkey));
                if (group is null)
                {
                    var index = ContactSource.TakeWhile(x => x.Key.CompareTo(groupkey) < 0).Count();
                    ContactSource.Insert(index, new(groupkey, new[] { entry }));
                }
                else
                {
                    var index = group.TakeWhile(x => x.SortKey.CompareTo(entry.SortKey) < 0).Count();
                    group.Insert(index, entry);
                }

                CardSource.Add(entry);
            }
            Count += number;
        }

        public void Remove(Card card)
        {
            var groupkey = KeySelector(card);
            var group = ContactSource.FirstOrDefault(group => group.Key.Equals(groupkey));
            var entry = group.FirstOrDefault(x => x.Card == card);
            Count -= entry.Quantity;
            group.Remove(entry);
            if (group.Count == 0) ContactSource.RemoveGroup(group.Key);

            CardSource.Remove(entry);
        }
    }

    public CardGroup<CardType> PokemonGroup { get; set; }
    public CardGroup<TrainerCardType> TrainerGroup { get; set; }
    public CardGroup<EnergyCardType> EnergyGroup { get; set; }

    public DeckListViewModel(CardDataBaseService cardService)
    {
        CardService = cardService;

        CardSource = new();

        PokemonGroup = new(CardSource, m => CardType.Pokemon);
        TrainerGroup = new(CardSource, m => m.Trainer.Type);
        EnergyGroup = new(CardSource, m => m.Energy.Type);

        IncreaseCardCommand = new RelayCommand(IncreaseCard);
        DecreaseCardCommand = new RelayCommand(DecreasedCard);
        DeleteCardCommand = new RelayCommand(DeleteCard);
    }

    public void CheckCount()
    {
        PokemonCount = PokemonGroup.Count;
        TrainerCount = TrainerGroup.Count;
        EnergyCount = EnergyGroup.Count;
        TotalCount = PokemonCount + TrainerCount + EnergyCount;

        var checker = new DeckChecker();
        var entries = CardSource.Select(x => new DeckChecker.Entry(x.Card, x.Quantity)).ToArray();
        var result = checker.Check(entries);
        BasicPokemonCount = result.BasicPokemon;
        BasicPokemonProbability = result.BasicPokemonProbability.ToString("P0");
        foreach (var card in CardSource)
        {
            card.ErrorMessage = result.ErrorCards.FirstOrDefault(x => x.card == card.Card).Error;
        }
    }

    public void Clear()
    {
        PokemonGroup.Clear();
        TrainerGroup.Clear();
        EnergyGroup.Clear();
        CardSource.Clear();

        BasicPokemonCount = 0;
        BasicPokemonProbability = "";
    }

    public void AddCard(Card card, int number = 1, bool updateui = true)
    {
        if (card.Type == CardType.Pokemon) PokemonGroup.Add(card, number);
        else if (card.Type == CardType.Trainer) TrainerGroup.Add(card, number);
        else if (card.Type == CardType.Energy) EnergyGroup.Add(card, number);
        if (updateui)
        {
            CheckCount();
        }
    }

    public void DeleteCard(Card card)
    {
        if (card.Type == CardType.Pokemon) PokemonGroup.Remove(card);
        else if (card.Type == CardType.Trainer) TrainerGroup.Remove(card);
        else if (card.Type == CardType.Energy) EnergyGroup.Remove(card);
        CheckCount();
    }

    private void IncreaseCard()
    {
        if (SelectedCardEntry != null)
        {
            AddCard(SelectedCardEntry.Card, 1);
        }
    }

    private void DecreasedCard()
    {
        if (SelectedCardEntry != null && SelectedCardEntry.Quantity > 1)
        {
            AddCard(SelectedCardEntry.Card, -1);
        }
    }

    private void DeleteCard()
    {
        if (SelectedCardEntry != null)
        {
            DeleteCard(SelectedCardEntry.Card);
        }
    }

}
