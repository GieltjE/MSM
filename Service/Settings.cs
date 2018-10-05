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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Xml.Serialization;
using MSM.Data;
using MSM.Extends;
using MSM.Functions;
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
                Flush();
            }
        }
        private static void ReadSettings()
        {
            _readingSettings = true;
            try
            {
                using (StreamReader streamReader = new StreamReader(SettingsFile))
                {
                    Values = (Values)XMLSerializer.Deserialize(streamReader);
                }
            }
            catch
            {
                Values = new Values();
            }
            _readingSettings = false;
        }

        public static Values Values = new Values();
        public static event ExtensionMethods.CustomDelegate OnSettingsUpdatedEvent;
        public static event ExtensionMethods.CustomDelegate OnSettingsServerUpdatedEvent;
        public static void FireOnSettingsServerUpdatedEvent()
        {
            OnSettingsServerUpdatedEvent?.Invoke();
        }

        public static Server FindServer(String nodeID, CollectionConverter<Node> firstNode = null)
        {
            if (firstNode == null)
            {
                firstNode = Values.Nodes;
            }

            foreach (Node node in firstNode)
            {
                foreach (Server server in node.ServerList)
                {
                    if (String.Equals(server.NodeID, nodeID, StringComparison.Ordinal))
                    {
                        return server;
                    }
                }

                Server foundServer = FindServer(nodeID, node.Nodes);
                if (foundServer != null)
                {
                    return foundServer;
                }
            }

            return null;
        }

        private static Boolean _readingSettings;
        private static readonly XmlSerializer XMLSerializer = new XmlSerializer(typeof(Values));
        private static readonly String SettingsFile;
        internal static void Flush()
        {
            if (_readingSettings) return;

            lock (Values)
            {
                using (StreamWriter writer = new StreamWriter(SettingsFile))
                {
                    XMLSerializer.Serialize(writer, Values);
                    writer.Flush();
                }
            }

            OnSettingsUpdatedEvent?.Invoke();
        }
    }

    [Serializable]
    public class Values
    {
        [Category("Basic"), DisplayName("Automatically check for updates"), TypeConverter(typeof(BooleanYesNoConverter))]
        public Boolean CheckForUpdates
        {
            get => _checkForUpdates;
            set
            {
                Boolean update = _checkForUpdates != value;
                _checkForUpdates = value;

                if (update)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Boolean _checkForUpdates = true;

        [Category("UI"), DisplayName("Minimize to the tray instead of the taskbar"), TypeConverter(typeof(BooleanYesNoConverter))]
        public Boolean MinimizeToTray
        {
            get => _minimizeToTray;
            set
            {
                Boolean update = _minimizeToTray != value;
                _minimizeToTray = value;

                if (update)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Boolean _minimizeToTray;

        [Category("UI"), DisplayName("Always show the tray icon"), TypeConverter(typeof(BooleanYesNoConverter))]
        public Boolean AlwaysShowTrayIcon
        {
            get => _alwaysShowTrayIcon;
            set
            {
                Boolean update = _alwaysShowTrayIcon != value;

                if (value)
                {
                    Data.Variables.MainForm.NotifyIcon.Visible = true;
                }
                else if(Data.Variables.MainForm.WindowState != FormWindowState.Minimized)
                {
                    Data.Variables.MainForm.NotifyIcon.Visible = false;
                }
                _alwaysShowTrayIcon = value;

                if (update)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Boolean _alwaysShowTrayIcon;

        [Category("UI"), DisplayName("Maximize on start"), TypeConverter(typeof(BooleanYesNoConverter))]
        public Boolean MaximizeOnStart
        {
            get => _maximizeOnStart;
            set
            {
                Boolean update = _maximizeOnStart != value;
                _maximizeOnStart = value;

                if (update)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Boolean _maximizeOnStart = true;

        [Category("UI"), DisplayName("Action to perform when closing"), TypeConverter(typeof(EnumDescriptionConverter<Enumerations.CloseAction>))]
        public Enumerations.CloseAction CloseAction
        {
            get => _closeAction;
            set
            {
                Boolean update = _closeAction != value;
                _closeAction = value;

                if (update)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Enumerations.CloseAction _closeAction = Enumerations.CloseAction.Close;

        [Category("Putty"), DisplayName("Putty executable"), Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
        public String PuttyExecutable
        {
            get => _puttyExecutable;
            set
            {
                Boolean update = !String.Equals(_puttyExecutable, value, StringComparison.Ordinal);
                _puttyExecutable = value;

                if (update)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private String _puttyExecutable;

        [Category("Putty"), DisplayName("Putty extra parameters"), Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
        public String PuttyExtraParamaters
        {
            get => _puttyExtraParamaters;
            set
            {
                Boolean update = !String.Equals(_puttyExtraParamaters, value, StringComparison.Ordinal);
                _puttyExtraParamaters = value;

                if (update)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private String _puttyExtraParamaters;

        [Category("Sessions"), DisplayName("Which sessions to automatically start"), TypeConverter(typeof(EnumDescriptionConverter<Enumerations.InitialSessions>))]
        public Enumerations.InitialSessions InitialSessions
        {
            get => _initialSessions;
            set
            {
                Boolean update = _initialSessions != value;
                _initialSessions = value;

                if (update)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Enumerations.InitialSessions _initialSessions = Enumerations.InitialSessions.Previous;

        [Browsable(false)]
        public Boolean ShowServerList
        {
            get => _showServerList;
            set
            {
                Boolean update = _showServerList != value;
                _showServerList = value;

                if (update)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Boolean _showServerList = true;

        [Category("Servers"), DisplayName("Available keywords"), TypeConverter(typeof(CsvConverter)), XmlArrayItem(ElementName = "Keyword")]
        public String[] Keywords
        {
            get => _keywords;
            set
            {
                Boolean update = !_keywords.Equals(value);
                _keywords = value;

                if (update)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private String[] _keywords = new String[0];

        [Category("Servers"), DisplayName("Available Variables"), TypeConverter(typeof(CsvConverter)), XmlArrayItem(ElementName = "Variable")]
        public String[] Variables
        {
            get => _variables;
            set
            {
                Boolean update = !_variables.Equals(value);
                _variables = value;

                if (update)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private String[] _variables = new String[0];

        [Category("Servers"), DisplayName("Restore checked servers on startup"), TypeConverter(typeof(BooleanYesNoConverter))]
        public Boolean SaveCheckedServers
        {
            get => _saveCheckedServers;
            set
            {
                Boolean update = _saveCheckedServers != value;
                _saveCheckedServers = value;

                if (update)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Boolean _saveCheckedServers = true;

        [Browsable(false)]
        public HashSet<String> CheckedNodes = new HashSet<String>(StringComparer.Ordinal);

        [Category("Nodes"), DisplayName("Node list")]
        public CollectionConverter<Node> Nodes
        {
            get
            {
                if (!_nodeList.AddedOrRemovedSet())
                {
                    _nodeList.AddedOrRemoved += Settings.Flush;
                    _nodeList.AddedOrRemoved += Settings.FireOnSettingsServerUpdatedEvent;
                }
                return _nodeList;
            }
            set => _nodeList = value;
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

        [Browsable(false)]
        public String NodeID { get; set; } = Generate.RandomUniqueID(10);

        [Category("Node"), DisplayName("Node name")]
        public String NodeName
        {
            get => _nodeName;
            set
            {
                _nodeName = value;

                if (_nodeName != value)
                {
                    Settings.FireOnSettingsServerUpdatedEvent();
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private String _nodeName = "/";

        [Category("Node"), DisplayName("Node list")]
        public CollectionConverter<Node> Nodes
        {
            get
            {
                if (!_nodeList.AddedOrRemovedSet())
                {
                    _nodeList.AddedOrRemoved += Settings.Flush;
                    _nodeList.AddedOrRemoved += Settings.FireOnSettingsServerUpdatedEvent;
                }
                return _nodeList;
            }
            set => _nodeList = value;
        }
        [XmlIgnore] private CollectionConverter<Node> _nodeList = new CollectionConverter<Node>();

        [Category("Servers"), DisplayName("Server list")]
        public CollectionConverter<Server> ServerList
        {
            get
            {
                if (!_serverList.AddedOrRemovedSet())
                {
                    _serverList.AddedOrRemoved += Settings.Flush;
                    _serverList.AddedOrRemoved += Settings.FireOnSettingsServerUpdatedEvent;
                }
                return _serverList;
            }
            set => _serverList = value;
        }
        [XmlIgnore] private CollectionConverter<Server> _serverList = new CollectionConverter<Server>();
    }
    [Serializable]
    public class Server
    {
        public override String ToString()
        {
            return String.IsNullOrEmpty(DisplayName) ? "?" : DisplayName;
        }

        [Browsable(false)]
        public String NodeID { get; set; } = Generate.RandomUniqueID(10);

        [Category("Basic"), DisplayName("Display name")]
        public String DisplayName
        {
            get => _displayName;
            set
            {
                Boolean update = false;
                if (!String.Equals(_displayName, value, StringComparison.Ordinal))
                {
                    Settings.FireOnSettingsServerUpdatedEvent();
                    update = true;
                }

                _displayName = value;

                if (update)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private String _displayName;

        [Category("Server"), DisplayName("Hostname")]
        public String Hostname
        {
            get => _hostName;
            set
            {
                Boolean update = !String.Equals(_hostName, value, StringComparison.Ordinal);
                _hostName = value;

                if (update)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private String _hostName;

        [Category("Server"), DisplayName("Username")]
        public String Username
        {
            get => _username;
            set
            {
                Boolean update = !String.Equals(_username, value, StringComparison.Ordinal);
                _username = value;

                if (update)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private String _username;

        [Category("Server"), DisplayName("Password"), Description("Warning: passwords ARE NOT ENCRYPTED")]
        public String Password
        {
            get => _password;
            set
            {
                Boolean update = !String.Equals(_password, value, StringComparison.Ordinal);

                if (!String.IsNullOrWhiteSpace(value))
                {
                    UI.ShowMessage(null, "Warning: passwords are NOT encrypted, please use other authentication methods!", "NO ENCRYPTION", MessageBoxIcon.Stop);
                }

                _password = value;

                if (update)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private String _password;

        [Category("Server"), DisplayName("Port number")]
        public UInt16 Port
        {
            get => _portNumber;
            set
            {
                Boolean update = _portNumber != value;
                _portNumber = value;

                if (update)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private UInt16 _portNumber = 22;

        [Category("Basic"), DisplayName("Keywords"), XmlArrayItem(ElementName = "Keyword"), Editor(typeof(CheckedListBoxUITypeEditor), typeof(UITypeEditor)), TypeConverter(typeof(CsvConverter)), Arguments(Enumerations.CheckedListBoxSetting.ServerKeywords)]
        public String[] Keywords
        {
            get => _keywords;
            set
            {
                Boolean update = !_keywords.Equals(value);
                _keywords = value;

                if (update)
                {
                    Settings.Flush();
                }
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
            set => _variables = value;
        }
        [XmlIgnore] private BasicPropertyBag _variables = new BasicPropertyBag();
    }
}