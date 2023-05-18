namespace Versatile.Common;


public static class VersatileApp
{
    public static string AppName = "Versatile";

    public static string StartupPath = AppDomain.CurrentDomain.BaseDirectory;
    public static string SolutionPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));

    public static string DocumentPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), AppName);

    public static bool DevMode = false;

    public static Version Version { get; set; }

    public static void Init()
    {
    }

    public static void Fin()
    {
    }

    public delegate object GetServiceDelegate(Type type);

    public static GetServiceDelegate OnGetService;

    public static T GetService<T>()
    {
        return (T)OnGetService(typeof(T));
    }

    public static Action<PageKey> NavigateTo;

    public static Func<string, string> DoLocalize;


    public static List<string> RecentFiles { get; } = new();

    public static string Localize(string resourceName)
    {
        return DoLocalize(resourceName);
    }

    public static string Localize(string resourceName, params object[] args)
    {
        var str = Localize(resourceName);
        if(str != null && args.Length > 0)
        {
            return string.Format(str, args);
        }
        else
        {
            return str;
        }
    }

    public static string Localize<T>(T value, string path) where T : struct, Enum
    {
        return DoLocalize($"{path}/{typeof(T).Name}_{value}");
    }


}
