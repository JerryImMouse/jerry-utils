namespace Jerry.Utilities.Utility;

public static class StringExtensions
{
    public static string CapitalizeFirstLetter(this string str)
    {
        if (str.Length == 0)
            throw new NullReferenceException("String is null");

        var array = str.ToCharArray();
        array[0] = char.ToUpper(array[0]);
        return new string(array);
    }
}