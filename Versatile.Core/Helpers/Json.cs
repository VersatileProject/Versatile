using Newtonsoft.Json;

namespace Versatile.Core.Helpers;

public static class Json
{
    public static T ToObject<T>(string value)
    {
        return JsonConvert.DeserializeObject<T>(value);
    }

    public static string Stringify(object value)
    {
        return JsonConvert.SerializeObject(value);
    }
}
