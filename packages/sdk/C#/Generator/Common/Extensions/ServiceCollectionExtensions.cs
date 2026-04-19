using Generator.Common.Configs;
using Generator.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Generator.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.ConfigureLogging();

        services.AddScoped<IApiClientGeneratorService, ApiClientGeneratorService>();

        return services;
    }
}