Work in progress
# ReadMe #

This example code is part of a blog post on the codit blog about improving BizTalk performance by bypassing the BizTalk MessageBox with the help of Redis cache and a WCF IOperationInvoker.


## RedisCacheClient Usage ##

The RedisCacheClient has the following NuGet Dependencies:

- StackExchange.Redis.StrongName

- Newtonsoft.Json

To use the RedisCacheClient you'll have to update the connectionString fields (these are here only for demo-purposes), we recommend to abstract these away from the client's implementation.  

*Write-Operations* will be done to the "master", *Read-Operations* will be done to the "slave". We trust Redis to do the replication of this data on multi-instance environments. 

More information about configuring Redis for master/slave configuration & replication can be found [here](http://redis.io/topics/replication "Official Redis documentation"). 

To write a value, create an instance of the RedisCacheClient, and as parameters pass a typed cache key, a value and a optional time-to-live. 


    var client = new RedisCacheClient();
    var success = client.Write(new YourCacheKey("Codit"), true, TimeSpan.FromMinutes(15));

To read a value with the client, you only need to have a cacheKey

    var client = new RedisCacheClient();
    var cachedValue = _cacheClient.Read<bool>(new YourCacheKey("Codit"));

The return value implements the "MayBe principle", therefor you can check in advance if there was an acutal value found. 

    if (cachedValue.IsPresent)
    {
       // do something with cachedValue.Value
    }


### CacheKeys ###
Redis a a key-value store, to ease programming against it and to avoid typos. We introduced typed cache keys this allows us to re-use cache keys without having to worry about keeping strings up-to-date. 

An example of a cachekey


	public class TestCacheKey : ICacheKey
	{
		public TestCacheKey(string parameter)
        {
            _name = string.Format("TestCacheKey:{0}", parameter);
        }

        private readonly string _name; 
        public string Name
        {
            get
            {
                return _name;
            }
        }
	}


This is how the values are stored in Redis:
![](\images\RedisDesktopManager.png)


## Cache operation Invoker Usage ##

todo

# Links #
todo