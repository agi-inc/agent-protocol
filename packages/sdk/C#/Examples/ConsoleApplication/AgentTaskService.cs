using AnotherAgentProtocolLibrary;
#pragma warning disable CS8601 // Possible null reference assignment.

namespace ConsoleApplication
{
    public class AgentTaskService : IAgentTaskService
    {
        private readonly IApiClient _apiClient;

        public AgentTaskService(IApiClient apiClient)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        public async Task<AgentTask> CreateTaskAsync(string input, object? additionalInput = null)
        {
            Body body = new() { Input = input };
            if (additionalInput != null)
            {
                body.Additional_input = additionalInput;
            }

            Response response = await _apiClient.CreateAgentTaskAsync(body);
            return MapToAgentTask(response);
        }

        public async Task<PaginatedData<AgentTask>> GetTasksAsync(int? page = null, int? pageSize = null)
        {
            Response2 response = await _apiClient.ListAgentTasksAsync(page, pageSize);

            PaginatedData<AgentTask> result = new()
            {
                CurrentPage = response.Pagination.Current_page,
                PageSize = response.Pagination.Page_size,
                TotalCount = response.Pagination.Total_items,
                TotalPages = response.Pagination.Total_pages,
            };

            return result;
        }

        public async Task<AgentTask> GetTaskAsync(string taskId)
        {
            Response3 response = await _apiClient.GetAgentTaskAsync(taskId);
            return MapToAgentTask(response);
        }

        public async Task<PaginatedData<AgentStep>> GetTaskStepsAsync(string taskId, int? page = null, int? pageSize = null)
        {
            Response4 response = await _apiClient.ListAgentTaskStepsAsync(taskId, page, pageSize);

            PaginatedData<AgentStep> result = new()
            {
                CurrentPage = response.Pagination.Current_page,
                PageSize = response.Pagination.Page_size,
                TotalCount = response.Pagination.Total_items,
                TotalPages = response.Pagination.Total_pages,
            };

            return result;
        }

        public async Task<AgentStep> ExecuteTaskStepAsync(string taskId, string input, object? additionalInput = null)
        {
            Body2 body = new() { Input = input };
            if (additionalInput != null)
            {
                body.Additional_input = additionalInput;
            }

            Response5 response = await _apiClient.ExecuteAgentTaskStepAsync(taskId, body);
            return MapToAgentStep(response);
        }

        public async Task<AgentStep> GetTaskStepAsync(string taskId, string stepId)
        {
            Response6 response = await _apiClient.GetAgentTaskStepAsync(taskId, stepId);
            return MapToAgentStep(response);
        }

        public async Task<PaginatedData<AgentArtifact>> GetTaskArtifactsAsync(string taskId, int? page = null, int? pageSize = null)
        {
            Response7 response = await _apiClient.ListAgentTaskArtifactsAsync(taskId, page, pageSize);

            PaginatedData<AgentArtifact> result = new()
            {
                CurrentPage = response.Pagination.Current_page,
                PageSize = response.Pagination.Page_size,
                TotalCount = response.Pagination.Total_items,
                TotalPages = response.Pagination.Total_pages,
                Data = response.Artifacts.Select(MapToAgentArtifact).ToList()
            };

            return result;
        }

        public async Task<AgentArtifact> UploadArtifactAsync(string taskId, string filePath, string relativePath)
        {
            // Read the file
            byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
            string fileName = Path.GetFileName(filePath);

            // Create FileParameter
            FileParameter fileParameter = new(
                new MemoryStream(fileBytes),
                fileName,
                GetMimeType(fileName)
            );

            Response8 response = await _apiClient.UploadAgentTaskArtifactsAsync(taskId, fileParameter, relativePath);
            return MapToAgentArtifact(response);
        }

        public async Task<FileInfo> DownloadArtifactAsync(string taskId, string artifactId, string outputPath)
        {
            FileResponse response = await _apiClient.DownloadAgentTaskArtifactAsync(taskId, artifactId);

            await using (FileStream fileStream = File.Create(outputPath))
            {
                await response.Stream.CopyToAsync(fileStream);
            }

            return new FileInfo(outputPath);
        }

