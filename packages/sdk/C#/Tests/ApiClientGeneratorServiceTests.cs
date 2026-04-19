using FluentAssertions;
using Generator.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests;

public class ApiClientGeneratorServiceTests
{
    private readonly Mock<ILogger<ApiClientGeneratorService>> _loggerMock;
    private readonly IApiClientGeneratorService _service;
    private readonly string _testDataPath;

    public ApiClientGeneratorServiceTests()
    {
        _loggerMock = new Mock<ILogger<ApiClientGeneratorService>>();
        _service = new ApiClientGeneratorService(_loggerMock.Object);

        _testDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");

        Directory.CreateDirectory(_testDataPath);

        string sourceFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sample-api.json");
        string destFile = Path.Combine(_testDataPath, "sample-api.json");

        if (File.Exists(sourceFile) && !File.Exists(destFile))
        {
            File.Copy(sourceFile, destFile);
        }
    }

    [Fact]
    public async Task GenerateClientAsync_WithValidInput_ShouldReturnTrue()
    {
        // Arrange
        string inputFile = Path.Combine(_testDataPath, "sample-api.json");
        string outputDir = Path.Combine(_testDataPath, "Output");
        const string namespaceName = "TestNamespace";

        // Cleanup before test
        if (Directory.Exists(outputDir))
        {
            Directory.Delete(outputDir, true);
        }

        // Act
        bool result = await _service.GenerateClientAsync(inputFile, outputDir, namespaceName);

        // Assert
        result.Should().BeTrue();
        File.Exists(Path.Combine(outputDir, "ApiClient.cs")).Should().BeTrue();

        // Verify the content has the correct namespace
        string generatedCode = await File.ReadAllTextAsync(Path.Combine(outputDir, "ApiClient.cs"));
        generatedCode.Should().Contain($"namespace {namespaceName}");
    }

    [Fact]
    public async Task GenerateClientAsync_WithNonExistentFile_ShouldReturnFalse()
    {
        // Arrange
        string inputFile = Path.Combine(_testDataPath, "non-existent-file.json");
        string outputDir = Path.Combine(_testDataPath, "Output");
        const string namespaceName = "TestNamespace";

        // Act
        bool result = await _service.GenerateClientAsync(inputFile, outputDir, namespaceName);

        // Assert
        result.Should().BeFalse();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("does not exist")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }
}