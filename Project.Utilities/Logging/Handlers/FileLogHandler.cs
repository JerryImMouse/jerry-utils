using System.Text;
using Project.Utilities.Logging.Interfaces;
using Project.Utilities.Logging.LogStructs;
using Serilog.Events;

namespace Project.Utilities.Logging.Handlers;

public class FileLogHandler : ILogHandler, IDisposable
{
    private readonly TextWriter _writer;

    public FileLogHandler(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        _writer = TextWriter.Synchronized(
            new StreamWriter(
                new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read | FileShare.Delete),
                new UTF8Encoding()));
    }
    public void Dispose()
    {
        _writer.Dispose();
    }

    public void Log(string objectName, LogEvent message)
    {
        var name = LogExt.LogLevelToName(message.Level.ToUtils());
        _writer.WriteLine("{0:o} [{1}] {2}: {3}", DateTime.Now, name, objectName, message.RenderMessage());

        if (message.Exception != null)
        {
            _writer.WriteLine(message.Exception.ToString());
        }

        _writer.Flush();
    }
}