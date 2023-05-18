namespace Versatile.Common.Cards;

public class Series
{
    public string Key { get; set; }
    public string Title { get; set; }
    public string ShortTitle { get; set; }
    public DateTime ReleaseDate { get; set; }
    public Product[] Products { get; set; }
}
