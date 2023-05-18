using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Versatile.Browsers.Decks;

namespace Versatile.Browsers.Decks;

public sealed partial class DeckInfoPanel : UserControl
{
    public DeckEditorViewModel ViewModel
    {
        get => (DeckEditorViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty
        = DependencyProperty.Register(
              "ViewModel",
              typeof(DeckEditorViewModel),
              typeof(DeckListPanel),
              new PropertyMetadata(null)
          );

    public DeckInfoPanel()
    {
        this.InitializeComponent();
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        //ViewModel.IsChanged = true;
    }
}
