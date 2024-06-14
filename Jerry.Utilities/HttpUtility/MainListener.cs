using System.Net;
using Jerry.Utilities.Logging;
using Jerry.Utilities.Logging.LogStructs;

namespace Jerry.Utilities.HttpUtility;

/// <summary>
/// Wrapper around <see cref="HttpListener"/> for ease use, provides simple interface to "talk" with some program using HTTP or creating an api
/// </summary>
public class MainListener
{
    private HttpListener _nativeListener = new();
    public IHandlerGroup? HandlerGroup = null;
    private List<Func<HttpContext, Task<bool>>> _handlers = new();
    public Logger Logger = Logger.GetLogger("httpListener", HandlerFlags.Console, LogLevel.Info);

    public async Task StartListenAsync()
    {
        HandlerGroup?.Initialize(this);
        _nativeListener.Start();
        Logger.Info("Started listening for incoming connections...");
        while (_nativeListener.IsListening)
        {
            var rawCtx = await _nativeListener.GetContextAsync();
            var ctx = new HttpContext(rawCtx);
            Logger.Info($"Processing incoming connections from {ctx.RemoteEndPoint.Address}:{ctx.RemoteEndPoint.Port} to {ctx.RelativeUrl}");
            
            // we are not waiting for task finishing
            Task.Run(() => ProcessContextAsync(ctx));
        }
    }

    private async Task ProcessContextAsync(HttpContext ctx)
    {
        foreach (var handler in _handlers)
        {
            if (await handler(ctx))
                return;
        }
        // implicit 404
        await ctx.RespondErrorAsync();
    }

    public void AddPrefix(string prefix)
    {
        _nativeListener.Prefixes.Add(prefix);
    }

    public void RegisterHandler(Func<HttpContext, Task<bool>> handler)
    {
        _handlers.Add(handler);
    }
}