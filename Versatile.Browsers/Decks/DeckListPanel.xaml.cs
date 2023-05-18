using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Versatile.Browsers.Decks;

public sealed partial class DeckListPanel : UserControl
{
    public DeckListPanel()
    {
        this.InitializeComponent();
    }

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

    private void CommandBarFlyout_Opening(object sender, object e)
    {

        if (((CommandBarFlyout)sender).Target is ListViewItem lvi && lvi.Content is DeckCardModel dcm)
        {
            ViewModel.SelectedCardEntry = dcm;
        }

    }

    private void AppBarButton_Click(object sender, RoutedEventArgs e)
    {
    }
}
