using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRint
{
    public sealed class Logger : ILogger
    {
        public event EventHandler<LoggerEventArgs> OnErrorOccured;
        public event EventHandler<LoggerEventArgs> OnWarningOccured;
        public event EventHandler<LoggerEventArgs> OnNoticeOccured;

        private static readonly Logger instance = new Logger();
        private Logger() {}
        public static Logger Instance
        {
            get
            {
                return instance;
            }
        }

        public void LogError(string errorMessage)
        {
            if (OnErrorOccured != null)
            {
                LoggerEventArgs args = new LoggerEventArgs();
                args.Message = errorMessage;
                OnErrorOccured(this, args);
            }
        }

        public void LogWarning(string warningMessage)
        {
            if (OnWarningOccured != null)
            {
                LoggerEventArgs args = new LoggerEventArgs();
                args.Message = warningMessage;
                OnWarningOccured(this, args);
            }
        }

        public void LogNotice(string noticeMessage)
        {
            if (OnNoticeOccured != null)
            {
                LoggerEventArgs args = new LoggerEventArgs();
                args.Message = noticeMessage;
                OnNoticeOccured(this, args);
            }
        }
    }
}
