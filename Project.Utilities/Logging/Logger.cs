using Project.Utilities.IoC.General;
using Project.Utilities.Logging.Handlers;
using Project.Utilities.Logging.Interfaces;
using Project.Utilities.Logging.LogStructs;
using Serilog;
using Serilog.Events;
using SLogger = Serilog.Core.Logger;
namespace Project.Utilities.Logging;

public class Logger : IDisposable
{
    // Need this to act as a proxy for some internal Serilog APIs related to message parsing.
    private readonly SLogger _sLogger = new LoggerConfiguration().CreateLogger();
    public List<ILogHandler> Handlers { get; }
    public string Name;
    private ReaderWriterLockSlim _handlersLock = new();
    private bool _disposed;
    public void AddHandler(ILogHandler handler)
    {
        _handlersLock.EnterWriteLock();
        try
        {
            Handlers.Add(handler);
        }
        finally
        {
            _handlersLock.ExitWriteLock();
        }
    }

    public void RemoveHandler(ILogHandler handler)
    {
        _handlersLock.EnterWriteLock();
        try
        {
            Handlers.Remove(handler);
        }
        finally
        {
            _handlersLock.ExitWriteLock();
        }
    }
    public LogLevel? Level
    {
        get => _level;
        set
        {
            if (Name == "root" && value == null)
            {
                throw new ArgumentException("Cannot set root sawmill level to null.");
            }

            _level = value;
        }
    }

    private LogLevel? _level = null;
    
    public void Log(LogLevel level, Exception? exception, string message, params object?[] args)
    {
        if (!_sLogger.BindMessageTemplate(message, args, out var parsedTemplate, out var properties))
            return;

        var msg = new LogEvent(DateTimeOffset.Now, level.ToSerilog(), exception, parsedTemplate, properties);
        LogInternal(Name, msg);
    }
    public void Log(LogLevel level, string message, params object?[] args)
    {
        if (args.Length != 0 && message.Contains("{0"))
        {
            // Fallback for logs that still use the string.Format approach.
            message = string.Format(message, args);
            args = Array.Empty<object>();
        }

        Log(level, null, message, args);
    }

    public void Log(LogLevel level, string message)
    {
        Log(level, message, Array.Empty<object>());
    }
    private void LogInternal(string sourceLogger, LogEvent message)
    {
        if (message.Level.ToUtils() < GetPracticalLevel())
        {
            return;
        }

        _handlersLock.EnterReadLock();
        try
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Logger));
            }

            foreach (var handler in Handlers)
            {
                handler.Log(sourceLogger, message);
            }
        }
        finally
        {
            _handlersLock.ExitReadLock();
        }
    }

    private LogLevel GetPracticalLevel()
    {
        if (Level.HasValue)
        {
            return Level.Value;
        }

        return default;
    }
    
    public static Logger GetLogger(string name, HandlerFlags handlers, LogLevel? level = LogLevel.Debug)
    {
        var list = handlers.ToHandlers();
        return new Logger(name, list, level);
    }

    private Logger(string name, IEnumerable<ILogHandler> handlers, LogLevel? level)
    {
        Name = name;
        Handlers = handlers.ToList();
        Level = level;
    }
    public void Debug(string message, params object?[] args)
    {
        Log(LogLevel.Debug, message, args);
    }

    public void Debug(string message)
    {
        Log(LogLevel.Debug, message);
    }

    public void Info(string message, params object?[] args)
    {
        Log(LogLevel.Info, message, args);
    }

    public void Info(string message)
    {
        Log(LogLevel.Info, message);
    }

    public void Warning(string message, params object?[] args)
    {
        Log(LogLevel.Warning, message, args);
    }

    public void Warning(string message)
    {
        Log(LogLevel.Warning, message);
    }

    public void Error(string message, params object?[] args)
    {
        Log(LogLevel.Error, message, args);
    }

    public void Error(string message)
    {
        Log(LogLevel.Error, message);
    }

    public void Fatal(string message, params object?[] args)
    {
        Log(LogLevel.Fatal, message, args);
    }

    public void Fatal(string message)
    {
        Log(LogLevel.Fatal, message);
    }

    public void Dispose()
    {
        _handlersLock.EnterWriteLock();
        try
        {
            _disposed = true;

            foreach (ILogHandler handler in Handlers)
            {
                if (handler is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
        finally
        {
            _handlersLock.ExitWriteLock();
        }
    }
}