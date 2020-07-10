using System;
using System.Text;
using Common.Logic.Helpers;

namespace Common.Cache.Extensions
{
    public static class HashCodeExtension
    {
        public static string GetBodyHashCode<TIn, TOut>(this Func<TIn, Int32, Int32, TOut> predicate, int skip = 0, int take = 0, string param = "")
        {
            return GetBodyHashCode(predicate) + $"_{param}_{skip}{take}";
        }

        public static string GetBodyHashCode<TIn, TOut>(this Func<TIn, TOut> predicate, int skip = 0, int take = 0, string param = "")
        {
            return GetBodyHashCode(predicate) + $"_{param}_{skip}{take}";
        }


        public static string GetBodyHashCode<TIn, TOut>(this Func<TIn, TOut> predicate)
        {
            return Encoding.UTF8.GetBytes(predicate.Target.ToString()).ToHex(false) + "_" +
                   predicate.Method.GetMethodBody().GetILAsByteArray().ToHex(false) + "_" +
                   Encoding.UTF8.GetBytes(JsonHelper.Serialize(predicate.Target)).ToHex(false);
        }

        public static string GetBodyHashCode<TIn, TOut>(this Func<TIn, Int32, Int32, TOut> predicate)
        {
            return Encoding.UTF8.GetBytes(predicate.Target.ToString()).ToHex(false) + "_" +
                   predicate.Method.GetMethodBody().GetILAsByteArray().ToHex(false) + "_" +
                   Encoding.UTF8.GetBytes(JsonHelper.Serialize(predicate.Target)).ToHex(false);
        }


        private static string ToHex(this byte[] bytes, bool upperCase)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);

            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));

            return result.ToString();
        }
    }
}
