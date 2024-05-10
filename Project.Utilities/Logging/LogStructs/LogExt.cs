using System.Runtime.CompilerServices;
using Project.Utilities.Logging.Handlers;
using Project.Utilities.Logging.Interfaces;
using Serilog.Events;

namespace Project.Utilities.Logging.LogStructs;

public static class LogExt
{
    public const string LogNameVerbose = "VERB";
    public const string LogNameDebug = "DEBG";
    public const string LogNameInfo = "INFO";
    public const string LogNameWarning = "WARN";
    public const string LogNameError = "ERRO";
    public const string LogNameFatal = "FATL";
    public const string LogNameUnknown = "UNKO";
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LogLevel ToUtils(this LogEventLevel level)
    {
        return (LogLevel) level;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LogEventLevel ToSerilog(this LogLevel level)
    {
        return (LogEventLevel) level;
    }
    public static List<ILogHandler> ToHandlers(this HandlerFlags flags)
    {
        var handlerInstances = new List<ILogHandler>();
        if (flags.HasFlag(HandlerFlags.Console))
        {
            handlerInstances.Add(ConsoleLogHandler.Instance);
        }

        if (flags.HasFlag(HandlerFlags.File))
        {
            handlerInstances.Add(new FileLogHandler("./logs.log"));
        }
        //TODO: loki handler for advanced logging
        
        return handlerInstances;
    }
    public static string LogLevelToName(LogLevel level)
    {
        return level switch
        {
            LogLevel.Verbose => LogNameVerbose,
            LogLevel.Debug => LogNameDebug,
            LogLevel.Info => LogNameInfo,
            LogLevel.Warning => LogNameWarning,
            LogLevel.Error => LogNameError,
            LogLevel.Fatal => LogNameFatal,
            _ => LogNameUnknown
        };
    }
}