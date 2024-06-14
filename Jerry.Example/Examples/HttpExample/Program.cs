using Jerry.Utilities.HttpUtility;

namespace Jerry.Example.Examples.HttpExample;

public static class HttpExample
{
    public static async Task Main(string[] args)
    {
        var listener = new MainListener();
        listener.HandlerGroup = new Handlers();
        listener.AddPrefix("http://localhost:4000");
        await listener.StartListenAsync();
    }
}

