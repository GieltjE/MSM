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
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using MSM.Data;
using MSM.Functions;
using Quartz;

namespace MSM.Service
{
    public static class Settings
    {
        static Settings()
        {
            if (!File.Exists(Path.Combine(FileOperations.GetRunningDirectory(), "Settings.xml")))
            {
                FileOperations.CreateFile(Path.Combine(FileOperations.GetRunningDirectory(), "Settings.xml"));

                // Load some defaults
                SetDefaults();
            }

            try
            {
                using (StreamReader streamReader = new StreamReader(Path.Combine(FileOperations.GetRunningDirectory(), "Settings.xml")))
                {
                    SettingsClass = (MSMSettings) XMLSerializer.Deserialize(streamReader);
                    SettingsClass.Dirty = false;
                }
            }
            catch
            {
                SettingsClass = new MSMSettings();
                SetDefaults();
            }

            Statics.CronService.CreateJob<FlushTerminalSettings>(0, 0, 5, true);

            Events.ShutDownFired += EventsShutDownFired;
        }
        private static void EventsShutDownFired()
        {
            Statics.CronService.RemoveJob<FlushTerminalSettings>();
            Flush();
        }

        private static void SetDefaults()
        {
            SettingsClass.CheckForUpdates = true;
        }

        public static MSMSettings SettingsClass = new MSMSettings();

        private static readonly XmlSerializer XMLSerializer = new XmlSerializer(typeof(MSMSettings));
        internal static void Flush()
        {
            if (!SettingsClass.Dirty) return;

            lock (SettingsClass)
            {
                using (StreamWriter writer = new StreamWriter(Path.Combine(FileOperations.GetRunningDirectory(), "Settings.xml")))
                {
                    XMLSerializer.Serialize(writer, SettingsClass);
                    writer.Flush();
                }
                SettingsClass.Dirty = false;
            }
        }
    }

    [Serializable]
    public class MSMSettings
    {
        [XmlIgnore] public Boolean Dirty;
        
        public Boolean CheckForUpdates
        {
            get => _checkForUpdates;
            set
            {
                if (_checkForUpdates != value)
                {
                    Dirty = true;
                }
                _checkForUpdates = value;
            }
        }
        [XmlIgnore] private Boolean _checkForUpdates;

        public Enumerations.Themes Theme
        {
            get => _theme;
            set
            {
                if (_theme != value)
                {
                    Dirty = true;
                }
                _theme = value;
            }
        }
        [XmlIgnore] private Enumerations.Themes _theme = Enumerations.Themes.Dark;
    }

    [DisallowConcurrentExecution]
    internal class FlushTerminalSettings : IJob
    {
        public virtual Task Execute(IJobExecutionContext context)
        {
            Settings.Flush();
            return Task.CompletedTask;
        }
    }
}