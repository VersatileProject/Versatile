using Versatile.Common;

namespace Versatile.Navigation;

internal class PageDefinition
{
    public PageKey Key;
    public Type PageType;
    public Type ViewModelType;
    public bool IsOpened;

    public PageDefinition(PageKey key, Type pageType, Type viewModelType)
    {
        Key = key;
        PageType = pageType;
        ViewModelType = viewModelType;
    }
}
