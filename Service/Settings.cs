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
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Xml.Serialization;
using MSM.Data;
using MSM.Extends;
using MSM.Functions;
using Quartz;
using String = System.String;

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
                    Values = (Values) XMLSerializer.Deserialize(streamReader);
                    Values.Dirty = false;
                }
            }
            catch
            {
                Values = new Values();
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
            Values.CheckForUpdates = true;
        }

        public static Values Values = new Values();

        private static readonly XmlSerializer XMLSerializer = new XmlSerializer(typeof(Values));
        internal static void Flush()
        {
            if (!Values.Dirty) return;

            lock (Values)
            {
                using (StreamWriter writer = new StreamWriter(Path.Combine(FileOperations.GetRunningDirectory(), "Settings.xml")))
                {
                    XMLSerializer.Serialize(writer, Values);
                    writer.Flush();
                }
                Values.Dirty = false;
            }
        }
    }

    [Serializable]
    public class Values
    {
        [XmlIgnore, Browsable(false)]
        public Boolean Dirty { get; set; }
        
        [Category("Basic"), DisplayName("Automatically check for updates"), TypeConverter(typeof(BooleanYesNoConverter))]
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

        [Category("UI"), DisplayName("Minimize to the tray instead of the taskbar"), TypeConverter(typeof(BooleanYesNoConverter))]
        public Boolean MinimizeToTray
        {
            get => _minimizeToTray;
            set
            {
                if (_minimizeToTray != value)
                {
                    Dirty = true;
                }
                _minimizeToTray = value;
            }
        }
        [XmlIgnore] private Boolean _minimizeToTray;

        [Category("UI"), DisplayName("Always show the tray icon"), TypeConverter(typeof(BooleanYesNoConverter))]
        public Boolean AlwaysShowTrayIcon
        {
            get => _alwaysShowTrayIcon;
            set
            {
                if (_alwaysShowTrayIcon != value)
                {
                    Dirty = true;
                }

                if (value)
                {
                    ((Main)Variables.MainForm).NotifyIcon.Visible = true;
                }
                else
                {
                    if (Variables.MainForm.WindowState != FormWindowState.Minimized)
                    {
                        ((Main)Variables.MainForm).NotifyIcon.Visible = false;
                    }
                }
                _alwaysShowTrayIcon = value;
            }
        }
        [XmlIgnore] private Boolean _alwaysShowTrayIcon = true;

        [Category("UI"), DisplayName("Maximize on start"), TypeConverter(typeof(BooleanYesNoConverter))]
        public Boolean MaximizeOnStart
        {
            get => _maximizeOnStart;
            set
            {
                if (_maximizeOnStart != value)
                {
                    Dirty = true;
                }
                _maximizeOnStart = value;
            }
        }
        [XmlIgnore] private Boolean _maximizeOnStart = true;

        [Category("UI"), DisplayName("Action to perform when closing"), TypeConverter(typeof(EnumDescriptionConverter<Enumerations.CloseAction>))]
        public Enumerations.CloseAction CloseAction
        {
            get => _closeAction;
            set
            {
                if (_closeAction != value)
                {
                    Dirty = true;
                }
                _closeAction = value;
            }
        }
        [XmlIgnore] private Enumerations.CloseAction _closeAction = Enumerations.CloseAction.Close;

        [Category("Putty"), DisplayName("Putty executable"), Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
        public String PuttyExecutable
        {
            get => _puttyExecutable;
            set
            {
                if (_puttyExecutable != value)
                {
                    Dirty = true;
                }
                _puttyExecutable = value;
            }
        }
        [XmlIgnore] private String _puttyExecutable;

        [Category("Sessions"), DisplayName("Which sessions to automatically start"), TypeConverter(typeof(EnumDescriptionConverter<Enumerations.InitialSessions>))]
        public Enumerations.InitialSessions InitialSessions
        {
            get => _initialSessions;
            set
            {
                if (_initialSessions != value)
                {
                    Dirty = true;
                }
                _initialSessions = value;
            }
        }
        [XmlIgnore] private Enumerations.InitialSessions _initialSessions = Enumerations.InitialSessions.Previous;

        [Browsable(false)]
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

        [Browsable(false)]
        public Boolean ShowServerList
        {
            get => _showServerList;
            set
            {
                if (_showServerList != value)
                {
                    Dirty = true;
                }
                _showServerList = value;
            }
        }
        [XmlIgnore] private Boolean _showServerList = true;

        [Category("Servers"), DisplayName("Available keywords"), TypeConverter(typeof(CsvConverter))]
        public String[] Keywords
        {
            get => _keywords;
            set
            {
                if (!_keywords.Equals(value))
                {
                    Dirty = true;
                }
                _keywords = value;
            }
        }
        [XmlIgnore] private String[] _keywords;
        
        [Category("Servers"), DisplayName("Serverlist")]
        public CollectionConverter<Node> Servers
        {
            get => _nodeList;
            set
            {
                if (_nodeList != value)
                {
                    Dirty = true;
                }
                _nodeList = value;
            }
        }
        [XmlIgnore] private CollectionConverter<Node> _nodeList = new CollectionConverter<Node>();
    }

    public class Node
    {
        public override String ToString()
        {
            return String.IsNullOrEmpty(NodeName) ? "?" : NodeName;
        }

        [Category("Node"), DisplayName("Node name")]
        public String NodeName
        {
            get => _nodeName;
            set
            {
                if (_nodeName != value)
                {
                    Settings.Values.Dirty = true;
                }
                _nodeName = value;
            }
        }
        [XmlIgnore] private String _nodeName = "/";

        [Category("Servers"), DisplayName("Serverlist")]
        public CollectionConverter<Server> ServerList { get; set; }
    }
    public class Server
    {
        public override String ToString()
        {
            return String.IsNullOrEmpty(DisplayName) ? "?" : DisplayName;
        }

        [Category("Basic"), DisplayName("Display name")]
        public String DisplayName
        {
            get => _displayName;
            set
            {
                if (_displayName != value)
                {
                    Settings.Values.Dirty = true;
                }
                _displayName = value;
            }
        }
        [XmlIgnore] private String _displayName;

        [Category("Basic"), DisplayName("Keywords")]
        [Editor(typeof(CheckedListBoxUITypeEditor), typeof(UITypeEditor)), TypeConverter(typeof(CsvConverter))]
        [CheckedListBoxUITypeEditor.Arguments(Enumerations.CheckedListBoxSetting.ServerKeywords)]
        public String[] Keywords
        {
            get => _keywords;
            set
            {
                if (!_keywords.Equals(value))
                {
                    Settings.Values.Dirty = true;
                }
                _keywords = value;
            }
        }
        [XmlIgnore] private String[] _keywords;

        [Category("Server"), DisplayName("Hostname")]
        public String Hostname
        {
            get => _hostName;
            set
            {
                if (_hostName != value)
                {
                    Settings.Values.Dirty = true;
                }
                _hostName = value;
            }
        }
        [XmlIgnore] private String _hostName;

        [Category("Server"), DisplayName("Port number")]
        public UInt16 Port
        {
            get => _portNumber;
            set
            {
                if (_portNumber != value)
                {
                    Settings.Values.Dirty = true;
                }
                _portNumber = value;
            }
        }
        [XmlIgnore] private UInt16 _portNumber = 22;
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