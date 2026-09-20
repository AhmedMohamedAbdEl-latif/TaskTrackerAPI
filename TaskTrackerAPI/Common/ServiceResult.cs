namespace TaskTrackerAPI.Common
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public ServiceErrorType? ErrorType { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; set; }
    }
}
