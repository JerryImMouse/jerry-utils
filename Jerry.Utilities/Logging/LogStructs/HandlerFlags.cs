namespace Jerry.Utilities.Logging.LogStructs;
[Flags]
public enum HandlerFlags
{
    Console = 1 << 0,
    File = 1 << 1
}