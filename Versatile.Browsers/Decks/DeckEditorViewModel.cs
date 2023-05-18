using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Common.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls.TextToolbarSymbols;
using Versatile.Browsers.Cards;
using Versatile.Common;
using Versatile.Common.Cards;
using Versatile.CommonUI.Services;
using Versatile.Core.Services;

namespace Versatile.Browsers.Decks;

public class DeckEditorViewModel : ObservableRecipient
{
    public DeckCardModel _selectedCardEntry;
    public DeckCardModel SelectedCardEntry
    {
        get => _selectedCardEntry;
        set
        {
            SetProperty(ref _selectedCardEntry, value);
            if (value != null)
            {
                SelectedCard = value.Card;
            }
        }
    }

    public Card _selectedCard;
    public Card SelectedCard
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

    public int _pokemonCount;
    public int PokemonCount { get => _pokemonCount; set => SetProperty(ref _pokemonCount, value); }

    public int _trainerCount;
    public int TrainerCount { get => _trainerCount; set => SetProperty(ref _trainerCount, value); }

    public int _energyCount;
    public int EnergyCount { get => _energyCount; set => SetProperty(ref _energyCount, value); }

    public int _totalCount;
    public int TotalCount { get => _totalCount; set => SetProperty(ref _totalCount, value); }

    public bool _isChanged;
    public bool IsChanged { get => _isChanged; set => SetProperty(ref _isChanged, value); }

    public int _basicPokemonCount;
    public int BasicPokemonCount { get => _basicPokemonCount; set => SetProperty(ref _basicPokemonCount, value); }

    public string _basicPokemonProbability;
    public string BasicPokemonProbability { get => _basicPokemonProbability; set => SetProperty(ref _basicPokemonProbability, value); }

    public string _deckname;
    public string DeckName { get => _deckname; set { SetProperty(ref _deckname, value); IsChanged = true; } }

    public string _creator;
    public string Creator { get => _creator; set { SetProperty(ref _creator, value); IsChanged = true; } }

    public string _description;
    public string Description { get => _description; set { SetProperty(ref _description, value); IsChanged = true; } }

    public string _comments;
    public string Comments { get => _comments; set { SetProperty(ref _comments, value); IsChanged = true; } }

    public string _cardListText;
    public string CardListText { get => _cardListText; set => SetProperty(ref _cardListText, value); }

    public string _previewBackgroundPath;
    public string PreviewBackgroundPath { get => _previewBackgroundPath; set { SetProperty(ref _previewBackgroundPath, value); IsChanged = true; } }

    public List<DeckCardModel> CardSource { get; set; }

    public string DeckPath { get; set; }

    public ICommand NewDeckCommand { get; }
    public ICommand OpenDeckCommand { get; }
    public ICommand SaveDeckCommand { get; }
    public ICommand SaveAsCommand { get; }
    public ICommand SaveTemplateCommand { get; }

    public ICommand IncreaseCardCommand { get; }
    public ICommand DecreaseCardCommand { get; }
    public ICommand DeleteCardCommand { get; }

    public ICommand FindSameNameCommand { get; }
    public ICommand FindSamePokemonCommand { get; }
    public ICommand FindSameFamilyCommand { get; }
    public ICommand CommentCommand { get; }

    private CardDataBaseService CardService { get; }

    public CardGroup<CardType> PokemonGroup { get; set; }
    public CardGroup<TrainerCardType> TrainerGroup { get; set; }
    public CardGroup<EnergyCardType> EnergyGroup { get; set; }

    public ObservableCollection<Card> DeletedCards { get; set; }

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

        public DeckCardModel Add(Card card, int number)
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
            return entry;
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

