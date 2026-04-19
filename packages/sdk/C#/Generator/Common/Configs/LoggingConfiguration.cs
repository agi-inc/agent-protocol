using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Generator.Common.Configs;

public static class LoggingConfiguration
{
    public static IServiceCollection ConfigureLogging(this IServiceCollection services, LogLevel minimumLevel = LogLevel.Information)
    {
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();

            builder.SetMinimumLevel(minimumLevel);
        });

        return services;
    }
}