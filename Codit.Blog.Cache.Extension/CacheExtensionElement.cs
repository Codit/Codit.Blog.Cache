using System;
using System.ServiceModel.Configuration;

namespace Codit.Blog.Cache.Extension
{
    public class CacheExtensionElement : BehaviorExtensionElement
    {
        public override Type BehaviorType
        {
            get { return typeof(CacheServiceBehavior); }
        }

        protected override object CreateBehavior()
        {
            return new CacheServiceBehavior();
        }
    }
}
