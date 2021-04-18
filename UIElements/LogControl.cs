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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MSM.Data;
using MSM.Extends;
using MSM.Functions;
using MSM.Service;
using Quartz;

namespace MSM.UIElements
{
    public partial class LogControl : UserControlOptimized
    {
        internal static DataGridView DataGridView;
        internal static DataTable DataTable;
        internal static ConcurrentQueue<(DateTime dateTime, Enumerations.LogTarget target, Enumerations.LogLevel level, String message)> UIQueue = new();
        
        private readonly DataTable _logDataTable = new();
        public LogControl()
        {
            InitializeComponent();

            _logDataTable.Columns.Add("DateTime", typeof(DateTime));
            _logDataTable.Columns.Add("Target", typeof(Enumerations.LogTarget));
            _logDataTable.Columns.Add("Level", typeof(Enumerations.LogLevel));
            _logDataTable.Columns.Add("Message", typeof(String));

            BindingSource bindingSource = new() { DataSource = _logDataTable };
            DataGridView_Logs.DataSource = bindingSource;
            DataGridView_Logs.Columns[0].MinimumWidth = 200;
            DataGridView_Logs.Columns[1].MinimumWidth = 200;
            DataGridView_Logs.Columns[2].MinimumWidth = 200;
            DataGridView_Logs.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            DataGridView = DataGridView_Logs;
            DataTable = _logDataTable;

            Cron.CreateJob<LogToUI>("LogToUI", 0, 0, 2, true);
        }

        public void LoggerOnLogAdded(DateTime dateTime, Enumerations.LogTarget target, Enumerations.LogLevel level, String message)
        {
            UIQueue.Enqueue((dateTime, target, level, message));
        }
        private static readonly List<(DateTime dateTime, Enumerations.LogTarget target, Enumerations.LogLevel level, String message)> ToShow = new();
        internal static void ProcessUIQueue()
        {
            try
            {
                while (UIQueue.TryDequeue(out (DateTime dateTime, Enumerations.LogTarget target, Enumerations.LogLevel level, String message) item))
                {
                    ToShow.Add(item);
                }

                if (!ToShow.Any()) return;

                DataTable.BeginLoadData();
                foreach ((DateTime dateTime, Enumerations.LogTarget target, Enumerations.LogLevel level, String message) in ToShow.OrderByDescending(x => x.dateTime))
                {
                    DataRow row = DataTable.NewRow();
                    row[0] = dateTime;
                    row[1] = target;
                    row[2] = level;
                    row[3] = message;
                    DataTable.Rows.InsertAt(row, 0);
                }

                if (DataTable.Rows.Count > Settings.Values.MaxVisibleLogLines)
                {
                    for (Int32 i = DataTable.Rows.Count; i > Settings.Values.MaxVisibleLogLines; i--)
                    {
                        DataTable.Rows.RemoveAt(i);
                    }
                }

                DataGridView.BeginInvoke(new Action(DataTable.EndLoadData)).AutoEndInvoke(DataGridView);
                ToShow.Clear();
            }
            catch (Exception exception)
            {
                Logger.Log(Enumerations.LogTarget.General, Enumerations.LogLevel.Fatal, "Could not display log line to the UI", exception);
            }
        }
    }

    [DisallowConcurrentExecution, ResetTimerAfterRunCompletes]
    internal class LogToUI : IJob
    {
        public virtual Task Execute(IJobExecutionContext context)
        {
            LogControl.ProcessUIQueue();
            return Task.CompletedTask;
        }
    }
}
