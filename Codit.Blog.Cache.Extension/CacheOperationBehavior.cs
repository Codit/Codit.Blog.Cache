using Codit.Blog.Cache.Interfaces;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Codit.Blog.Cache.Extension
{
    public class CacheOperationBehavior : IOperationBehavior
    {
        public CacheOperationBehavior()
        {
           
        }

        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            // We recommend to do this by using a IoC, for demo purposes a new instance is created manually.
            var cacheClient = new RedisCacheClient();
            dispatchOperation.Invoker = new CacheOperationInvoker(dispatchOperation.Invoker, cacheClient);
        }

        public void Validate(OperationDescription operationDescription)
        {
        }
    }
}
