using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SFTP_Synchronize
{
    /// <summary>
    /// Used for logging all events taking place in each process of the Application. Windows Event Viewer or other Event Log viewers (i.e. NetCrunch WMI Tool) can be used for viewing the log
    /// </summary>
    public static class EventLogging
    {
        public static EventLog AppEventLog = new EventLog("Application");
        public static int AppLoggingEventId = 0;
        public static short AppLoggingCategory = 0;

        /// <summary>
        /// Writes the actual message in the Event Logging
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="type">Type of message: Information, SuccessAudit, Error</param>
        public static void WriteEntry(string message, EventLogEntryType type)
        {
            AppEventLog.WriteEntry(message, type, AppLoggingEventId, AppLoggingCategory);
        }
    }
}
