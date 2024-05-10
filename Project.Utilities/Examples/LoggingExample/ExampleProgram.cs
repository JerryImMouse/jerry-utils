using Project.Utilities.Logging;
using Project.Utilities.Logging.Handlers;
using Project.Utilities.Logging.LogStructs;

namespace Project.Utilities.Examples.LoggingExample;

// lets pretend its a entry point for our program
public static class ExampleProgram
{
    private static Logger _logger = default!;
    public static void Main(string[] args)
    {
        // here "program" means this logger is working inside our program class
        // this name can be whatever you want user to see inside logs, this will be like
        // [INFO] program: Log text
        // HandlerFlags is a enum where you can specify which handler to use, you can specify multiple by
        // HandlerFlags.Console | HandlerFlags.File etc.
        // the last one specifies which loglevel is max for this logger, logs with level higher than this will be ignored
        // for example, this logger is able to log Info levels, but not Debug and Verbose
        _logger = Logger.GetLogger("program", HandlerFlags.Console, LogLevel.Info);
        
        // this log will be ignored due to max level of our logger
        _logger.Debug("Some logs");
        
        // this log won't be ignored, we can log info levels
        _logger.Info("Some info logs");
        
        // you can also specify exception in this method, but it won't be thrown and code completion will persist.
        // It'll just show you the name of the exception and its message
        // If you want to log trace, you can specify it in message with Environment.StackTrace
        _logger.Log(LogLevel.Error, new NullReferenceException(), "Some shit happened");
        
        // you can add handlers in runtime with AddHandler, there are also a RemoveHandler method.
        // FileLogHandler depends on correct filename, so make sure you have "./" in the beginning of your path
        _logger.AddHandler(new FileLogHandler("./logs.log"));
        
        // Error will be thrown
        // _logger.AddHandler(new FileLogHandler("logs.log"));
        
        // need to add a little delay, so program won't be closed and logs will be outputed correctly.
        // Make sure you're waiting before all logs out and then finish your program.
        // You DON'T need to wait in runtime, only before program closes
        // Delay in 1-2 milliseconds usually enough
        Thread.Sleep(1);
    }
}