        #region Helper Methods

        private AgentTask MapToAgentTask(Response response)
        {
            return new AgentTask
            {
                Id = response.Task_id,
                Input = response.Input,
                Status = "Created", // Assuming status from Response, which isn't explicitly defined in SDK
                CreatedAt = DateTime.Now, // SDK doesn't appear to provide creation time in the response
                Artifacts = response.Artifacts?.Select(MapToAgentArtifact).ToList() ?? []
            };
        }

        private AgentTask MapToAgentTask(Response3 response)
        {
            return new AgentTask
            {
                Id = response.Task_id,
                Input = response.Input,
                Status = "Active", // Assuming status
                CreatedAt = DateTime.Now, // SDK doesn't appear to provide creation time
                Artifacts = response.Artifacts?.Select(MapToAgentArtifact).ToList() ?? []
            };
        }

        private AgentTask MapToAgentTask(Tasks task)
        {
            return new AgentTask
            {
                Id = task.Task_id,
                Input = task.Input,
                Status = "Active", // Assuming status
                CreatedAt = DateTime.Now, // Not provided in SDK
                Artifacts = task.Artifacts?.Select(MapToAgentArtifact).ToList() ?? []
            };
        }

        private AgentStep MapToAgentStep(Response5 response)
        {
            return new AgentStep
            {
                Id = response.Step_id,
                TaskId = response.Task_id,
                Input = response.Input,
                Output = response.Output,
                Status = response.Status.ToString(),
                AdditionalOutput = response.Additional_output,
                IsLast = response.Is_last,
                CreatedAt = DateTime.Now, // Not provided in SDK
                ExecutedAt = DateTime.Now, // Assuming just executed
                Artifacts = response.Artifacts?.Select(MapToAgentArtifact).ToList() ?? []
            };
        }

        private AgentStep MapToAgentStep(Response6 response)
        {
            return new AgentStep
            {
                Id = response.Step_id,
                TaskId = response.Task_id,
                Input = response.Input,
                Output = response.Output,
                Status = response.Status.ToString(),
                AdditionalOutput = response.Additional_output,
                IsLast = response.Is_last,
                CreatedAt = DateTime.Now, // Not provided in SDK
                ExecutedAt = response.Status == Response6Status.Completed ? DateTime.Now : null,
                Artifacts = response.Artifacts?.Select(MapToAgentArtifact).ToList() ?? []
            };
        }

        private AgentStep MapToAgentStep(Steps step)
        {
            return new AgentStep
            {
                Id = step.Step_id,
                TaskId = step.Task_id,
                Input = step.Input,
                Output = step.Output,
                Status = step.Status.ToString(),
                AdditionalOutput = step.Additional_output,
                IsLast = step.Is_last,
                CreatedAt = DateTime.Now, // Not provided in SDK
                ExecutedAt = step.Status == StepsStatus.Completed ? DateTime.Now : null,
                Artifacts = step.Artifacts?.Select(MapToAgentArtifact).ToList() ?? []
            };
        }

        private AgentArtifact MapToAgentArtifact(artifacts artifact)
        {
            return new AgentArtifact
            {
                Id = artifact.Artifact_id,
                AgentCreated = artifact.Agent_created,
                FileName = artifact.File_name,
                RelativePath = artifact.Relative_path,
                Size = 0, // Size not provided in SDK
                CreatedAt = DateTime.Now // Not provided in SDK
            };
        }

        private AgentArtifact MapToAgentArtifact(artifacts2 artifact)
        {
            return new AgentArtifact
            {
                Id = artifact.Artifact_id,
                AgentCreated = artifact.Agent_created,
                FileName = artifact.File_name,
                RelativePath = artifact.Relative_path,
                Size = 0, // Size not provided
                CreatedAt = DateTime.Now // Not provided
            };
        }

