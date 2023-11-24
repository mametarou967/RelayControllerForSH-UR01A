using Prism.Events;
using System;
using System.Windows;

namespace RelayControllerForSHUR01A.Model.Logging
{
    public class LogWriteRequester : ILogWriteRequester
    {
        IEventAggregator _ea;

        public LogWriteRequester(IEventAggregator ea)
        {
            _ea = ea;
        }

        public void WriteRequest(LogLevel logLevel, string message)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _ea.GetEvent<LogEvent>().Publish(
                    new Log()
                    {
                        dateTime = DateTime.Now,
                        content = message,
                        logLevel = logLevel
                    });
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
