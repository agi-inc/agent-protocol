using CommandLine;
using Generator.Common.Extensions;
using Generator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Generator;
// Command line options class

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Setup dependency injection
        IServiceProvider serviceProvider = ConfigureServices();

        return await Parser.Default.ParseArguments<Options>(args)
            .MapResult(
                async (Options opts) => await RunWithOptionsAsync(opts, serviceProvider),
                errs => Task.FromResult(1)
            );
    }

    private static IServiceProvider ConfigureServices()
    {
        ServiceCollection services = new();

        // Configure all services using the extension method
        services.AddApplicationServices();

        return services.BuildServiceProvider();
    }

    private static async Task<int> RunWithOptionsAsync(Options options, IServiceProvider serviceProvider)
    {
        // Get logger
        ILogger<Program>? logger = serviceProvider.GetService(typeof(ILogger<Program>)) as ILogger<Program>;
        logger?.LogInformation("Starting OpenAPI Client Generator");

        try
        {
            // Determine output directory
            string outputDir = options.OutputDirectory ?? System.IO.Path.Combine(
                System.IO.Directory.GetCurrentDirectory(), "Generated");

            // Get the generator service
            IApiClientGeneratorService? generatorService = serviceProvider.GetService(typeof(IApiClientGeneratorService)) as IApiClientGeneratorService;
            if (generatorService == null)
            {
                logger?.LogError("Failed to resolve ApiClientGeneratorService");
                return 1;
            }

            // Generate the client
            bool result = await generatorService.GenerateClientAsync(
                options.InputFilePath,
                outputDir,
                options.Namespace
            );

            return result ? 0 : 1;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            return 1;
        }
    }
}