using MediaWeb.Infrastructure.Factories;
using MediaWeb.Infrastructure.Model;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

[assembly: FunctionsStartup(typeof(MediaApi.Startup))]
namespace MediaApi
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {                                    
            builder.Services.AddSingleton<IMediaServiceFactory, MediaServiceFactory>();
            builder.Services.AddSingleton(new ConfigWrapper());
        }
    }
}
