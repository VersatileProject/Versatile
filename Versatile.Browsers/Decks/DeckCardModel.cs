using CommunityToolkit.Mvvm.ComponentModel;
using Versatile.Common.Cards;

namespace Versatile.Browsers.Decks;

public class DeckCardModel : ObservableObject
{
    public string Key { get; set; }
    public Card Card { get; set; }
    public string SortKey { get; set; }

    private int _quantity;
    public int Quantity { get => _quantity; set => SetProperty(ref _quantity, value); }

    private string _errorMessage;
    public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }

    private string _commentMessage;
    public string CommentMessage { get => _commentMessage; set => SetProperty(ref _commentMessage, value); }
}
