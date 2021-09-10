using System;
using Kadder.AspNetCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class KadderServiceExtension
    {
        public static IServiceCollection UseKadder(this IServiceCollection services, Action<KadderBuilder> builderAction)
        {
            var builder = new KadderBuilder();
            builder.Services = services;
            builderAction(builder);

            services.AddSingleton(builder);

            return services;
        }
    }
}
