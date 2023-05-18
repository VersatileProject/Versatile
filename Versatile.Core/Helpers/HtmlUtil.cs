namespace Versatile.Core.Helpers;

public static class HtmlUtil
{
    public static async Task<string> GetSourceCode(string url)
    {
        using var client = new HttpClient();
        using var response = await client.GetAsync(url);
        using var content = response.Content;
        var sourcecode = await content.ReadAsStringAsync();
        return sourcecode;
    }
}
