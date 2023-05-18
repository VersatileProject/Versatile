using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Versatile.Common.Cards;

public class Deck
{
    public DeckCardEntry[] Cards { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
    public string Creator { get; set; }
    public string Comments { get; set; }

    public string Format { get; set; }

    public string[] DeletedCards { get; set; }

    public string PreviewBackgroundPath { get; set; }

    public Deck()
    {

    }

    public static Deck Load(string path)
    {
        var text = File.ReadAllText(path);
        var deck = JsonSerializer.Deserialize<Deck>(text);
        return deck;
    }

    public void Save(string path)
    {
        var options = new JsonSerializerOptions()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
        var text = JsonSerializer.Serialize(this, options);
        File.WriteAllText(path, text);
    }

    public int GetCardCount()
    {
        return Cards.Sum(c => c.Quantity);
    }

}
