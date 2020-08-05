namespace LinqToElk.Extensions
{
    public static class StringExtensions
    {
        public static string ToLowerFirstChar(this string input)
        {
            var newString = input;
            if (!string.IsNullOrEmpty(input))
            {
                newString = char.ToLower(newString[0]) + newString.Substring(1);
            }

            return newString;
        }
    }
}