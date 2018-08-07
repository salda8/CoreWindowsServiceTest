﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using Polly;
using Serilog;

namespace Complaya
{
    public static class IServiceCollectionExtension
    {
        public static void AddServices(this IServiceCollection services)
        {
            
            services.AddTransient<SerilogHttpMessageHandler>();

            // Register a HTTP Client
            services.AddHttpClient<KxClient>().AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10)
                })).AddHttpMessageHandler<SerilogHttpMessageHandler>();

            
        }
    }


}