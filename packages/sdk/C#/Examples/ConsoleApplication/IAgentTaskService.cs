namespace ConsoleApplication;

#pragma warning disable CS8601
public interface IAgentTaskService
{
    Task<AgentTask> CreateTaskAsync(string input, object? additionalInput = null);
    Task<PaginatedData<AgentTask>> GetTasksAsync(int? page = null, int? pageSize = null);
    Task<AgentTask> GetTaskAsync(string taskId);
    Task<PaginatedData<AgentStep>> GetTaskStepsAsync(string taskId, int? page = null, int? pageSize = null);
    Task<AgentStep> ExecuteTaskStepAsync(string taskId, string input, object? additionalInput = null);
    Task<AgentStep> GetTaskStepAsync(string taskId, string stepId);
    Task<PaginatedData<AgentArtifact>> GetTaskArtifactsAsync(string taskId, int? page = null, int? pageSize = null);
    Task<AgentArtifact> UploadArtifactAsync(string taskId, string filePath, string relativePath);
    Task<FileInfo> DownloadArtifactAsync(string taskId, string artifactId, string outputPath);
}