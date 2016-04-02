using System;
using System.Collections.Generic;

namespace Codit.Blog.Cache.Interfaces
{
    public interface ICacheClient
    {
        /// <summary>
        /// Reads the value from the cache for a specific key
        /// </summary>
        /// <typeparam name="TExpected">Expected type of the value</typeparam>
        /// <param name="keyName">Name of the key</param>
        /// <returns>Value for the key</returns>
        Maybe<TExpected> Read<TExpected>(ICacheKey key);

        /// <summary>
        /// Writes a value to a specific key
        /// </summary>
        /// <typeparam name="TValue">Type of the value</typeparam>
        /// <param name="keyName">Name of the key</param>
        /// <param name="value">New value of the key</param>
        /// <param name="expiration">Duration that the key will live in the cache</param>
        /// <returns>Indication whether or not the operation succeeded</returns>
        bool Write<TValue>(ICacheKey key, TValue value, TimeSpan expiration);

        /// <summary>
        /// Removes a key from the cache
        /// </summary>
        /// <param name="keyName">Name of the key</param>
        /// <returns>Indication whether or not the operation succeeded</returns>
        bool Remove(ICacheKey key);

        /// <summary>
        /// Removes a list of keys from the cache
        /// </summary>
        /// <param name="keys">List of keys</param>
        /// <returns>The number of keys that were removed</returns>
        long Remove(List<ICacheKey> keys);
    }
}