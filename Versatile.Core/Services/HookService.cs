namespace Versatile.Core.Services;

public class HookService
{
    private Dictionary<Type, List<Delegate>> Actions { get; set; }

    public HookService()
    {
        Actions = new();
    }


    public void Register<T>(Action<T> action)
    {
        var type = typeof(T);
        if(!Actions.TryGetValue(type, out var actions))
        {
            actions = new List<Delegate>();
            Actions.Add(type, actions);
        }
        actions.Add(action);
    }

    public void Register<T>(Func<T, Task> action)
    {
        var type = typeof(T);
        if (!Actions.TryGetValue(type, out var actions))
        {
            actions = new List<Delegate>();
            Actions.Add(type, actions);
        }
        actions.Add(action);
    }

    public async Task Raise<T>(T arguments, Func<T, bool> checkcontinue = null)
    {
        var type = typeof(T);
        if (Actions.TryGetValue(type, out var actions))
        {
            foreach (var action in actions)
            {
                if(action is Func<T, Task> func)
                {
                    await func(arguments);
                }
                else
                {
                    action?.DynamicInvoke(arguments);
                }
                if(checkcontinue?.Invoke(arguments) == false)
                {
                    break;
                }
            }
        }
    }

}

public class MainWindowClosingArguments
{
    public bool Cancel { get; set; }
}

public class MainWindowClosedArguments
{
}