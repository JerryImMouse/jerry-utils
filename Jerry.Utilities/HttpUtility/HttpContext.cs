using System.Net;
using System.Text;
using System.Text.Json;
using Jerry.Utilities.Utility;
using Method = Jerry.Utilities.HttpUtility.HttpMethod;
namespace Jerry.Utilities.HttpUtility;


/// <summary>
/// Wrapper around <see cref="HttpListenerContext"/> for ease use.
/// </summary>
public class HttpContext
{
    private HttpListenerContext _nativeContext;
    private StreamReader _requestReader;
    private StreamWriter _responseWriter;
    public Dictionary<string, string> Headers;
    

    public IPEndPoint RemoteEndPoint => _nativeContext.Request.RemoteEndPoint;
    public Method HttpMethod;

    public string AbsoluteUrl => _nativeContext.Request.Url!.ToString();
    public string RelativeUrl => _nativeContext.Request.RawUrl!.ToString();
    
    #region Private
    private void ParseHttpMethod()
    {
        var method = _nativeContext.Request.HttpMethod;
        
        method = method
            .ToLower()
            .CapitalizeLetter(0);
        
        if (!Enum.TryParse(method, out Method parsedMethod))
            HttpMethod = Method.Get;
        HttpMethod = parsedMethod;
    }
    
    private Dictionary<string, string> GetHeaders()
    {
        var headers = new Dictionary<string, string>();
        var native = _nativeContext.Request.Headers;
        foreach (var key in native.Keys)
        {
            var strKey = key.ToString();
            if (strKey == null)
                continue;
            var value = native[strKey];
            if (value == null)
                continue;
            headers.Add(strKey, value);
        }

        return headers;
    }
    #endregion
    
    
    public HttpContext(HttpListenerContext nativeContext)
    {
        _nativeContext = nativeContext;
        Headers = GetHeaders();
        ParseHttpMethod();
        _requestReader = new StreamReader(_nativeContext.Request.InputStream);
        _responseWriter = new StreamWriter(_nativeContext.Response.OutputStream);
    }
    
    public async Task<string> ReadJsonBodyAsync()
    {
        var builder = new StringBuilder();
        while (!_requestReader.EndOfStream)
        {
            builder.Append(await _requestReader.ReadLineAsync());
        }
        
        return builder.ToString();
    }
    public async Task<T> ReadJsonBodyAsync<T>()
    {
        var builder = new StringBuilder();
        while (!_requestReader.EndOfStream)
        {
            builder.Append(await _requestReader.ReadLineAsync());
        }

        var deserialized = JsonSerializer.Deserialize<T>(builder.ToString());
        return deserialized!;
    }

    public async Task RespondAsync(string data, string contentType, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _nativeContext.Response.ContentType = contentType;
        _nativeContext.Response.StatusCode = (int)statusCode;
        await _responseWriter.WriteLineAsync(data);
        await _responseWriter.FlushAsync();
        DisposeConnection();
    }

    public async Task RespondFileAsync(string path, string contentType, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        // here i was made to use output stream directly to pass byte data
        if (!File.Exists(path))
        {
            await RespondErrorAsync();
            return;
        }
        _nativeContext.Response.ContentType = contentType;
        _nativeContext.Response.StatusCode = (int)statusCode;
        var buffer = await File.ReadAllBytesAsync(path);
        var stream = _nativeContext.Response.OutputStream;
        await stream.WriteAsync(buffer);
        DisposeConnection();
    }
    public async Task RespondFileTextAsync(string text, string contentType, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        // here I was made to use output stream directly to pass byte data
        _nativeContext.Response.ContentType = contentType;
        _nativeContext.Response.StatusCode = (int)statusCode;
        var buffer = Encoding.UTF8.GetBytes(text);
        var stream = _nativeContext.Response.OutputStream;
        await stream.WriteAsync(buffer);
        DisposeConnection();
    }
    public async Task RespondHtmlAsync(string path, string contentType, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var text = await File.ReadAllTextAsync(path);
        await RespondHtmlTextAsync(text, contentType, statusCode);
    }
    public async Task RespondHtmlTextAsync(string text, string contentType, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        await RespondAsync(text, contentType, statusCode);
    }
    public async Task RespondErrorAsync(HttpStatusCode statusCode = HttpStatusCode.NotFound)
    {
        _nativeContext.Response.StatusCode = (int)statusCode;
        DisposeConnection();
    }

    private void DisposeConnection()
    {
        _requestReader.Close();
        _responseWriter.Close();
        _nativeContext.Response.Close();
    }

    public void AddHeader(string key, string value)
    {
        _nativeContext.Response.Headers.Add(key, value);
    }

    public void Close() => DisposeConnection();
}