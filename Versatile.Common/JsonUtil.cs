using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Versatile.Core;

internal static class JsonUtil
{
    private static JsonSerializerOptions JsonOptions = new JsonSerializerOptions()
    {
        WriteIndented = true,
        IncludeFields = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        IgnoreReadOnlyFields = true,
        IgnoreReadOnlyProperties = true,
        Converters =
        {
            new JsonStringEnumConverter(),
        },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    public static string Serialize(object data)
    {
        return JsonSerializer.Serialize(data, JsonOptions);
    }

    public static T Deserialize<T>(string text)
    {
        return JsonSerializer.Deserialize<T>(text, JsonOptions);
    }

    public static T DeserializeFromFile<T>(string filename)
    {
        var fs = File.OpenRead(filename);
        return JsonSerializer.Deserialize<T>(fs, JsonOptions);
    }


    public static T ReadOrCreate<T>(string path) where T : new()
    {
        if (File.Exists(path))
        {
            var fs = File.OpenRead(path);
            return JsonSerializer.Deserialize<T>(fs, JsonOptions);
        }
        else
        {
            return new();
        }
    }

    public static T Read<T>(string path) where T : class
    {
        if (File.Exists(path))
        {
            var fs = File.OpenRead(path);
            return JsonSerializer.Deserialize<T>(fs, JsonOptions);
        }
        else
        {
            return null;
        }
    }

    public static void Save(string path, object value)
    {
        var text = JsonSerializer.Serialize(value, JsonOptions);
        File.WriteAllText(path, text);
    }

}
