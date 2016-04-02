using System;
using Codit.Blog.Cache.Interfaces;

namespace Codit.Blog.Cache.CacheKey
{
    public class NoMessagesAvailableCacheKey : ICacheKey
    {
        public NoMessagesAvailableCacheKey(string organisationId)
        {
            _name = string.Format("NoMessagesAvailableCacheKey:{0}", organisationId);
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
}