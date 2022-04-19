using System;
using System.Runtime.InteropServices;

namespace ROS2
{
    /// <summary>
    /// Base class of a Service without generic type arguments for use in collections or so.
    /// </summary>
    public abstract class Service
    {
        // Only allow internal subclasses.
        internal Service()
        {
        }

        // Service does intentionaly (for now) not implement IDisposable as this
        // needs some extra consideration how the type works after its
        // internal handle is disposed.
        // By relying on the GC/Finalizer of SafeHandle the handle only gets
        // Disposed if the service is not live anymore.
        abstract internal SafeServiceHandle Handle { get; }

        abstract internal IRosMessage CreateRequest();
        
        abstract internal SafeHandle CreateRequestHandle();

        abstract internal IRosMessage CreateResponse();

        abstract internal SafeHandle CreateResponseHandle();

        abstract internal void TriggerCallback(IRosMessage request, IRosMessage response);
    }

    public sealed class Service<TService, TRequest, TResponse> : Service
        where TService : IRosServiceDefinition<TRequest, TResponse>
        where TRequest : IRosMessage, new()
        where TResponse : IRosMessage, new()
    {
        private Action<TRequest, TResponse> _callback;

        internal Service(SafeServiceHandle handle, Action<TRequest, TResponse> callback)
        {
            Handle = handle;
            _callback = callback;
        }

        internal override SafeServiceHandle Handle { get; } 

        internal override IRosMessage CreateRequest() => (IRosMessage)new TRequest();

        internal override SafeHandle CreateRequestHandle() => MessageStaticMemberCache<TRequest>.CreateMessageHandle();

        internal override IRosMessage CreateResponse() => (IRosMessage)new TResponse();

        internal override SafeHandle CreateResponseHandle() => MessageStaticMemberCache<TResponse>.CreateMessageHandle();

        internal override void TriggerCallback(IRosMessage request, IRosMessage response)
        {
            _callback((TRequest)request, (TResponse)response);
        }
    }
}
