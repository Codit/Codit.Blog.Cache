using Codit.Blog.Cache.Converters;
using Codit.Blog.Cache.Exceptions;
using Codit.Blog.Cache.Interfaces;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Codit.Blog.Cache
{
    public class RedisCacheClient : ICacheClient
    {
        private readonly string _masterConnectionString = "FillInYourMasterConnectionString";
        private readonly string _slaveConnectionString = "FillInYourSlaveConnectionString";

        /// <summary>
        /// Connection to the cache
        /// </summary>
        public Lazy<IConnectionMultiplexer> Connection { get; set; }

        /// <summary>
        /// Read-Only Connection to the closest node
        /// </summary>
        public Lazy<IConnectionMultiplexer> ReadOnlyConnection { get; set; }

        public RedisCacheClient()
        {
            Connection = new Lazy<IConnectionMultiplexer>(InitializeConnection);
            ReadOnlyConnection = new Lazy<IConnectionMultiplexer>(InitializeReadOnlyConnection);
        }

        /// <summary>
        /// Reads the value from the cache for a specific key
        /// </summary>
        /// <typeparam name="TExpected">Expected type of the value</typeparam>
        /// <param name="keyName">Name of the key</param>
        /// <returns>Value for the key</returns>
        public Maybe<TExpected> Read<TExpected>(ICacheKey key)
        {
            try
            {
                // Retrieve the default db
                IDatabase defaultDb = GetDefaultRedisDb(readOnly: true);

                // Read the value
                RedisValue redisValue = defaultDb.StringGet(key.Name);
                if (redisValue.IsNull)
                {
                    return new Maybe<TExpected>();
                }

                // Parse to the expected type
                TExpected value = RedisConverter.ConvertTo<TExpected>(redisValue);

                return new Maybe<TExpected>(value);
            }
            catch (Exception ex)
            {
                string exceptionMessage = string.Format("Something went wrong while reading the key '{0}' from the cache.", key.Name);
                var cacheException = new CacheException(exceptionMessage, ex);
                
                return new Maybe<TExpected>();
            }
        }

        /// <summary>
        /// Removes a key from the cache
        /// </summary>
        /// <param name="keyName">Name of the key</param>
        /// <returns>Indication whether or not the operation succeeded</returns>
        public bool Remove(ICacheKey key)
        {
            try
            {
                // Remove using the existing functionality
                long operationResult = RemoveKeys(new List<ICacheKey>() { key });
                return (operationResult == 1);
            }
            catch (Exception ex)
            {
                string exceptionMessage = string.Format("Something went wrong while deleting the key '{0}' from the cache.", key.Name);
                var cacheException = new CacheException(exceptionMessage, ex);
                
                return false;
            }
        }

        /// <summary>
        /// Removes the specified keys. 
        /// </summary>
        /// <param name="keys">list of keys</param>
        /// <returns>The number of keys that were removed</returns>
        public long Remove(System.Collections.Generic.List<ICacheKey> keys)
        {         
            try
            {
                return RemoveKeys(keys);
            }
            catch (Exception ex)
            {
                string exceptionMessage = "Something went wrong while deleting the list of keys from the cache.";
                var cacheException = new CacheException(exceptionMessage, ex);
 
                return 0;
            }
        }

        /// <summary>
        /// Writes a value to a specific key
        /// </summary>
        /// <typeparam name="TValue">Type of the value</typeparam>
        /// <param name="keyName">Name of the key</param>
        /// <param name="value">New value of the key</param>
        /// <param name="expiration">Duration that the key will live in the cache</param>
        /// <returns>Indication whether or not the operation succeeded</returns>
        public bool Write<TValue>(ICacheKey key, TValue value, TimeSpan expiration)
        { 
            try
            {
                // Retrieve the default db
                IDatabase defaultDb = GetDefaultRedisDb(readOnly: false);

                // Convert the value to native redis value
                RedisValue nativeValue = RedisConverter.ConvertFrom<TValue>(value);

                // Write the value to the key
                bool operationResult = defaultDb.StringSet(key.Name, nativeValue, expiration, When.Always);

                return operationResult;
            }
            catch (Exception ex)
            {
                string exceptionMessage = string.Format("Something went wrong while writing the key '{0}' to the cache.", key.Name);
                var cacheException = new CacheException(exceptionMessage, ex);
                
                return false;
            }
        }

        private long RemoveKeys(List<ICacheKey> keys)
        {
            // Early exit
            long operationResult = 0;
           
            // Retrieve the default db
            IDatabase defaultDb = GetDefaultRedisDb(readOnly: false);

            // A key is ignored if it does not exist.
            // The number of keys that were removed
            var keyArray = keys.Select(c => (RedisKey)c.Name).ToArray();

            operationResult = defaultDb.KeyDelete(keyArray);
            
            return operationResult;

        }

        private ConnectionMultiplexer InitializeConnection()
        {
            return InitializeConnection(_masterConnectionString);
        }

        private ConnectionMultiplexer InitializeReadOnlyConnection()
        {
            return InitializeConnection(_slaveConnectionString);
        }

        private ConnectionMultiplexer InitializeConnection(string connectionString)
        {
            return ConnectToRedisCache(connectionString);
        }

        private ConnectionMultiplexer ConnectToRedisCache(string configurationString)
        {
            try
            {
                // Assign options for the connection
                var configOptions = ConfigurationOptions.Parse(configurationString);
                configOptions.ClientName = "Codit Cache Client";

                // Connect to the cache
                ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(configOptions);
                
                return connection;
            }
            catch (Exception ex)
            {
                string exceptionMessage = string.Format("Failed to connect to the Redis cache with configuration string '{0}'.", configurationString);
                var cacheConnectionEx = new CacheException(exceptionMessage, ex);

                throw cacheConnectionEx;
            }
        }
        
        public bool Write<TValue>(ICacheKey key, TValue value)
        {
            return Write<TValue>(key, value, TimeSpan.MaxValue);
        }

        private IDatabase GetDefaultRedisDb(bool readOnly)
        {
            return (readOnly) ? ReadOnlyConnection.Value.GetDatabase() : Connection.Value.GetDatabase();
        }

    }
}