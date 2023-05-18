using System;
using Microsoft.UI.Xaml.Controls;
using Versatile.Common;
using Versatile.Common.Cards;

namespace Versatile.Browsers.Cards;

public sealed partial class CardProductPanel : UserControl
{
    public event Action<CardSearchOptions, SearchScope> SearchRequested;

    public CardProductViewModel ViewModel { get; set; }

    public CardProductPanel()
    {
        ViewModel = VersatileApp.GetService<CardProductViewModel>();
        this.InitializeComponent();

    }

    private void ListView_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        var lvw = (ListView)sender;
        if(lvw.SelectedValue is Product product)
        {
            var options = new CardSearchOptions
            {
                ProductKeys = new[]
                {
                    product.Key
                }
            };
            SearchRequested?.Invoke(options, SearchScope.AllCards);
        }
    }
}