    public DeckEditorViewModel(CardDataBaseService cardService, HookService hookService)
    {
        CardService = cardService;

        CardSource = new();
        DeletedCards = new();

        PokemonGroup = new(CardSource, m => CardType.Pokemon);
        TrainerGroup = new(CardSource, m => m.Trainer.Type);
        EnergyGroup = new(CardSource, m => m.Energy.Type);

        NewDeckCommand = new AsyncRelayCommand(NewDeck);
        OpenDeckCommand = new RelayCommand(OpenDeck);
        SaveDeckCommand = new AsyncRelayCommand(SaveDeck);
        SaveAsCommand = new AsyncRelayCommand(SaveAs);
        SaveTemplateCommand = new AsyncRelayCommand(SaveTemplate);

        IncreaseCardCommand = new RelayCommand(IncreaseCard);
        DecreaseCardCommand = new RelayCommand(DecreasedCard);
        DeleteCardCommand = new RelayCommand(DeleteCard);

        FindSameNameCommand = new RelayCommand<Card>(FindSameName, x => x != null);
        FindSamePokemonCommand = new RelayCommand<Card>(FindSamePokemon, x => x?.Type == CardType.Pokemon);
        FindSameFamilyCommand = new RelayCommand<Card>(FindSameFamily, x => x?.Type == CardType.Pokemon);

        CommentCommand = new AsyncRelayCommand<DeckCardModel>(CommentCardAsync);

        hookService.Register<MainWindowClosingArguments>(async (args) =>
        {
            if (IsChanged)
            {
                VersatileApp.NavigateTo(PageKey.Deck);
            }

            if (await CheckChange() == false)
            {
                args.Cancel = true;
            }
        });
    }

    public async Task NewDeck()
    {
        if (await CheckChange() == false)
        {
            return;
        }

        Clear();
        CheckCount();

        IsChanged = false;
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
        DeletedCards.Clear();

        BasicPokemonCount = 0;
        BasicPokemonProbability = $"{0:P0}";
        DeckPath = null;
        DeckName = "";
        Creator = "";
        Description = "";
        Comments = "";
        PreviewBackgroundPath = "";
        IsChanged = false;
    }

    public Deck ToDeck()
    {
        var deck = new Deck
        {
            Cards = CardSource.Select(x => new DeckCardEntry()
            {
                Key = x.Card.Key,
                Quantity = x.Quantity,
                Name = x.Card.DisplayName,
                Comment = x.CommentMessage,

            }).ToArray(),
            Name = DeckName,
            Creator = Creator,
            Description = Description,
            Comments = Comments,
            PreviewBackgroundPath = PreviewBackgroundPath,
            DeletedCards = DeletedCards.Select(x => x.Key).ToArray()
        };
        return deck;
    }

    public DeckCardModel AddCard(Card card, int number = 1, bool updateui = true)
    {
        var model = card.Type switch
        {
            CardType.Pokemon => PokemonGroup.Add(card, number),
            CardType.Trainer => TrainerGroup.Add(card, number),
            CardType.Energy => EnergyGroup.Add(card, number),
            _ => null,
        };

        if (updateui)
        {
            CheckCount();
        }

        if (DeletedCards.Contains(card))
        {
            DeletedCards.Remove(card);
        }

        IsChanged = true;

        return model;
    }

    public void DeleteCard(Card card)
    {
        if (!CardSource.Any(x => x.Card == card)) return;
        if (card.Type == CardType.Pokemon) PokemonGroup.Remove(card);
        else if (card.Type == CardType.Trainer) TrainerGroup.Remove(card);
        else if (card.Type == CardType.Energy) EnergyGroup.Remove(card);
        CheckCount();

        if (DeletedCards.Contains(card))
        {
            DeletedCards.Remove(card);
        }
        if(DeletedCards.Count > 9)
        {
            for (var i = 0; i < DeletedCards.Count - 9 - 1; i++)
            {
                DeletedCards.RemoveAt(0);
            }
        }
        DeletedCards.Add(card);

        IsChanged = true;
    }

    private async void OpenDeck()
    {
        if (await CheckChange() == false)
        {
            return;
        }

        var fs = VersatileApp.GetService<DialogService>();
        var unknownCards = new StringBuilder();
        await fs.TryOpen(new[] { ".deck", ".decktemplate" }, (path) =>
        {
            var deck = Deck.Load(path);
            if (deck != null)
            {
                Clear();
                DeckPath = path;

                foreach (var entry in deck.Cards)
                {
                    var cardkey = entry.Key;
                    var card = CardService.Get(cardkey);
                    if (card == null)
                    {
                        unknownCards.AppendLine($"- {entry.Key}({entry.Name})");
                        continue;
                    }
                    var model = AddCard(card, entry.Quantity, false);
                    if (model == null)
                    {
                        unknownCards.AppendLine($"- {entry.Key}({entry.Name})");
                        continue;
                    }
                    if (!string.IsNullOrEmpty(entry.Comment))
                    {
                        model.CommentMessage = entry.Comment;
                    }
                }

                DeckName = deck.Name;
                Creator = deck.Creator;
                Description = deck.Description;
                Comments = deck.Comments;
                PreviewBackgroundPath = deck.PreviewBackgroundPath;

                if(deck.DeletedCards?.Length > 0)
                {
                    foreach (var cardkey in deck.DeletedCards)
                    {
                        var card = CardService.Get(cardkey);
                        if (card == null) continue;
                        if (CardSource.Any(x => x.Card.Key == cardkey)) continue;
                        DeletedCards.Add(card);
                    }
                }

                CheckCount();

                IsChanged = false;
            }
        });

        if (unknownCards.Length > 0)
        {
            var dialog = VersatileApp.GetService<DialogService>();
            await dialog.ShowOk("Unknown cards", unknownCards.ToString());
        }
    }

