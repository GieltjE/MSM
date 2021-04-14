// 
// This file is a part of MSM (Multi Server Manager)
// Copyright (C) 2016-2021 Michiel Hazelhof (michiel@hazelhof.nl)
// 
// MSM is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// MSM is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// If not, see <http://www.gnu.org/licenses/>.
//

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
        public static event Action<DateTime, Enumerations.LogTarget, Enumerations.LogLevel, String> LogAdded;

        public static void Log(Enumerations.LogTarget target, Enumerations.LogLevel level, String logMessage, Exception exception)
        {
            if (!Variables.ShutDownFired)
            {
                LogAdded?.BeginInvoke(DateTime.Now, target, level, logMessage + (exception == null ? "" : " (" + exception + ")"), null, null).AutoEndInvoke(Variables.MainForm);
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
