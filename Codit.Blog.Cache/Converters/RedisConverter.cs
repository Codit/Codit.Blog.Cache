using Newtonsoft.Json;
using StackExchange.Redis;
using System.ComponentModel;
using System.Text;

namespace Codit.Blog.Cache.Converters
{
    public static class RedisConverter
    {
        /// <summary>
        /// Converts a native RedisValue to the specified type
        /// </summary>
        /// <typeparam name="TExpected">Type you want to convert to</typeparam>
        /// <param name="nativeValue">The native RedisValue</param>
        /// <returns>Converted value</returns>
        public static TExpected ConvertTo<TExpected>(RedisValue nativeValue)
        {
            string rawValue = (string)nativeValue;

            if (typeof(TExpected) == typeof(long)
            || typeof(TExpected) == typeof(int)
            || typeof(TExpected) == typeof(double)
            || typeof(TExpected) == typeof(string))
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(TExpected));
                return (TExpected)converter.ConvertFromString(nativeValue.ToString());
            }
            else if (typeof(TExpected) == typeof(byte[]))
            {
                return (TExpected)(object)Encoding.UTF8.GetBytes(rawValue);
            }
            else if (typeof(TExpected) == typeof(bool))
            {
                return (TExpected)(object)(rawValue == "1");
            }
            else
            {
                return JsonConvert.DeserializeObject<TExpected>(nativeValue);
            }
        }

        /// <summary>
        /// Convert a value to the native RedisValue
        /// </summary>
        /// <typeparam name="TValue">Type of the input value</typeparam>
        /// <param name="value">Value to convert</param>
        /// <returns>Native RedisValue</returns>
        public static RedisValue ConvertFrom<TValue>(TValue value)
        {
            if (typeof(TValue) == typeof(string))
            {
                return value as string;
            }
            else if (typeof(TValue) == typeof(bool))
            {
                return bool.Parse(value.ToString());
            }
            else if (typeof(TValue) == typeof(long))
            {
                return long.Parse(value.ToString());
            }
            else if (typeof(TValue) == typeof(int))
            {
                return int.Parse(value.ToString());
            }
            else if (typeof(TValue) == typeof(double))
            {
                return double.Parse(value.ToString());
            }
            else if (typeof(TValue) == typeof(byte[]))
            {
                return value as byte[];
            }
            else
            {
                return JsonConvert.SerializeObject(value, Formatting.None, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });

            }
        }
    }
}
