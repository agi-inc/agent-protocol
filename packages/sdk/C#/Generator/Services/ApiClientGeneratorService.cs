using Microsoft.Extensions.Logging;
using NSwag;
using NSwag.CodeGeneration.CSharp;

namespace Generator.Services;

public class ApiClientGeneratorService(ILogger<ApiClientGeneratorService> logger) : IApiClientGeneratorService
{
    private readonly ILogger<ApiClientGeneratorService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<bool> GenerateClientAsync(string inputFilePath, string outputDirectory, string namespaceName)
    {
        try
        {
            _logger.LogInformation("Loading OpenAPI specification from: {FilePath}", inputFilePath);

            if (!File.Exists(inputFilePath))
            {
                _logger.LogError("Input file '{FilePath}' does not exist", inputFilePath);
                return false;
            }

            Directory.CreateDirectory(outputDirectory);

            _logger.LogDebug("Parsing OpenAPI document");
            OpenApiDocument? document = await OpenApiDocument.FromFileAsync(inputFilePath);

            CSharpClientGeneratorSettings settings = new()
            {
                ClassName = "ApiClient",
                CSharpGeneratorSettings =
                {
                    Namespace = namespaceName,
                    GenerateNullableReferenceTypes = true
                },
                GenerateDtoTypes = true,
                GenerateClientInterfaces = true,
                GenerateOptionalParameters = true,
                UseBaseUrl = false
            };

            _logger.LogInformation("Generating C# client code with namespace: {Namespace}", namespaceName);
            CSharpClientGenerator generator = new(document, settings);
            string? code = generator.GenerateFile();

            string outputFile = Path.Combine(outputDirectory, "ApiClient.cs");
            await File.WriteAllTextAsync(outputFile, code);

            _logger.LogInformation("Client successfully generated at: {OutputPath}", outputFile);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating client: {ErrorMessage}", ex.Message);
            return false;
        }
    }
}