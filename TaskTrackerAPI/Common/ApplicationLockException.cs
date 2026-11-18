namespace TaskTrackerAPI.Common
{
    public class ApplicationLockException: Exception
    {
        public int ResultCode { get; }
        public ApplicationLockErrorType ErrorType { get; }

        public ApplicationLockException(int resultCode)
            : base($"Failed to acquire application lock. SQL result: {resultCode}")
        {
            ResultCode = resultCode;

            ErrorType = resultCode switch
            {
                -1 => ApplicationLockErrorType.Timeout,
                -2 => ApplicationLockErrorType.Canceled,
                -3 => ApplicationLockErrorType.Deadlock,
                _ => ApplicationLockErrorType.GeneralError
            };
        }
    }
}
