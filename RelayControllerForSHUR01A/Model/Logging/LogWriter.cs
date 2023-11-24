using Prism.Events;
using System;

namespace RelayControllerForSHUR01A.Model.Logging
{
    class LogWriter
    {
        public bool LogUpdatedEventFlag { get; set; } = true;

        IEventAggregator _ea;
        Action<Log> logWrite;

        public LogWriter(IEventAggregator ea, Action<Log> logWrite)// Collection<LogItem> logItems)
        {
            _ea = ea;
            this.logWrite = logWrite;

            _ea.GetEvent<LogEvent>().Subscribe(Write);
        }

        public void Write(Log log)
        {
            logWrite(log);
            if (LogUpdatedEventFlag) _ea.GetEvent<LogUpdated>().Publish(true);
        }
    }
}
