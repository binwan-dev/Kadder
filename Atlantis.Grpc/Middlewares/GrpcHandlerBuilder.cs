using System;
using Followme.AspNet.Core.FastCommon.ThirdParty.GrpcServer.Middlewares;
using System.Collections.Generic;
using Followme.AspNet.Core.FastCommon.Components;
using Followme.AspNet.Core.FastCommon.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace Followme.AspNet.Core.FastCommon.ThirdParty.GrpcServer.Middlewares
{
    public class GrpcHandlerBuilder
    {
        private readonly IList<Func<HandlerDelegateAsync,HandlerDelegateAsync>> _delegates;
        private readonly HandlerDelegateAsync _last=
            (context)=>
            {
                context.HasDone=true;
                context.Result=new MessageResult(ResultCode.Exception,"No handler can be handle message!");
                return Task.CompletedTask;
            };
        
        public GrpcHandlerBuilder()
        {
            _delegates=new List<Func<HandlerDelegateAsync,HandlerDelegateAsync>>();
        }

        public HandlerDelegateAsync DelegateProxyAsync{get;private set;}
        
        public GrpcHandlerBuilder UseMiddleware<T>()
        {
            var type=typeof(T);
            if(!typeof(GrpcMiddlewareBase).IsAssignableFrom(type))throw new InvalidCastException($"The middle haven't implement to GrpcMiddlewareBase! type is: {type.FullName}");

            return Use((next)=>{
                var constructor=type.GetConstructor(new[]{typeof(HandlerDelegateAsync)});
                var middleware=(GrpcMiddlewareBase)constructor.Invoke(new object[]{next});
                return middleware.HandleAsync;
            });
        }

        public GrpcHandlerBuilder Use(Func<HandlerDelegateAsync,HandlerDelegateAsync> func)
        {
            _delegates.Add(func); 
            return this;
        }

        public HandlerDelegateAsync Build()
        {
            var handlerDelegate=_last;
            foreach(var delegateItem in _delegates.Reverse())
            {
                handlerDelegate= delegateItem(handlerDelegate);
            }
            DelegateProxyAsync=handlerDelegate;
            return handlerDelegate;
        }
    }
}
