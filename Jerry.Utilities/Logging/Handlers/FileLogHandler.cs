#region --- License ---

// MIT License
//
// Copyright (c) 2017-2024 Space Wizards Federation
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
//     copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion --- License ---

using System.Text;
using Jerry.Utilities.IoC.General;
using Jerry.Utilities.Logging.Interfaces;
using Jerry.Utilities.Logging.LogStructs;
using Serilog.Events;

namespace Jerry.Utilities.Logging.Handlers;
public class FileLogHandler : ILogHandler, IDisposable
{
    private readonly TextWriter _writer;
    private static FileLogHandler? _instance;

    public static FileLogHandler Instance
    {
        get
        {
            if (_instance != null) 
                return _instance;
                
            _instance = new FileLogHandler("./logs.log");
            return _instance;

        }
        set => _instance = value;
    }
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