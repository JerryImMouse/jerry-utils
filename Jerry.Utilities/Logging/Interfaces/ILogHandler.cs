using Serilog.Events;

namespace Jerry.Utilities.Logging.Interfaces;

public interface ILogHandler
{
    public void Log(string objectName, LogEvent message);
}