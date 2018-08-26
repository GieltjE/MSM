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
            String portableSettingsFile = Path.Combine(FileOperations.GetRunningDirectory(), "Settings.xml");
            String localSettingsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MSM", "Settings.xml");
            
            if (File.Exists(portableSettingsFile))
            {
                SettingsFile = portableSettingsFile;
                ReadSettings();
            }
            else if (File.Exists(localSettingsFile))
            {
                SettingsFile = localSettingsFile;
                ReadSettings();
            }

            if (SettingsFile == null)
            {
                SettingsFile = UI.AskQuestion(Variables.MainForm, "Create portable settings file?", "No settings file found", MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button1, MessageBoxIcon.Question) == DialogResult.Yes ? portableSettingsFile : localSettingsFile;
                FileOperations.CreateFile(SettingsFile);
                Values.Dirty = true;
                Flush();
            }

            Statics.CronService.CreateJob<FlushTerminalSettings>(0, 0, 5, true);

            Events.ShutDownFired += EventsShutDownFired;
        }
        private static void ReadSettings()
        {
            try
            {
                using (StreamReader streamReader = new StreamReader(SettingsFile))
                {
                    Values = (Values)XMLSerializer.Deserialize(streamReader);
                    Values.Dirty = false;
                }
            }
            catch
            {
                Values = new Values();
            }
        }
        private static void EventsShutDownFired()
        {
            Statics.CronService.RemoveJob<FlushTerminalSettings>();
            Flush();
        }

        public static Values Values = new Values();

        private static readonly XmlSerializer XMLSerializer = new XmlSerializer(typeof(Values));
        private static readonly String SettingsFile;
        internal static void Flush()
        {
            if (!Values.Dirty) return;

            lock (Values)
            {
                using (StreamWriter writer = new StreamWriter(SettingsFile))
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
        [XmlIgnore] private Boolean _checkForUpdates = true;

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
                    ((Main)Data.Variables.MainForm).NotifyIcon.Visible = true;
                }
                else if(Data.Variables.MainForm.WindowState != FormWindowState.Minimized)
                {
                    ((Main)Data.Variables.MainForm).NotifyIcon.Visible = false;
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
                if (!String.Equals(_puttyExecutable, value, StringComparison.Ordinal))
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

        [Category("Servers"), DisplayName("Available keywords"), TypeConverter(typeof(CsvConverter)), XmlArrayItem(ElementName = "Keyword")]
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
        [XmlIgnore] private String[] _keywords = new String[0];

        [Category("Servers"), DisplayName("Available Variables"), TypeConverter(typeof(CsvConverter)), XmlArrayItem(ElementName = "Variable")]
        public String[] Variables
        {
            get => _variables;
            set
            {
                if (!_variables.Equals(value))
                {
                    Dirty = true;
                }
                _variables = value;
            }
        }
        [XmlIgnore] private String[] _variables = new String[0];

        [Category("Servers"), DisplayName("Serverlist")]
        public CollectionConverter<Node> Servers
        {
            get => _nodeList;
            set
            {
                Dirty = true;
                _nodeList = value;
            }
        }
        [XmlIgnore] private CollectionConverter<Node> _nodeList = new CollectionConverter<Node>();
    }
    [Serializable]
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
        public CollectionConverter<Server> ServerList
        {
            get => _serverList;
            set
            {
                Settings.Values.Dirty = true;
                _serverList = value;
            }
        }
        private CollectionConverter<Server> _serverList = new CollectionConverter<Server>();
    }
    [Serializable]
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
                if (!String.Equals(_displayName, value, StringComparison.Ordinal))
                {
                    Settings.Values.Dirty = true;
                }

                _displayName = value;
            }
        }
        [XmlIgnore] private String _displayName;

        [Category("Server"), DisplayName("Hostname")]
        public String Hostname
        {
            get => _hostName;
            set
            {
                if (!String.Equals(_hostName, value, StringComparison.Ordinal))
                {
                    Settings.Values.Dirty = true;
                }

                _hostName = value;
            }
        }
        [XmlIgnore] private String _hostName;

        [Category("Server"), DisplayName("Username")]
        public String Username
        {
            get => _username;
            set
            {
                if (!String.Equals(_username, value, StringComparison.Ordinal))
                {
                    Settings.Values.Dirty = true;
                }

                _username = value;
            }
        }
        [XmlIgnore] private String _username;

        [Category("Server"), DisplayName("Password"), Description("Warning: passwords ARE NOT ENCRYPTED")]
        public String Password
        {
            get => _password;
            set
            {
                if (!String.Equals(_password, value, StringComparison.Ordinal))
                {
                    Settings.Values.Dirty = true;
                }

                if (!String.IsNullOrWhiteSpace(value))
                {
                    UI.ShowMessage(null, "Warning: passwords are NOT encrypted, please use other authentication methods!", "NO ENCRYPTION", MessageBoxIcon.Stop);
                }

                _password = value;
            }
        }
        [XmlIgnore] private String _password;

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

        [Category("Basic"), DisplayName("Keywords"), XmlArrayItem(ElementName = "Keyword"), Editor(typeof(CheckedListBoxUITypeEditor), typeof(UITypeEditor)), TypeConverter(typeof(CsvConverter)), Arguments(Enumerations.CheckedListBoxSetting.ServerKeywords)]
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
        [XmlIgnore] private String[] _keywords = new String[0];

        [Browsable(false)]
        public Variable[] Variables
        {
            get => _variables.Properties;
            set => _variables.Properties = value;
        }
        [XmlIgnore, Category("Basic"), DisplayName("Variables"), Arguments(Enumerations.CheckedListBoxSetting.ServerVariables)]
        public BasicPropertyBag VariablesInternal
        {
            get => _variables;
            set
            {
                Settings.Values.Dirty = true;

                _variables = value;
            }
        }
        [XmlIgnore] private BasicPropertyBag _variables = new BasicPropertyBag();
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