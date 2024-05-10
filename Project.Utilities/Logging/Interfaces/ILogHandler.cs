using Project.Utilities.Logging.LogStructs;
using Serilog.Events;

namespace Project.Utilities.Logging.Interfaces;

public interface ILogHandler
{
    public void Log(string objectName, LogEvent message);
}