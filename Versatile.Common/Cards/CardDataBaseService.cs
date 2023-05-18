using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using Versatile.Core;

namespace Versatile.Common.Cards;

public class CardDataBaseService
{
    public TimeSpan LoadingDuration { get; private set; }

    private CardCollection DataBase { get; }
    public Regulation[] Regulations { get; set; }
    public Series[] Series { get; set; }

    private string[] searchFolders = new[]{
        VersatileApp.StartupPath,
        VersatileApp.DocumentPath,
    };

    public int CardCount => DataBase.Count;

    public CardDataBaseService()
    {
        DataBase = new();

        var sw = new Stopwatch();
        sw.Start();

        LoadRegulations();
        LoadSeries();
        LoadCards();

        LoadLocalization();

        sw.Stop();
        LoadingDuration = sw.Elapsed;
    }

    public IEnumerable<Card> AsEnumerable() => DataBase.AsEnumerable();

    private void LoadRegulations()
    {
        var files = FindFiles("REGULATION_*.json");
        var list = new List<Regulation>();
        foreach(var file in files)
        {
            try
            {
                using var fs = File.OpenRead(file);
                list.AddRange(JsonSerializer.Deserialize<Regulation[]>(fs)!);
            }
            catch (Exception e)
            {
            }
        }
        Regulations = list.ToArray();
    }

    private void LoadSeries()
    {
        var files = FindFiles("SERIES_*.json");
        var list = new List<Series>();
        foreach (var file in files)
        {
            try
            {
                var series = JsonUtil.DeserializeFromFile<Series>(file);
                list.Add(series!);
            }
            catch(Exception e)
            {
            }
        }
        Series = list.ToArray();
    }

    private void LoadLocalization()
    {
        var folders = searchFolders
            .Select(x => Path.Combine(x, "Assets", "Localization"))
            .Where(Directory.Exists)
            .SelectMany(Directory.GetDirectories)
            .Select(x => (Path:x, Lang: Path.GetFileName(x)))
            .ToArray();


        var lcid = Thread.CurrentThread.CurrentUICulture.Name;
        var i = Array.FindIndex(folders, f => f.Lang == lcid);
        if (i == -1)
        {
            lcid = lcid.Split("-")[0];
            i = Array.FindIndex(folders, f => f.Lang.StartsWith(lcid));
            if (i == -1)
            {
                return;
            }
        }

        var regulationPaths = Directory.GetFiles(folders[i].Path, "REGULATION_*.json");
        foreach (var path in regulationPaths)
        {
            var regulationDictionary = JsonUtil.DeserializeFromFile<Dictionary<string, string>>(path);
            foreach(var (key, value) in regulationDictionary)
            {
                var parts = key.Split('.');
                var regulation = Regulations.FirstOrDefault(x => x.Key == parts[0]);
                if (regulation == null)
                {
                    continue;
                }
                switch (parts[1])
                {
                    case "Title":
                        regulation.Title = value;
                        break;
                }
            }
        }

        var seriesPaths = Directory.GetFiles(folders[i].Path, "SERIES_*.json");
        foreach (var seriesPath in seriesPaths)
        {
            var seriesKey = Path.GetFileNameWithoutExtension(seriesPath);
            var series = Series.FirstOrDefault(x => x.Key == seriesKey);
            if (series == null)
            {
                continue;
            }
            var seriesDictionary = JsonUtil.DeserializeFromFile<Dictionary<string, string>>(seriesPath);
            foreach (var (key, value) in seriesDictionary)
            {
                var parts = key.Split('.');
                if (parts.Length != 2) continue;
                if (parts[0].StartsWith("SERIES_"))
                {
                    switch (parts[1])
                    {
                        case "Title":
                            series.Title = value;
                            break;
                        case "ShortTitle":
                            series.ShortTitle = value;
                            break;
                    }
                }
                else if (parts[0].StartsWith("PRODUCT_"))
                {
                    var product = series.Products?.FirstOrDefault(x => x.Key == parts[0]);
                    if (product == null)
                    {
                        continue;
                    }
                    switch (parts[1])
                    {
                        case "Title":
                            product.Title = value;
                            break;
                    }
                }
            }
        }

        var cardPaths = Directory.GetFiles(folders[i].Path, "CARD*.json");
        foreach (var cardPath in cardPaths)
        {
            var cardDictionary = JsonUtil.DeserializeFromFile<Dictionary<string, string>>(cardPath);
            foreach (var (key, value) in cardDictionary)
            {
                var parts = key.Split('.');
                if (!DataBase.TryGetValue(parts[0], out var card)) continue;
                switch (parts[1])
                {
                    case "Name":
                        card.Name = value;
                        break;
                    case "Subname":
                        card.Subname = value;
                        break;
                    case var x when x.StartsWith("Abilities"):
                        {
                            var ai = int.Parse(x[10].ToString());
                            switch (parts[2])
                            {
                                case "Name":
                                    card.Abilities[ai].Name = value;
                                    break;
                                case "Text":
                                    card.Abilities[ai].Text = value;
                                    break;
                            }
                        }
                        break;
                }
            }
        }
    }

    public Series[] GetSeries(Regulation regulation)
    {
        var list = new List<Series>();
        foreach(var series in Series)
        {
            if(regulation?.Series?.Contains(series.Key) == false)
            {
                continue;
            }

            list.Add(series);
        }
        return list.ToArray();
    }

