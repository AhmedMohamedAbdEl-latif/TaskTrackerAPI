using System.Text.Json.Serialization;

namespace TaskTrackerAPI.Common
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public ServiceErrorType? ErrorType { get; set; }
        public List<string> Errors { get; set; } = new();

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TraceId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? RequiresTwoFactor { get; set; }
    }

    public class ServiceResult<T> : ServiceResult
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; set; }
    }
}