        private AgentArtifact MapToAgentArtifact(artifacts3 artifact)
        {
            return new AgentArtifact
            {
                Id = artifact.Artifact_id,
                AgentCreated = artifact.Agent_created,
                FileName = artifact.File_name,
                RelativePath = artifact.Relative_path,
                Size = 0, // Size not provided
                CreatedAt = DateTime.Now // Not provided
            };
        }

        private AgentArtifact MapToAgentArtifact(artifacts4 artifact)
        {
            return new AgentArtifact
            {
                Id = artifact.Artifact_id,
                AgentCreated = artifact.Agent_created,
                FileName = artifact.File_name,
                RelativePath = artifact.Relative_path,
                Size = 0,
                CreatedAt = DateTime.Now
            };
        }

        private AgentArtifact MapToAgentArtifact(artifacts5 artifact)
        {
            return new AgentArtifact
            {
                Id = artifact.Artifact_id,
                AgentCreated = artifact.Agent_created,
                FileName = artifact.File_name,
                RelativePath = artifact.Relative_path,
                Size = 0,
                CreatedAt = DateTime.Now
            };
        }

        private AgentArtifact MapToAgentArtifact(artifacts6 artifact)
        {
            return new AgentArtifact
            {
                Id = artifact.Artifact_id,
                AgentCreated = artifact.Agent_created,
                FileName = artifact.File_name,
                RelativePath = artifact.Relative_path,
                Size = 0,
                CreatedAt = DateTime.Now
            };
        }

        private AgentArtifact MapToAgentArtifact(artifacts7 artifact)
        {
            return new AgentArtifact
            {
                Id = artifact.Artifact_id,
                AgentCreated = artifact.Agent_created,
                FileName = artifact.File_name,
                RelativePath = artifact.Relative_path,
                Size = 0,
                CreatedAt = DateTime.Now
            };
        }

        private AgentArtifact MapToAgentArtifact(artifacts8 artifact)
        {
            return new AgentArtifact
            {
                Id = artifact.Artifact_id,
                AgentCreated = artifact.Agent_created,
                FileName = artifact.File_name,
                RelativePath = artifact.Relative_path,
                Size = 0,
                CreatedAt = DateTime.Now
            };
        }

        private AgentArtifact MapToAgentArtifact(artifacts9 artifact)
        {
            return new AgentArtifact
            {
                Id = artifact.Artifact_id,
                AgentCreated = artifact.Agent_created,
                FileName = artifact.File_name,
                RelativePath = artifact.Relative_path,
                Size = 0,
                CreatedAt = DateTime.Now
            };
        }

        private AgentArtifact MapToAgentArtifact(artifacts10 artifact)
        {
            return new AgentArtifact
            {
                Id = artifact.Artifact_id,
                AgentCreated = artifact.Agent_created,
                FileName = artifact.File_name,
                RelativePath = artifact.Relative_path,
                Size = 0,
                CreatedAt = DateTime.Now
            };
        }

        private AgentArtifact MapToAgentArtifact(artifacts11 artifact)
        {
            return new AgentArtifact
            {
                Id = artifact.Artifact_id,
                AgentCreated = artifact.Agent_created,
                FileName = artifact.File_name,
                RelativePath = artifact.Relative_path,
                Size = 0,
                CreatedAt = DateTime.Now
            };
        }

        private AgentArtifact MapToAgentArtifact(Response8 artifact)
        {
            return new AgentArtifact
            {
                Id = artifact.Artifact_id,
                AgentCreated = artifact.Agent_created,
                FileName = artifact.File_name,
                RelativePath = artifact.Relative_path,
                Size = 0, // Not available in response
                CreatedAt = DateTime.Now // Not available in response
            };
        }

        private AgentArtifact MapToAgentArtifact(Artifacts artifact)
        {
            return new AgentArtifact
            {
                Id = artifact.Artifact_id,
                AgentCreated = artifact.Agent_created,
                FileName = artifact.File_name,
                RelativePath = artifact.Relative_path,
                Size = 0, // Not available
                CreatedAt = DateTime.Now // Not available
            };
        }

        private string GetMimeType(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".txt" => "text/plain",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".csv" => "text/csv",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }

        #endregion
    }
}
