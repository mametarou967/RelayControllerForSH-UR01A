namespace RelayControllerForSHUR01A.Model.Logging
{
    public interface ILogWriteRequester
    {
        void WriteRequest(LogLevel logLevel, string message);
    }
}
