namespace TaskTrackerAPI.Common
{
    public enum ApplicationLockErrorType
    {
        Timeout = 1,
        Canceled = 2,
        Deadlock = 3,
        GeneralError = 4
    }
}
