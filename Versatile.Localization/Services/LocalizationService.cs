using System.Collections.Generic;
using System.Reflection;
using Windows.ApplicationModel.Resources;

namespace Versatile.Localization.Services;

public class LocalizationService
{
    private Dictionary<string, ResourceLoader> ResourceLoaders { get; }

    private string AssemblyName { get; }

    public LocalizationService()
    {
        AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        ResourceLoaders = new();
    }

    public string GetString(string resourceName)
    {
        var parts = resourceName.Split('/');
        if (parts.Length != 2)
        {
            return null;
        }

        var path = parts[0];
        var key = parts[1];
        if (!ResourceLoaders.TryGetValue(path, out var resourceLoader))
        {
            resourceLoader = ResourceLoader.GetForViewIndependentUse(AssemblyName + "/" + path);
            ResourceLoaders.Add(path, resourceLoader);
        }
        if (resourceLoader != null)
        {
            return resourceLoader.GetString(key);
        }
        else
        {
            return null;
        }
    }

    public string GetString(string resourceName, params object[] args)
    {
        var str = GetString(resourceName);
        return string.Format(str, args);
    }
}
