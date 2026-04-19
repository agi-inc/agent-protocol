// ReSharper disable All
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
namespace ConsoleApplication
{
    // Simplified model classes that wrap the SDK response types
    // to make them easier to use in the console application

    public class AgentTask
    {
        public string Id { get; set; }
        public string Input { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<AgentArtifact> Artifacts { get; set; } = new();
    }

    public class AgentStep
    {
        public string Id { get; set; }
        public string TaskId { get; set; }
        public string Input { get; set; }
        public string Output { get; set; }
        public string Status { get; set; }
        public object AdditionalOutput { get; set; }
        public bool IsLast { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExecutedAt { get; set; }
        public List<AgentArtifact> Artifacts { get; set; } = new();
    }

    public class AgentArtifact
    {
        public string Id { get; set; }
        public bool AgentCreated { get; set; }
        public string FileName { get; set; }
        public string RelativePath { get; set; }
        public long Size { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PaginatedData<T>
    {
        public List<T> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}