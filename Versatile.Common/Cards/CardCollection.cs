using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Versatile.Common.Cards;

public class CardCollection : KeyedCollection<string, Card>
{
    protected override string GetKeyForItem(Card item) => item.Key;

    public void Load(string filepath)
    {
        using var fs = File.OpenRead(filepath);
        var jsonOptions = new JsonSerializerOptions()
        {
            IncludeFields = true,
            Converters =
            {
                new JsonStringEnumConverter(),
            },
        };
        try
        {
            var cards = JsonSerializer.Deserialize<Card[]>(fs, jsonOptions);
            foreach(var card in cards)
            {
                this.Add(card);
            }
        }
        catch
        {
        }
    }
}
