using Followme.AspNet.Core.FastCommon.Infrastructure;
using System.Threading.Tasks;

namespace Followme.AspNet.Core.FastCommon.ThirdParty.GrpcServer.Middlewares
{
    public delegate Task HandlerDelegateAsync(GrpcContext cotext);

}
