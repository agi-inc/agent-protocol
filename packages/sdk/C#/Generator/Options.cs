using CommandLine;

namespace Generator;

public class Options
{
    [Option('i', "input", Required = true, HelpText = "Input OpenAPI JSON file path.")]
    public string InputFilePath { get; set; } = string.Empty;

    [Option('o', "output", Required = false, HelpText = "Output directory for generated C# client.")]
    public string? OutputDirectory { get; set; }

    [Option('n', "namespace", Required = false, Default = "GeneratedApiClient", HelpText = "Namespace for the generated client.")]
    public string Namespace { get; set; } = "GeneratedApiClient";
}