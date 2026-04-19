namespace Generator.Services;

public interface IApiClientGeneratorService
{
    Task<bool> GenerateClientAsync(string inputFilePath, string outputDirectory, string namespaceName);
}