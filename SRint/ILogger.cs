using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRint
{
    interface ILogger
    {
        event EventHandler<LoggerEventArgs> OnErrorOccured;
        event EventHandler<LoggerEventArgs> OnWarningOccured;
        event EventHandler<LoggerEventArgs> OnNoticeOccured;

        void LogError(string errorMessage);
        void LogWarning(string warningMessage);
        void LogNotice(string noticeMessage);
    }

    public class LoggerEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}
