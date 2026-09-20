using System.Text.Json.Serialization;

namespace TaskTrackerAPI.Common
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ServiceErrorType
    {
        Validation,
        NotFound,
        Forbidden
    }
}
