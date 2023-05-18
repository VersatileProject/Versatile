using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Versatile.Common;
using Versatile.Common.Cards;
using Versatile.Core.Services;
using Versatile.Core.Settings;

namespace Versatile.Browsers.Cards;

public class CardProductViewModel : ObservableRecipient
{
    public Regulation[] Regulations { get; set; }

    private Series[] _series;
    public Series[] Series { get => _series; set => SetProperty(ref _series, value); }

    private Regulation _selectedRegulation;
    public Regulation SelectedRegulation { get => _selectedRegulation; set => SetProperty(ref _selectedRegulation, value); }

    private Series _selectedSeries;
    public Series SelectedSeries { get => _selectedSeries; set => SetProperty(ref _selectedSeries, value); }

    private Product _selectedProduct;
    public Product SelectedProduct { get => _selectedProduct; set => SetProperty(ref _selectedProduct, value); }

    private IEnumerable<IGrouping<ProductType, Product>> _productGroups;
    public IEnumerable<IGrouping<ProductType, Product>> ProductGroups { get => _productGroups; set => SetProperty(ref _productGroups, value); }

    private CardDataBaseService DataBase { get; set; }

    public CardProductViewModel(CardDataBaseService database, ILocalSettingsService settings, HookService hook)
    {
        DataBase = database;
        Regulations = DataBase.Regulations.ToArray();

        PropertyChanged += CardProductViewModel_PropertyChanged;

        var regulationKey = settings.ReadSetting<string>(nameof(SelectedRegulation));
        var regulation = Regulations.FirstOrDefault(x => x.Key == regulationKey);
        if (regulation != null)
        {
            SelectedRegulation = regulation;
        }

        var seriesKey = settings.ReadSetting<string>(nameof(SelectedSeries));
        var series = Series?.FirstOrDefault(x => x.Key == seriesKey);
        if (regulation != null)
        {
            SelectedSeries = series;
        }

        hook.Register<MainWindowClosedArguments>(args =>
        {
            settings.SaveSetting(nameof(SelectedRegulation), SelectedRegulation?.Key);
            settings.SaveSetting(nameof(SelectedSeries), SelectedSeries?.Key);
        });
    }

    private void CardProductViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedRegulation) when SelectedRegulation != null:
                var selectedSeries = SelectedSeries;
                var selectedProduct = SelectedProduct;
                Series = DataBase.GetSeries(SelectedRegulation).OrderBy(GetSeriesDate).ToArray();
                if (Series.Contains(selectedSeries))
                {
                    SelectedSeries = selectedSeries;
                    if (SelectedSeries.Products.Contains(selectedProduct))
                    {
                        SelectedProduct = selectedProduct;
                    }
                }
                else
                {
                    SelectedSeries = Series.LastOrDefault();
                }
                break;
            case nameof(SelectedSeries) when SelectedRegulation != null && SelectedSeries != null:
                var products = DataBase.GetProducts(SelectedRegulation, SelectedSeries);
                ProductGroups = products
                    .OrderByDescending(x => x.ReleaseDate)
                    .GroupBy(x => x.Type)
                    .OrderBy(x => x.Key)
                    ;
                break;
        }
    }

    private static DateTime GetSeriesDate(Series series)
    {
        if(series.ReleaseDate != default)
        {
            return series.ReleaseDate;
        }
        else
        {
            return series.Products.Where(x => x.ReleaseDate != default).Min(x => x.ReleaseDate);
        }
    }
}
