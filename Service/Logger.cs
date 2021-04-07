using System;
using System.Reflection;
using log4net;
using MSM.Data;
using MSM.Functions;

namespace MSM.Service
{
    public static class Logger
    {
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static event Action<Enumerations.LogTarget, Enumerations.LogLevel, String> LogAdded;

        public static void Log(Enumerations.LogTarget target, Enumerations.LogLevel level, String logMessage, Exception exception)
        {
            if (!Variables.ShutDownFired)
            {
                LogAdded?.BeginInvoke(target, level, logMessage + (exception == null ? "" : " (" + exception + ")"), null, null).AutoEndInvoke(Variables.MainForm);
            }

            logMessage = $"[{target}] {logMessage}";
            
            switch (level)
            {
                case Enumerations.LogLevel.Info:
                    Log4Net.Info(logMessage, exception);
                    break;
                case Enumerations.LogLevel.Warn:
                    Log4Net.Warn(logMessage, exception);
                    break;
                case Enumerations.LogLevel.Error:
                    Log4Net.Error(logMessage, exception);
                    break;
                case Enumerations.LogLevel.Fatal:
                    Log4Net.Fatal(logMessage, exception);
                    break;
                case Enumerations.LogLevel.Debug:
                    Log4Net.Debug(logMessage, exception);
                    break;
            }
        }
    }
}