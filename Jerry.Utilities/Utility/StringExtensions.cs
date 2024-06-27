namespace Jerry.Utilities.Utility;

public static class StringExtensions
{
    public static string CapitalizeLetter(this string str, int letterIndex)
    {
        if (str.Length == 0)
            throw new NullReferenceException("String is null");

        ArgumentOutOfRangeException.ThrowIfGreaterThan(letterIndex, str.Length - 1);

        var array = str.ToCharArray();
        array[letterIndex] = char.ToUpper(array[letterIndex]);
        return new string(array);
    }
}