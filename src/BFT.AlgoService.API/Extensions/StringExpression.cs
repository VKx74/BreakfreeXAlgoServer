namespace BFT.AlgoService.API.Extensions
{
    public static class StringExpression
    {
        public static string RemoveLast(this string str)
        {
            return string.IsNullOrEmpty(str) ? str : str.Remove(str.Length - 1, 1);
        }

        public static string RemoveFirst(this string str)
        {
            return string.IsNullOrEmpty(str) ? str : str.Remove(0, 1);
        }
    }
}
