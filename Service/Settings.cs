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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Xml;
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
            if (File.Exists(Variables.PortableSettingsFile))
            {
                Variables.SettingsFileChosen = Variables.PortableSettingsFile;
                ReadSettings();
            }
            else if (File.Exists(Variables.NormalSettingsFile))
            {
                Variables.SettingsFileChosen = Variables.NormalSettingsFile;
                ReadSettings();
            }

            if (Variables.SettingsFileChosen == null)
            {
                Variables.SettingsFileChosen = UI.AskQuestion(Variables.MainForm, "Create portable settings file?", "No settings file found", MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button1, MessageBoxIcon.Question) == DialogResult.Yes ? Variables.PortableSettingsFile : Variables.NormalSettingsFile;
                FileOperations.CreateDirectory(Path.GetDirectoryName(Variables.SettingsFileChosen));
                FileOperations.CreateFile(Variables.SettingsFileChosen);
                Values = new Values();
                Flush();
            }
        }
        private static void ReadSettings()
        {
            _readingSettings = true;
            try
            {
                using XmlTextReader xmlReader = new(Variables.SettingsFileChosen);
                Values = (Values)XMLSerializer.Deserialize(xmlReader);
                UpdateNodesAndServers();
            }
            catch
            {
                Values = new Values();
            }
            _readingSettings = false;
        }

        public static Values Values;
        public static event ExtensionMethods.CustomDelegate OnSettingsUpdatedEvent;
        public static event ExtensionMethods.CustomDelegate OnSettingsServerUpdatedEvent;
        public static void FireOnSettingsServerUpdatedEvent()
        {
            OnSettingsServerUpdatedEvent?.Invoke();
        }

        public static readonly Dictionary<String, Node> AllNodes = new(StringComparer.Ordinal);
        public static Node FindNode(String nodeID)
        {
            return AllNodes.ContainsKey(nodeID) ? AllNodes[nodeID] : null;
        }
        private static void FindAllNodes(Node node)
        {
            foreach (Node nodeFound in node.NodeList)
            {
                AllNodes.Add(nodeFound.NodeID, nodeFound);
                FindAllNodes(nodeFound);
            }
        }
        public static readonly Dictionary<String, Server> AllServers = new(StringComparer.Ordinal);
        public static Server FindServer(String nodeID)
        {
            return AllServers.ContainsKey(nodeID) ? AllServers[nodeID] : null;
        }
        private static void FindAllServers(Node node)
        {
            foreach (Server server in node.ServerList)
            {
                AllServers.Add(server.NodeID, server);
            }

            foreach (Node nodeFound in node.NodeList)
            {
                FindAllServers(nodeFound);
            }
        }

        private static void UpdateNodesAndServers()
        {
            AllNodes.Clear();
            FindAllNodes(Values.Node);
            AllServers.Clear();
            FindAllServers(Values.Node);
        }

        private static Boolean _readingSettings;
        private static readonly XmlSerializer XMLSerializer = new(typeof(Values));
        internal static void Flush()
        {
            if (_readingSettings) return;

            lock (Values)
            {
                UpdateNodesAndServers();

                using StreamWriter writer = new(Variables.SettingsFileChosen);
                XMLSerializer.Serialize(writer, Values);
                writer.Flush();
            }

            OnSettingsUpdatedEvent?.Invoke();
        }
    }

    [Serializable, TypeConverter(typeof(ExpandableObjectConverter))]
    public class Values
    {
        [Category("Basic"), DisplayName("Periodically check for updates"), TypeConverter(typeof(BooleanYesNoConverter))]
        public Boolean CheckForUpdates
        {
            get => _checkForUpdates;
            set
            {
                Boolean update = _checkForUpdates != value;
                _checkForUpdates = value;

                if (Settings.Values != null)
                {
                    if (!value)
                    {
                        if (UpdateCheck.HasUpdateCheck())
                        {
                            UpdateCheck.StopUpdateCheck();
                        }
                    }
                    else
                    {
                        if (!UpdateCheck.HasUpdateCheck())
                        {
                            UpdateCheck.StartUpdateCronJob();
                        }
                    }
                }

                if (update && Settings.Values != null)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Boolean _checkForUpdates = true;

        [Category("Basic"), DisplayName("Theme to apply"), TypeConverter(typeof(EnumDescriptionConverter<Enumerations.Theme>))]
        public Enumerations.Theme Theme
        {
            get => _theme;
            set
            {
                Boolean update = _theme != value;
                _theme = value;

                if (update && Settings.Values != null)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Enumerations.Theme _theme = Enumerations.Theme.Black;

        [Category("UI"), DisplayName("Minimize to the tray instead of the taskbar"), TypeConverter(typeof(BooleanYesNoConverter))]
        public Boolean MinimizeToTray
        {
            get => _minimizeToTray;
            set
            {
                Boolean update = _minimizeToTray != value;
                _minimizeToTray = value;

                if (update && Settings.Values != null)
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
                _alwaysShowTrayIcon = value;

                if (Settings.Values != null)
                {
                    if (value)
                    {
                        Data.Variables.MainForm.NotifyIcon.Visible = true;
                    }
                    else if (Data.Variables.MainForm.WindowState != FormWindowState.Minimized)
                    {
                        Data.Variables.MainForm.NotifyIcon.Visible = false;
                    }

                }
                
                if (update && Settings.Values != null)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Boolean _alwaysShowTrayIcon;

        [Category("UI"), DisplayName("Send the command in the command box on enter"), TypeConverter(typeof(BooleanYesNoConverter))]
        public Boolean SendCommandOnEnter
        {
            get => _sendCommandOnEnter;
            set
            {
                Boolean update = _sendCommandOnEnter != value;
                _sendCommandOnEnter = value;

                if (update && Settings.Values != null)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Boolean _sendCommandOnEnter = true;

        [Category("UI"), DisplayName("Clear the command box after sending it"), TypeConverter(typeof(BooleanYesNoConverter))]
        public Boolean ClearCommandAfterSend
        {
            get => _clearCommandAfterSend;
            set
            {
                Boolean update = _clearCommandAfterSend != value;
                _clearCommandAfterSend = value;

                if (update && Settings.Values != null)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Boolean _clearCommandAfterSend;

        [Category("UI"), DisplayName("Action to perform when closing"), TypeConverter(typeof(EnumDescriptionConverter<Enumerations.CloseAction>))]
        public Enumerations.CloseAction CloseAction
        {
            get => _closeAction;
            set
            {
                Boolean update = _closeAction != value;
                _closeAction = value;

                if (update && Settings.Values != null)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Enumerations.CloseAction _closeAction = Enumerations.CloseAction.Close;

        [Category("UI"), DisplayName("Maximum number of lines to keep in the log window")]
        public UInt32 MaxVisibleLogLines
        {
            get => _maxVisibleLogLines;
            set
            {
                Boolean update = _maxVisibleLogLines != value;
                _maxVisibleLogLines = value;

                if (update && Settings.Values != null)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private UInt32 _maxVisibleLogLines = 2000;

        [Category("UI"), DisplayName("Initial window state"), TypeConverter(typeof(EnumDescriptionConverter<Enumerations.InitialWindowState>))]
        public Enumerations.InitialWindowState InitialWindowState
        {
            get => _initialWindowState;
            set
            {
                Boolean update = _initialWindowState != value;
                _initialWindowState = value;

                if (update && Settings.Values != null)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Enumerations.InitialWindowState _initialWindowState = Enumerations.InitialWindowState.MaximizedPreviousWindow;
        [Browsable(false)]
        public FormWindowState WindowState
        {
            get => _windowState;
            set
            {
                Boolean update = _windowState != value;
                _windowState = value;

                if (update && Settings.Values != null)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private FormWindowState _windowState = FormWindowState.Maximized;
        [Browsable(false)]
        public Point WindowLocation
        {
            get => _windowLocation;
            set
            {
                Boolean update = _windowLocation != value;
                _windowLocation = value;

                if (update && Settings.Values != null)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Point _windowLocation;
        [Browsable(false)]
        public Size WindowSize
        {
            get => _windowSize;
            set
            {
                Boolean update = _windowSize != value;
                _windowSize = value;

                if (update && Settings.Values != null)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Size _windowSize = new(500, 500);

        [Category("Putty"), DisplayName("Putty executable"), Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
        public String PuttyExecutable
        {
            get => _puttyExecutable;
            set
            {
                Boolean update = !String.Equals(_puttyExecutable, value, StringComparison.Ordinal);
                _puttyExecutable = value;

                if (update && Settings.Values != null)
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

                if (update && Settings.Values != null)
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

                if (update && Settings.Values != null)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Enumerations.InitialSessions _initialSessions = Enumerations.InitialSessions.Previous;

        [Category("Sessions"), DisplayName("Close session tab on crash"), TypeConverter(typeof(BooleanYesNoConverter))]
        public Boolean CloseTabOnCrash
        {
            get => _closeTabOnCrash;
            set
            {
                Boolean update = _closeTabOnCrash != value;
                _closeTabOnCrash = value;

                if (update && Settings.Values != null)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Boolean _closeTabOnCrash = true;

        [Category("Sessions"), DisplayName("Force close all sessions on application crash/closure"), TypeConverter(typeof(BooleanYesNoConverter))]
        public Boolean ForceCloseSessionsOnCrash
        {
            get => _forceCloseSessionsOnCrash;
            set
            {
                Boolean update = _forceCloseSessionsOnCrash != value;
                _forceCloseSessionsOnCrash = value;

                if (Settings.Values != null)
                {
                    if (value || Statics.InformationObjectManager == null)
                    {
                        Statics.InformationObjectManager = new InformationObjectManager();
                        Statics.InformationObjectManager.AddProcess(Process.GetCurrentProcess().SafeHandle);
                    }
                    else
                    {
                        Statics.InformationObjectManager?.Dispose();
                        Statics.InformationObjectManager = null;
                    }
                }

                if (update && Settings.Values != null)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Boolean _forceCloseSessionsOnCrash = true;

        [Category("Servers"), DisplayName("Available keywords"), TypeConverter(typeof(CsvConverter)), XmlArrayItem(ElementName = "Keyword")]
        public String[] Keywords
        {
            get => _keywords;
            set
            {
                Boolean update = !_keywords.Equals(value);
                _keywords = value;

                if (update && Settings.Values != null)
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

                if (update && Settings.Values != null)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private String[] _variables = new String[0];

        [Category("Servers"), DisplayName("Save checked nodes && servers"), TypeConverter(typeof(BooleanYesNoConverter))]
        public Boolean SaveCheckedNodes
        {
            get => _saveCheckedNodes;
            set
            {
                Boolean update = _saveCheckedNodes != value;
                _saveCheckedNodes = value;

                if (update && Settings.Values != null)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Boolean _saveCheckedNodes = true;

        [Category("Servers"), DisplayName("Save expanded nodes"), TypeConverter(typeof(BooleanYesNoConverter))]
        public Boolean SaveExpandedNodes
        {
            get => _saveExpandedNodes;
            set
            {
                Boolean update = _saveExpandedNodes != value;
                _saveExpandedNodes = value;

                if (update && Settings.Values != null)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Boolean _saveExpandedNodes = true;

        [Category("Servers"), DisplayName("Server && node list"), TypeConverter(typeof(ExpandableObjectConverter))]
        public Node Node
        {
            get => _node;
            set
            {
                _node = value;
                Settings.Flush();
            }
        }
        [XmlIgnore] private Node _node = new();
    }
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Node : ExpandableObjectConverter
    {
        public override String ToString() => String.IsNullOrEmpty(NodeName) ? "?" : NodeName;

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
        public CollectionConverter<Node> NodeList
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
        [XmlIgnore] private CollectionConverter<Node> _nodeList = new();

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
        [XmlIgnore] private CollectionConverter<Server> _serverList = new();

        [Browsable(false)]
        public Boolean Checked
        {
            get => _checked;
            set
            {
                Boolean update = _checked != value;
                _checked = value;

                if (update && Settings.Values != null)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Boolean _checked;

        [Browsable(false)]
        public Boolean Expanded
        {
            get => _expanded;
            set
            {
                Boolean update = _expanded != value;
                _expanded = value;

                if (update && Settings.Values != null)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Boolean _expanded;
    }
    [Serializable]
    public class Server
    {
        public override String ToString() => String.IsNullOrEmpty(DisplayName) ? "?" : DisplayName;

        [Browsable(false)]
        public String NodeID { get; set; } = Generate.RandomUniqueID(10);

        [Category("Basic"), DisplayName("Display name")]
        public String DisplayName
        {
            get => _displayName;
            set
            {
                Boolean update = false;
                if (!String.Equals(_displayName, value, StringComparison.Ordinal) && Settings.Values != null)
                {
                    Settings.FireOnSettingsServerUpdatedEvent();
                    update = true;
                }

                _displayName = value;

                if (update && Settings.Values != null)
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

                if (update && Settings.Values != null)
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

                if (update && Settings.Values != null)
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

                if (!String.IsNullOrWhiteSpace(value) && Settings.Values != null)
                {
                    UI.ShowWarning(Data.Variables.MainForm, "Warning: passwords are NOT encrypted, please use other authentication methods!", "NO ENCRYPTION", MessageBoxIcon.Stop);
                }

                _password = value;

                if (update && Settings.Values != null)
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

                if (update && Settings.Values != null)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private UInt16 _portNumber = 22;

        [Category("Server"), DisplayName("Newline to send after command")]
        public String NewLine
        {
            get => _newLine;
            set
            {
                Boolean update = _newLine != value;
                _newLine = value;

                if (update && Settings.Values != null)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private String _newLine = "\\n";

        [Category("Basic"), DisplayName("Keywords"), XmlArrayItem(ElementName = "Keyword"), Editor(typeof(CheckedListBoxUITypeEditor), typeof(UITypeEditor)), TypeConverter(typeof(CsvConverter)), Arguments(Enumerations.CheckedListBoxSetting.ServerKeywords)]
        public String[] Keywords
        {
            get => _keywords;
            set
            {
                Boolean update = value.Length != KeywordsHashed.Count || !value.All(x => KeywordsHashed.Contains(x));
                _keywords = value;

                if (update && Settings.Values != null)
                {
                    KeywordsHashed.Clear();
                    foreach (String keyword in _keywords)
                    {
                        KeywordsHashed.Add(keyword);
                    }
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private String[] _keywords = new String[0];
        [XmlIgnore] public HashSet<String> KeywordsHashed = new(StringComparer.Ordinal);

        [Browsable(false)]
        public Variable[] Variables
        {
            get => VariablesInternal.Properties;
            set => VariablesInternal.Properties = value;
        }

        [XmlIgnore, Category("Basic"), DisplayName("Variables"), Arguments(Enumerations.CheckedListBoxSetting.ServerVariables)]
        public BasicPropertyBag VariablesInternal { get; set; } = new();

        [Category("Basic"), DisplayName("Predefined start")]
        public Int32 PredefinedStartIndex
        {
            get => _predefinedStart;
            set
            {
                Boolean update = _predefinedStart != value;
                _predefinedStart = value;

                if (update && Settings.Values != null)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Int32 _predefinedStart = -1;

        [Browsable(false)]
        public Boolean Checked
        {
            get => _checked;
            set
            {
                Boolean update = _checked != value;
                _checked = value;

                if (update && Settings.Values != null)
                {
                    Settings.Flush();
                }
            }
        }
        [XmlIgnore] private Boolean _checked;
    }
}
