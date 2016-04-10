using Codit.Blog.Cache.CacheKey;
using Codit.Blog.Cache.Interfaces;
using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Threading;
using System.Threading.Tasks;

namespace Codit.Blog.Cache.Extension
{
    public class CacheOperationInvoker : IOperationInvoker
    {
        private static object[] EmptyObjectArray = new object[0];
        private readonly IOperationInvoker _innerInvoker;
        private readonly ICacheClient _cacheClient;

        public CacheOperationInvoker(IOperationInvoker innerInvoker, ICacheClient cacheClient)
        {
            _innerInvoker = innerInvoker;
            _cacheClient = cacheClient;
        }

        public bool IsSynchronous
        {
            get { return _innerInvoker.IsSynchronous; }
        }

        public object[] AllocateInputs()
        {
            return _innerInvoker.AllocateInputs();
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            outputs = null;
            object returnValue = null;

            if (inputs != null && inputs.Length == 1)
            {
                returnValue = InvokeOperationAsync(instance, inputs).Result;
            }
            else
            {
                returnValue = _innerInvoker.Invoke(instance, inputs, out outputs);
            }

            return returnValue;
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            var taskCompletionSource = new TaskCompletionSource<object>(state);

            Task<object> task = InvokeOperationAsync(instance, inputs, state);

            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    taskCompletionSource.TrySetException(t.Exception.InnerException);
                }
                else if (t.IsCanceled)
                {
                    taskCompletionSource.TrySetCanceled();
                }
                else
                {
                    taskCompletionSource.TrySetResult(t.Result);
                }
            }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default)
                .ContinueWith(x => { if (callback != null) { callback(taskCompletionSource.Task); } }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);

            return taskCompletionSource.Task;
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            var task = (Task<object>)result;

            // We don't need the outputs in BizTalk, so just set it fixed to an empty object array
            outputs = EmptyObjectArray;

            // Accessing the Result property might throw an exception if the original Task is cancelled or faulted
            try
            {
                return task.Result;
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        public Task<object> InvokeOperationAsync(object instance, object[] inputs)
        {
            return InvokeOperationAsync(instance, inputs, instance);
        }

        public Task<object> InvokeOperationAsync(object instance, object[] inputs, object state)
        {
            Message inputMessage = null;
            Message outputMessage = null;

            try
            {
                if (inputs != null && inputs.Length == 1)
                {
                    inputMessage = inputs[0] as Message;
                }

                if (inputMessage != null)
                {
                    // We need to copy the message in order to read it. 
                    // This means the original message will have the 'Read' state, which means it can no longer be read.
                    var bufferedMessage = inputMessage.CreateBufferedCopy(Int32.MaxValue);

                    inputs[0] = bufferedMessage.CreateMessage();
                    var messageToWorkOn = bufferedMessage.CreateMessage();

                    Stream messageStream = messageToWorkOn.GetBody<Stream>();
                    if (messageStream != null)
                    {
                        outputMessage = GetOutputMessageForInputMessageStream(messageStream);
                    }
                }
            }
            catch (Exception ex)
            {
                // Swallow all the exceptions
                outputMessage = null;
            }

            if (outputMessage == null)
            {
                var capturedOperationContext = OperationContext.Current;
                return Task<object>.Factory.StartNew(() =>
                {
                    OperationContext.Current = capturedOperationContext;
                    var begin = _innerInvoker.InvokeBegin(instance, inputs, null, state);
                    object[] o;

                    return _innerInvoker.InvokeEnd(instance, out o, begin);
                });
            }

            return Task.FromResult((object)outputMessage);
        }

        private Message GetOutputMessageForInputMessageStream(Stream messageStream)
        {
            Message outputMessage = null;

            var organisationId = GetOrganisationId(messageStream);

            var cachedValue = _cacheClient.Read<bool>(new NoMessagesAvailableCacheKey(organisationId));

            if (cachedValue.IsPresent && cachedValue.Value)
            {
                // Create As4 Warning Message (EmptyMessagePartitionChannel) - for demo purposes this is removed.
                var as4MessageStream = new MemoryStream();
            }

            return outputMessage;
        }

        private string GetOrganisationId(Stream messageStream)
        {
            // Here the functionality of reading the stream and retrieving the organisationId should be implemented.
            return "OrganisationId";
        }
    }
}
         
       

      

      
