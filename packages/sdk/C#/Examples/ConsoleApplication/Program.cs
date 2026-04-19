using System.Net.Http.Headers;
using AnotherAgentProtocolLibrary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Task = System.Threading.Tasks.Task;
// ReSharper disable All

namespace ConsoleApplication;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            // Set up Configuration
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            // Build service provider with DI
            ServiceProvider serviceProvider = ConfigureServices(configuration);

            // Get the required services
            serviceProvider.GetRequiredService<IApiClient>();
            ILogger<Program> logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            IAgentTaskService agentService = serviceProvider.GetRequiredService<IAgentTaskService>();

            // Main application loop
            bool exit = false;
            while (!exit)
            {
                exit = await RunMainMenuAsync(agentService, logger);
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red]Error:[/] {ex.Message}");
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
        }
    }

    private static ServiceProvider ConfigureServices(IConfiguration configuration)
    {
        ServiceCollection services = new();

        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Add HttpClient for API Client
        services.AddHttpClient<IApiClient, ApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"] ?? throw new InvalidOperationException("Base URL not configured");
            string apiKey = configuration["ApiSettings:ApiKey"] ?? throw new InvalidOperationException("API Key not configured");

            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            if (int.TryParse(configuration["ApiSettings:DefaultTimeout"], out int timeout))
            {
                client.Timeout = TimeSpan.FromSeconds(timeout);
            }
        });

        // Register the agent task service
        services.AddScoped<IAgentTaskService, AgentTaskService>();

        return services.BuildServiceProvider();
    }

    private static async Task<bool> RunMainMenuAsync(IAgentTaskService agentService, ILogger<Program> logger)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("Agent Tasks API").Centered().Color(Color.Blue));
        AnsiConsole.WriteLine();

        string choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("What would you like to do?")
                .PageSize(10)
                .AddChoices(new[]
                {
                    "Create a New Task",
                    "List All Tasks",
                    "Get Task Details",
                    "List Task Steps",
                    "Execute Task Step",
                    "Get Step Details",
                    "List Task Artifacts",
                    "Upload Artifact",
                    "Download Artifact",
                    "Exit"
                }));

        switch (choice)
        {
            case "Create a New Task":
                await CreateNewTaskAsync(agentService, logger);
                break;
            case "List All Tasks":
                await ListAllTasksAsync(agentService, logger);
                break;
            case "Get Task Details":
                await GetTaskDetailsAsync(agentService, logger);
                break;
            case "List Task Steps":
                await ListTaskStepsAsync(agentService, logger);
                break;
            case "Execute Task Step":
                await ExecuteTaskStepAsync(agentService, logger);
                break;
            case "Get Step Details":
                await GetStepDetailsAsync(agentService, logger);
                break;
            case "List Task Artifacts":
                await ListTaskArtifactsAsync(agentService, logger);
                break;
            case "Upload Artifact":
                await UploadArtifactAsync(agentService, logger);
                break;
            case "Download Artifact":
                await DownloadArtifactAsync(agentService, logger);
                break;
            case "Exit":
                return true;
        }

        // Wait for user to press a key before returning to the menu
        AnsiConsole.WriteLine();
        AnsiConsole.Markup("[dim]Press any key to continue...[/]");
        Console.ReadKey(true);
        return false;
    }

    private static async Task CreateNewTaskAsync(IAgentTaskService agentService, ILogger<Program> logger)
    {
        AnsiConsole.MarkupLine("[bold]Create a New Task[/]");
        AnsiConsole.WriteLine();

        string taskInput = AnsiConsole.Ask<string>("Enter task input text:");

        try
        {
            logger.LogInformation("Creating new task with input: {TaskInput}", taskInput);
            AgentTask task = await agentService.CreateTaskAsync(taskInput);

            // Display the result
            AnsiConsole.MarkupLine("[green]Task created successfully![/]");
            DisplayTask(task);
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to create task");
            AnsiConsole.MarkupLine($"[red]Error creating task: {ex.Message}[/]");
        }
    }

    private static async Task ListAllTasksAsync(IAgentTaskService agentService, ILogger<Program> logger)
    {
        AnsiConsole.MarkupLine("[bold]List All Tasks[/]");
        AnsiConsole.WriteLine();

        int pageSize = AnsiConsole.Ask("Page size:", 10);
        int currentPage = AnsiConsole.Ask("Page number:", 1);

        try
        {
            logger.LogInformation("Listing tasks (page {Page}, size {Size})", currentPage, pageSize);
            PaginatedData<AgentTask> pagedTasks = await agentService.GetTasksAsync(currentPage, pageSize);

            // Display the results
            Table table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Task ID")
                .AddColumn("Input")
                .AddColumn("Status")
                .AddColumn("Created At");

            foreach (AgentTask task in pagedTasks.Data)
            {
                table.AddRow(
                    task.Id,
                    task.Input ?? "N/A",
                    task.Status ?? "N/A",
                    task.CreatedAt.ToString("g")
                );
            }

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"Page {pagedTasks.CurrentPage} of {pagedTasks.TotalPages} (Total: {pagedTasks.TotalCount} tasks)");
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to list tasks");
            AnsiConsole.MarkupLine($"[red]Error listing tasks: {ex.Message}[/]");
        }
    }

    private static async Task GetTaskDetailsAsync(IAgentTaskService agentService, ILogger<Program> logger)
    {
        AnsiConsole.MarkupLine("[bold]Get Task Details[/]");
        AnsiConsole.WriteLine();

        string taskId = AnsiConsole.Ask<string>("Enter task ID:");

        try
        {
            logger.LogInformation("Getting details for task {TaskId}", taskId);
            AgentTask task = await agentService.GetTaskAsync(taskId);

            // Display the result
            DisplayTask(task);
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to get task details");
            AnsiConsole.MarkupLine($"[red]Error getting task details: {ex.Message}[/]");
        }
    }

    private static async Task ListTaskStepsAsync(IAgentTaskService agentService, ILogger<Program> logger)
    {
        AnsiConsole.MarkupLine("[bold]List Task Steps[/]");
        AnsiConsole.WriteLine();

        string taskId = AnsiConsole.Ask<string>("Enter task ID:");
        int pageSize = AnsiConsole.Ask("Page size:", 10);
        int currentPage = AnsiConsole.Ask("Page number:", 1);

        try
        {
            logger.LogInformation("Listing steps for task {TaskId}", taskId);
            PaginatedData<AgentStep> pagedSteps = await agentService.GetTaskStepsAsync(taskId, currentPage, pageSize);

            // Display the results
            Table table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Step ID")
                .AddColumn("Status")
                .AddColumn("Is Last")
                .AddColumn("Created At");

            foreach (AgentStep step in pagedSteps.Data)
            {
                table.AddRow(
                    step.Id,
                    step.Status,
                    step.IsLast ? "Yes" : "No",
                    step.CreatedAt.ToString("g")
                );
            }

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"Page {pagedSteps.CurrentPage} of {pagedSteps.TotalPages} (Total: {pagedSteps.TotalCount} steps)");
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to list task steps");
            AnsiConsole.MarkupLine($"[red]Error listing task steps: {ex.Message}[/]");
        }
    }

    private static async Task ExecuteTaskStepAsync(IAgentTaskService agentService, ILogger<Program> logger)
    {
        AnsiConsole.MarkupLine("[bold]Execute Task Step[/]");
        AnsiConsole.WriteLine();

        string taskId = AnsiConsole.Ask<string>("Enter task ID:");
        string input = AnsiConsole.Ask<string>("Enter step input:");

        try
        {
            logger.LogInformation("Executing step for task {TaskId}", taskId);
            AgentStep step = await agentService.ExecuteTaskStepAsync(taskId, input);

            // Display the result
            AnsiConsole.MarkupLine("[green]Step executed successfully![/]");
            DisplayStep(step);
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to execute task step");
            AnsiConsole.MarkupLine($"[red]Error executing task step: {ex.Message}[/]");
        }
    }

    private static async Task GetStepDetailsAsync(IAgentTaskService agentService, ILogger<Program> logger)
    {
        AnsiConsole.MarkupLine("[bold]Get Step Details[/]");
        AnsiConsole.WriteLine();

        string taskId = AnsiConsole.Ask<string>("Enter task ID:");
        string stepId = AnsiConsole.Ask<string>("Enter step ID:");

        try
        {
            logger.LogInformation("Getting details for step {StepId} in task {TaskId}", stepId, taskId);
            AgentStep step = await agentService.GetTaskStepAsync(taskId, stepId);

            // Display the result
            DisplayStep(step);
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to get step details");
            AnsiConsole.MarkupLine($"[red]Error getting step details: {ex.Message}[/]");
        }
    }

    private static async Task ListTaskArtifactsAsync(IAgentTaskService agentService, ILogger<Program> logger)
    {
        AnsiConsole.MarkupLine("[bold]List Task Artifacts[/]");
        AnsiConsole.WriteLine();

        string taskId = AnsiConsole.Ask<string>("Enter task ID:");
        int pageSize = AnsiConsole.Ask("Page size:", 10);
        int currentPage = AnsiConsole.Ask("Page number:", 1);

        try
        {
            logger.LogInformation("Listing artifacts for task {TaskId}", taskId);
            PaginatedData<AgentArtifact> pagedArtifacts = await agentService.GetTaskArtifactsAsync(taskId, currentPage, pageSize);

            // Display the results
            Table table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Artifact ID")
                .AddColumn("Filename")
                .AddColumn("Path")
                .AddColumn("Agent Created")
                .AddColumn("Created At");

            foreach (AgentArtifact artifact in pagedArtifacts.Data)
            {
                table.AddRow(
                    artifact.Id,
                    artifact.FileName,
                    artifact.RelativePath ?? "-",
                    artifact.AgentCreated ? "Yes" : "No",
                    artifact.CreatedAt.ToString("g")
                );
            }

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"Page {pagedArtifacts.CurrentPage} of {pagedArtifacts.TotalPages} (Total: {pagedArtifacts.TotalCount} artifacts)");
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to list task artifacts");
            AnsiConsole.MarkupLine($"[red]Error listing task artifacts: {ex.Message}[/]");
        }
    }

    private static async Task UploadArtifactAsync(IAgentTaskService agentService, ILogger<Program> logger)
    {
        AnsiConsole.MarkupLine("[bold]Upload Artifact[/]");
        AnsiConsole.WriteLine();

        string taskId = AnsiConsole.Ask<string>("Enter task ID:");
        string filePath = AnsiConsole.Ask<string>("Enter file path:");
        string relativePath = AnsiConsole.Ask<string>("Enter relative path in workspace:");

        if (!File.Exists(filePath))
        {
            AnsiConsole.MarkupLine("[red]File not found![/]");
            return;
        }

        try
        {
            logger.LogInformation("Uploading artifact for task {TaskId}", taskId);

            AgentArtifact artifact = await agentService.UploadArtifactAsync(taskId, filePath, relativePath);

            // Display the result
            AnsiConsole.MarkupLine("[green]Artifact uploaded successfully![/]");
            DisplayArtifact(artifact);
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to upload artifact");
            AnsiConsole.MarkupLine($"[red]Error uploading artifact: {ex.Message}[/]");
        }
    }

    private static async Task DownloadArtifactAsync(IAgentTaskService agentService, ILogger<Program> logger)
    {
        AnsiConsole.MarkupLine("[bold]Download Artifact[/]");
        AnsiConsole.WriteLine();

        string taskId = AnsiConsole.Ask<string>("Enter task ID:");
        string artifactId = AnsiConsole.Ask<string>("Enter artifact ID:");
        string outputPath = AnsiConsole.Ask<string>("Enter download location:");

        try
        {
            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");

            logger.LogInformation("Downloading artifact {ArtifactId} from task {TaskId}", artifactId, taskId);

            // Download the file
            await agentService.DownloadArtifactAsync(taskId, artifactId, outputPath);

            // Display the result
            AnsiConsole.MarkupLine("[green]Artifact downloaded successfully![/]");
            AnsiConsole.MarkupLine($"Saved to: {outputPath}");
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Failed to download artifact");
            AnsiConsole.MarkupLine($"[red]Error downloading artifact: {ex.Message}[/]");
        }
    }

    private static void DisplayTask(AgentTask task)
    {
        Panel panel = new Panel(
                Markup.Escape($"Task ID: {task.Id}\n" +
                              $"Input: {task.Input ?? "N/A"}\n" +
                              $"Status: {task.Status ?? "N/A"}\n" +
                              $"Created At: {task.CreatedAt}\n" +
                              $"Artifacts: {task.Artifacts.Count}")
            )
            .Header("Task Details")
            .Expand();

        AnsiConsole.Write(panel);

        if (task.Artifacts.Count > 0)
        {
            AnsiConsole.MarkupLine("\n[bold]Task Artifacts:[/]");
            Table artifactsTable = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Artifact ID")
                .AddColumn("Filename")
                .AddColumn("Path");

            foreach (AgentArtifact artifact in task.Artifacts)
            {
                artifactsTable.AddRow(
                    artifact.Id,
                    artifact.FileName,
                    artifact.RelativePath ?? "-"
                );
            }

            AnsiConsole.Write(artifactsTable);
        }
    }

    private static void DisplayStep(AgentStep step)
    {
        Panel panel = new Panel(
                Markup.Escape($"Step ID: {step.Id}\n" +
                              $"Task ID: {step.TaskId}\n" +
                              $"Status: {step.Status}\n" +
                              $"Is Last: {(step.IsLast ? "Yes" : "No")}\n" +
                              $"Created At: {step.CreatedAt}\n" +
                              $"Executed At: {step.ExecutedAt ?? DateTime.MinValue}\n" +
                              $"Input: {step.Input ?? "N/A"}\n" +
                              $"Output: {step.Output ?? "N/A"}")
            )
            .Header("Step Details")
            .Expand();

        AnsiConsole.Write(panel);

        if (step.Artifacts.Count > 0)
        {
            AnsiConsole.MarkupLine("\n[bold]Step Artifacts:[/]");
            Table artifactsTable = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Artifact ID")
                .AddColumn("Filename")
                .AddColumn("Path");

            foreach (AgentArtifact artifact in step.Artifacts)
            {
                artifactsTable.AddRow(
                    artifact.Id,
                    artifact.FileName,
                    artifact.RelativePath ?? "-"
                );
            }

            AnsiConsole.Write(artifactsTable);
        }
    }

    private static void DisplayArtifact(AgentArtifact artifact)
    {
        Panel panel = new Panel(
                Markup.Escape($"Artifact ID: {artifact.Id}\n" +
                              $"Filename: {artifact.FileName}\n" +
                              $"Path: {artifact.RelativePath ?? "N/A"}\n" +
                              $"Size: {artifact.Size / 1024.0:F2} KB\n" +
                              $"Agent Created: {(artifact.AgentCreated ? "Yes" : "No")}\n" +
                              $"Created At: {artifact.CreatedAt}")
            )
            .Header("Artifact Details")
            .Expand();

        AnsiConsole.Write(panel);
    }
}