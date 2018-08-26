// 
// This file is a part of MSM (Multi Server Manager)
// Copyright (C) 2016-2018 Michiel Hazelhof (michiel@hazelhof.nl)
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
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MSM.Data;
using MSM.Functions;
using Quartz;

namespace MSM.Service
{
    public static class Logging
    {
        static Logging()
        {
            Statics.CronService.CreateJob<LogJob>(0, 0, 3, true);

            if (!File.Exists(Path.Combine(FileOperations.GetRunningDirectory(), "Log.txt")))
            {
                FileOperations.CreateFile(Path.Combine(FileOperations.GetRunningDirectory(), "Log.txt"));
            }

            TextWriterFileStream = File.Open(Path.Combine(FileOperations.GetRunningDirectory(), "Log.txt"), FileMode.Append, FileAccess.Write, FileShare.Read);
            TextWriter = new StreamWriter(TextWriterFileStream, Encoding.UTF8, 512, true);

            Events.ShutDownFired += EventsShutDownFired;
        }
        private static void EventsShutDownFired()
        {
            Statics.CronService.RemoveJob<LogJob>();
            LogItemWorker();
        }

        private static readonly ConcurrentQueue<String> LogQueue = new ConcurrentQueue<String>();
        private static readonly TextWriter TextWriter;
        private static readonly FileStream TextWriterFileStream;

        public static void LogItem(String logItem)
        {
            LogQueue.Enqueue(logItem);
        }
        public static void LogErrorItem(Exception exception, Boolean logSilent = false, [CallerMemberName] String caller = null, [CallerLineNumber] Int32 lineNumber = 0)
        {
            if (!logSilent)
            {
                UI.ShowMessage(Variables.MainForm, "An serious error occured in \"" + caller + "\" on line " + lineNumber.ToString(CultureInfo.InvariantCulture) + " exception message: " + exception, "Exception", MessageBoxIcon.Error);
            }
        }

        internal static void LogItemWorker()
        {
            try
            {
                if (TextWriter == null) return;
                lock (TextWriter)
                {
                    Boolean written = LogQueue.Any();
                    while (LogQueue.TryDequeue(out String item))
                    {
                        if (!String.IsNullOrWhiteSpace(item))
                        {
                            TextWriter.WriteLine(item, Environment.NewLine);
                        }
                    }

                    if (!written) return;

                    TextWriter.Flush();
                    TextWriterFileStream.Flush(true);
                }
            }
            catch (Exception exception)
            {
                UI.ShowMessage(Variables.MainForm, "An exception occured during logging: " + exception, "Logger", MessageBoxIcon.Error);
            }
        }
    }

    [DisallowConcurrentExecution]
    internal class LogJob : IJob
    {
        public virtual Task Execute(IJobExecutionContext context)
        {
            Logging.LogItemWorker();
            return Task.CompletedTask;
        }
    }
}