    private async Task SaveDeck()
    {
        if (!IsChanged) return;

        if (DeckPath != null && !DeckPath.EndsWith(".decktemplate") && File.Exists(DeckPath))
        {
            SaveDeck(DeckPath);
        }
        else
        {
            var fs = VersatileApp.GetService<DialogService>();
            await fs.TrySave(new[] { ("Deck file", ".deck") }, "NewDeck", SaveDeck);
        }
    }

    private async Task SaveAs()
    {
        var fs = VersatileApp.GetService<DialogService>();
        await fs.TrySave(new[] { ("Deck file", ".deck") }, "NewDeck", SaveDeck);
    }

    private async Task SaveTemplate()
    {
        var fs = VersatileApp.GetService<DialogService>();
        await fs.TrySave(new[] { ("Deck Template", ".decktemplate") }, "NewDeck", SaveDeck);
    }

    private void SaveDeck(string path)
    {
        var deck = ToDeck();
        deck.Save(path);
        DeckPath = path;
        IsChanged = false;
    }

    public async Task<bool> CheckChange()
    {
        if (!IsChanged) return true;

        var ds = VersatileApp.GetService<DialogService>();
        var result = await ds.ShowYesNoCancel(VersatileApp.Localize("DeckEditor/Dialog_DeckChangedTitle"), VersatileApp.Localize("DeckEditor/Dialog_DeckChangedText"));
        if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            await SaveDeck();
            return !IsChanged;
        }
        else if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Secondary)
        {
            return true;
        }

        return false;
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

    private async Task CommentCardAsync(DeckCardModel card)
    {
        if (card == null)
        {
            return;
        }

        var dialog = VersatileApp.GetService<DialogService>();
        var comment = await dialog.ShowInput(card.Card.DisplayName, card.CommentMessage);
        if (comment == null) return;
        card.CommentMessage = comment;
        IsChanged = true;
    }

    public void UpdateCardList()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"{VersatileApp.Localize("Card/CardType_Pokemon")} ({PokemonGroup.Count})");
        sb.AppendLine("----------------");
        foreach (var group in PokemonGroup.ContactSource)
        {
            foreach (var entry in group)
            {
                sb.AppendLine($"- {entry.Quantity}×「{entry.Card.DisplayName}」");
            }
        }
        sb.AppendLine();

        sb.AppendLine($"{VersatileApp.Localize("Card/CardType_Trainer")} ({TrainerGroup.Count})");
        sb.AppendLine("----------------");
        foreach (var group in TrainerGroup.ContactSource)
        {
            foreach (var entry in group)
            {
                sb.AppendLine($"- {entry.Quantity}×「{entry.Card.DisplayName}」");
            }
        }
        sb.AppendLine();

        sb.AppendLine($"{VersatileApp.Localize("Card/CardType_Energy")} ({EnergyGroup.Count})");
        sb.AppendLine("----------------");
        foreach (var group in EnergyGroup.ContactSource)
        {
            foreach (var entry in group)
            {
                sb.AppendLine($"- {entry.Quantity}×「{entry.Card.DisplayName}」");
            }
        }
        sb.AppendLine();

        CardListText = sb.ToString();
    }

    private void FindSameName(Card card)
    {
        VersatileApp.GetService<CardBrowserViewModel>().FindSameNameCommand.Execute(card);
        VersatileApp.NavigateTo(PageKey.Card);
    }

    private void FindSamePokemon(Card card)
    {
        VersatileApp.GetService<CardBrowserViewModel>().FindSamePokemonCommand.Execute(card);
        VersatileApp.NavigateTo(PageKey.Card);
    }

    private void FindSameFamily(Card card)
    {
        VersatileApp.GetService<CardBrowserViewModel>().FindSameFamilyCommand.Execute(card);
        VersatileApp.NavigateTo(PageKey.Card);
    }

}
