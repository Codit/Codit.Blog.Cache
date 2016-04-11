# ReadMe #

This example code is part of a blog post on the codit blog about improving BizTalk performance by bypassing the BizTalk MessageBox with the help of Redis cache and a WCF IOperationInvoker.


## RedisCacheClient ##

The RedisCacheClient has the following NuGet Dependencies:

- *StackExchange.Redis.StrongName*

- *Newtonsoft.Json*

To use the RedisCacheClient you'll have to update the connectionString fields (these are here only for demo-purposes), we recommend to abstract these away from the client's implementation.  

*Write-Operations* will be done to the "master", *Read-Operations* will be done to the "slave". We trust Redis to do the replication of this data on multi-instance environments. 

More information about configuring Redis for master/slave configuration & replication can be found [here](http://redis.io/topics/replication "Official Redis documentation"). 

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

![](https://github.com/msjonathan/Codit.Blog.Cache/blob/master/Images/RedisDesktopManager.PNG?raw=true)

### Client ###

To write a value, create an instance of the RedisCacheClient, and as parameters pass a typed cache key, a value and a optional time-to-live. 


    var client = new RedisCacheClient();
    var success = client.Write(new YourCacheKey("Codit"), true, TimeSpan.FromMinutes(15));

To read a value with the client, you only need to have a cacheKey

    var client = new RedisCacheClient();
    var cachedValue = _cacheClient.Read<bool>(new YourCacheKey("Codit"));

The return value implements the "MayBe principle", therefor you can check in advance if there was an acutal value found. This reads easier than having null check's. 

    if (cachedValue.IsPresent)
    {
       // do something with cachedValue.Value
    }




## Cache operation Invoker ##

The IOperationInvoker is the last step before calling the actual implementation of your service, here you have the power of skipping the implementation and returning a custom message.  

This method can be implemented 

     public Task<object> InvokeOperationAsync(object instance, object[] inputs, object state)

The example code excludes retrieving the operationId and creation of the return message. If you want to know more about As4, the Codit blog has a serie [As4 For Dummies](http://www.codit.eu/blog/2016/02/01/as4-for-dummies-part-i-introduction/)
 
Steps to be taken to host this behavior in BizTalk: 

- GAC the signed dll. 
- Add the WCF behavior to the machine.config 
- Add the behvior to your WCF receive port. 



# Links #

- [https://github.com/uglide/RedisDesktopManager](https://github.com/uglide/RedisDesktopManager)
- [https://github.com/StackExchange/StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis)
- [https://github.com/JamesNK/Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)