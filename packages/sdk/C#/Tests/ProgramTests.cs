using FluentAssertions;
using Generator;
using Xunit;

namespace Tests;

public class ProgramTests
{
    [Fact]
    public async Task Main_WithValidArguments_ShouldReturnZero()
    {
        // Arrange
        string testDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");
        Directory.CreateDirectory(testDataPath);

        string inputFile = Path.Combine(testDataPath, "sample-api.json");
        string outputDir = Path.Combine(testDataPath, "Output");

        // Act
        int result = await Program.Main(["-i", inputFile, "-o", outputDir, "-n", "TestNamespace"]);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task Main_WithInvalidArguments_ShouldReturnNonZero()
    {
        // Arrange - No arguments provided

        // Act
        int result = await Program.Main([]);

        // Assert
        result.Should().NotBe(0);
    }

    [Fact]
    public async Task Main_WithNonExistentInputFile_ShouldReturnNonZero()
    {
        // Arrange
        string testDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");
        string inputFile = Path.Combine(testDataPath, "non-existent-file.json");
        string outputDir = Path.Combine(testDataPath, "Output");

        // Act
        int result = await Program.Main(["-i", inputFile, "-o", outputDir]);

        // Assert
        result.Should().NotBe(0);
    }
}