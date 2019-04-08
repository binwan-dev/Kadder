using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pyramid.Infrastructure.Common
{
    public class RequestHandler
    {
        // private readonly IRepositoryContextManager _contextManager;
        // private readonly ILogger _logger;
        // private readonly IJsonSerializer _jsonSerialzer;
        // private readonly ICurrentRepositoryContextProvider _currentContextProvider;

        // public RequestHandler(IRepositoryContextManager contextManager, ILoggerFactory loggerFactory,
        //     IJsonSerializer jsonSerializer, ICurrentRepositoryContextProvider currentRepositoryContextProvider)
        // {
        //     _contextManager = contextManager;
        //     _logger = loggerFactory.Create<RequestHandler>();
        //     _jsonSerialzer = jsonSerializer;
        //     _currentContextProvider = currentRepositoryContextProvider;
        // }

        // public TMessageResult ExecuteCommand<TMessage, TMessageResult>(TMessage message, Func<TMessage, TMessageResult> func) where TMessageResult : MessagingResult, new()
        // {
        //     return ExecuteCommandAsync(message, func).Result;
        // }

        // public Task<TMessageResult> ExecuteCommandAsync<TMessage, TMessageResult>(TMessage message, Func<TMessage, TMessageResult> func) where TMessageResult : MessagingResult, new()
        // {
        //     return Task.Run(() =>
        //     {
        //         try
        //         {
        //             if (_logger.IsDebugEnabled) _logger.Debug($"接收到消息{message.GetType()}，请求数据:{_jsonSerialzer.Serialize(message)}");
        //             var context = _contextManager.Create();

        //             var result = func(message);
        //             context = _currentContextProvider.Current;
        //             context.Commit();
        //             context.Dispose();
        //             if (_logger.IsDebugEnabled) _logger.Debug($"消息{message.GetType()}处理成功，响应数据:{_jsonSerialzer.Serialize(result)}");
        //             return result;
        //         }
        //         catch (EnsureException ex)
        //         {
        //             _logger.Warn($"执行消息{message.GetType().Name}时业务失败，原因：{ex.Message}");
        //             return new TMessageResult() { Code = ResultCode.BussinessError, Message = ex.Message };
        //         }
        //         catch (Exception ex)
        //         {
        //             _logger.Fatal($"系统在执行{typeof(TMessage)}消息的时候出现异常！原因：{ex.GetExceptionMessage()}", ex);
        //             return new TMessageResult() { Code = ResultCode.Exception, Message = ex.Message };
        //         }
        //     });
        // }

        // public TMessageResult ExecuteQuery<TMessage, TMessageResult>(TMessage message, Func<TMessage, TMessageResult> func) where TMessageResult : MessagingResult, new()
        // {
        //     try
        //     {
        //         return func(message);
        //     }
        //     catch (EnsureException ex)
        //     {
        //         return new TMessageResult() { Code = ResultCode.BussinessError, Message = ex.Message };
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.Fatal($"系统在执行{typeof(TMessage)}消息的时候出现异常！原因：{ex.GetExceptionMessage()}", ex);
        //         return new TMessageResult() { Code = ResultCode.Exception, Message = ex.Message };
        //     }
        // }
    }
}
