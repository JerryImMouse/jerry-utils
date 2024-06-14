using Jerry.Utilities.HttpUtility;
using HttpMethod = Jerry.Utilities.HttpUtility.HttpMethod;

namespace Jerry.Example.Examples.HttpExample;

public class Handlers : IHandlerGroup
{
    public void Initialize(MainListener listener)
    {
        listener.RegisterHandler(Index);
    }

    private async Task<bool> Index(HttpContext ctx)
    {
        if (ctx.RelativeUrl != "/index/" || ctx.HttpMethod != HttpMethod.Get)
            return false;
        
        await ctx.RespondAsync("Hello, World", "text/plain");
        return true;
    }
}