    public Product[] GetProducts(Regulation regulation, Series series)
    {
        var list = new List<Product>();
        foreach (var product in series.Products)
        {
            if (regulation.BeginDate != default && product.ReleaseDate != default && product.ReleaseDate < regulation.BeginDate)
            {
                continue;
            }
            if (regulation.EndDate != default && product.ReleaseDate != default && product.ReleaseDate > regulation.EndDate)
            {
                continue;
            }

            list.Add(product);
        }
        return list.ToArray();
    }

    public Product GetProduct(string productKey)
    {
        foreach(var series in Series)
        {
            foreach (var product in series.Products)
            {
                if(product.Key == productKey)
                {
                    return product;
                }
            }
        }
        return null;
    }

    private string[] FindFiles(string pattern)
    {
        var list = new List<string>();
        foreach (var folder in searchFolders)
        {
            var datafolder = Path.Combine(folder, "Assets", "Data");
            if (!Directory.Exists(datafolder))
            {
                continue;
            }

            var files = Directory.GetFiles(datafolder, pattern);
            list.AddRange(files);

            if (files.Length > 0) break;
        }

        return list.ToArray();
    }

    private void LoadCards()
    {
        var files = FindFiles(VersatileApp.DevMode ? "dev.cards.json" : "card*.json");

        foreach(var file in files)
        {
            DataBase.Load(file);
        }
    }

    public Card? Get(string key)
    {
        return DataBase.TryGetValue(key, out var card) ? card : null;
    }

    public Card[] Search(CardSearchOptions options)
    {
        var query = AsEnumerable();

        var cardkeys = options.CardKeys;
        if((cardkeys == null || cardkeys.Length == 0) && options.ProductKeys?.Length > 0)
        {
            var products = options.ProductKeys
                .Select(GetProduct)
                .ToArray();

            if (products.Length > 0)
            {
                var productkeys = new HashSet<string>(products.Select(x => x.Key).ToArray());
                query = query.Where(c =>
                {
                    return c.ProductKeys?.Any(x => productkeys.Contains(x)) ?? false;
                });
            }

        }

        if (cardkeys?.Length > 0)
        {
            var hs = new HashSet<string>(cardkeys);
            query = query.Where(c =>
            {
                return hs.Contains(c.Key);
            });
        }


        if (options.CardName != null)
        {
            query = query.Where(c =>
            {
                return c.Name == options.CardName;
            });
        }
        else if (options.KeywordsInName?.Length > 0)
        {
            query = query.Where(c =>
            {
                return options.KeywordsInName.All(y => c.Name.Contains(y));
                //return options.KeywordsInName.Any(y => c.Name.Contains(y));
            });
        }

        if (options.KeywordsInText?.Length > 0)
        {
            query = query.Where(c =>
            {
                return c.Abilities != null && options.KeywordsInText.All(x => c.Abilities.Any(y => y.Name?.Contains(x) == true || y.Text?.Contains(x) == true));
                //return c.Abilities != null && c.Abilities.Any(z => options.KeywordsInText.Any(y => z.Text.Contains(y)));
            });
        }

        if (options.CardTypes?.Length > 0)
        {
            query = query.Where(c =>
            {
                return options.CardTypes.Contains(c.Type);
            });
        }

        if (options.PokemonColors?.Length > 0)
        {
            query = query.Where(c =>
            {
                return c.Pokemon != null && c.Pokemon.Colors.Any(x => options.PokemonColors.Contains(x));
            });
        }

        if (options.PokemonStages?.Length > 0)
        {
            query = query.Where(c =>
            {
                return c.Pokemon != null && options.PokemonStages.Contains(c.Pokemon.Stage);
            });
        }

        if (options.PokemonDexNumbers?.Length > 0)
        {
            query = query.Where(c =>
            {
                return c.Pokemon != null && c.Pokemon.DexNumbers != null && c.Pokemon.DexNumbers.Any(x => options.PokemonDexNumbers.Contains(x));
            });
        }

        if (options.TrainerTypes?.Length > 0)
        {
            query = query.Where(c =>
            {
                return c.Trainer != null && options.TrainerTypes.Contains(c.Trainer.Type);
            });
        }

        if (options.EnergyTypes?.Length > 0)
        {
            query = query.Where(c =>
            {
                return c.Energy != null && options.EnergyTypes.Contains(c.Energy.Type);
            });
        }

        if (options.Abilities?.Length > 0)
        {
            query = query.Where(c =>
            {
                return c.Abilities?.Length > 0 && c.Abilities.Any(x => options.Abilities.Contains(x.Type));
            });
        }

        if (options.Tags?.Length > 0)
        {
            query = query.Where(c =>
            {
                return c.Tags?.Length > 0 && c.Tags.Any(x => options.Tags.Contains(x));
            });
        }

        if (options.Distinct)
        {
            query = query.DistinctBy(x => x.UniqueEffectId);
        }

        return query.ToArray();
    }

    public string GetCardImageUri(Card card)
    {
        var parts = card.Key.Split('_');
        foreach (var folder in searchFolders)
        {
            var filepath = Path.Combine(folder, "Assets", "CardImages", parts[1], parts[2], card.Key + ".jpg");
            if (File.Exists(filepath))
            {
                return filepath;
            }
        }
        return card.Image;
    }
}
