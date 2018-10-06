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
using System.Windows.Forms;
using MSM.Data;
using MSM.Extends;
using MSM.Graphics;
using MSM.Service;

namespace MSM.UIElements
{
    public partial class Servers : UserControlOptimized
    {
        private static ImageList _imageList;
        public Servers()
        {
            InitializeComponent();

            LoadOrUpdateTree();
            Service.Settings.OnSettingsServerUpdatedEvent += LoadOrUpdateTree;

            if (_imageList == null)
            {
                _imageList = new ImageList();
                _imageList.Images.Add(Resources.Folder);
                _imageList.Images.Add(Resources.Session);
            }
            Treeview_NodesAndServers.ImageList = _imageList;
            Treeview_NodesAndServers.AfterCheck += TreeviewNodesAndServersAfterCheck;
        }

        private Boolean _firstLoad = true;
        private void LoadOrUpdateTree()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(LoadOrUpdateTree));
                return;
            }

            AddNode(Service.Settings.Values.Node.NodeList);
            AddServers(Service.Settings.Values.Node.ServerList, null);

            List<TreeNode> nodes = Treeview_NodesAndServers.GetAllItems();
            HashSet<String> available = Service.Settings.FindAllNodeIDs();
            foreach (TreeNode treeNode in nodes)
            {
                if (!available.Contains(treeNode.Name))
                {
                    treeNode.Remove();
                }
            }
            
            _firstLoad = false;
        }

        private void AddNode(CollectionConverter<Node> nodes, TreeNode parent = null)
        {
            foreach (Node nodeToUpdateOrAdd in nodes)
            {
                TreeNode nodeFound = null;

                if (parent == null)
                {
                    foreach (TreeNode existingNode in Treeview_NodesAndServers.Nodes)
                    {
                        if (!String.Equals(existingNode.Name, nodeToUpdateOrAdd.NodeID, StringComparison.Ordinal)) continue;

                        nodeFound = existingNode;
                        nodeFound.Text = nodeToUpdateOrAdd.NodeName;
                        break;
                    }
                    if (nodeFound == null)
                    {
                        nodeFound = Treeview_NodesAndServers.Nodes.Add(nodeToUpdateOrAdd.NodeID, nodeToUpdateOrAdd.NodeName, 0, 0);
                    }
                }
                else
                {
                    foreach (TreeNode existingNode in parent.Nodes)
                    {
                        if (!String.Equals(existingNode.Name, nodeToUpdateOrAdd.NodeID, StringComparison.Ordinal)) continue;

                        nodeFound = existingNode;
                        nodeFound.Text = nodeToUpdateOrAdd.NodeName;
                        break;
                    }
                    if (nodeFound == null)
                    {
                        nodeFound = parent.Nodes.Add(nodeToUpdateOrAdd.NodeID, nodeToUpdateOrAdd.NodeName, 0, 0);
                    }
                }

                AddNode(nodeToUpdateOrAdd.NodeList, nodeFound);
                AddServers(nodeToUpdateOrAdd.ServerList, nodeFound);
            }
        }
        private void AddServers(CollectionConverter<Server> serverList, TreeNode node)
        {
            foreach (Server server in serverList)
            {
                TreeNode serverNodeFound = null;
                if (node != null)
                {
                    foreach (TreeNode newNode in node.Nodes)
                    {
                        if (!String.Equals(newNode.Name, server.NodeID, StringComparison.Ordinal)) continue;

                        serverNodeFound = newNode;
                        serverNodeFound.Text = server.DisplayName;
                        break;
                    }
                    if (serverNodeFound == null)
                    {
                        serverNodeFound = node.Nodes.Add(server.NodeID, server.DisplayName, 1, 1);
                    }
                }
                else
                {
                    foreach (TreeNode newNode in Treeview_NodesAndServers.Nodes)
                    {
                        if (!String.Equals(newNode.Name, server.NodeID, StringComparison.Ordinal)) continue;

                        serverNodeFound = newNode;
                        serverNodeFound.Text = server.DisplayName;
                        break;
                    }
                    if (serverNodeFound == null)
                    {
                        serverNodeFound = Treeview_NodesAndServers.Nodes.Add(server.NodeID, server.DisplayName, 1, 1);
                    }
                }

                if (_firstLoad && Service.Settings.Values.SaveCheckedNodes && server.Checked)
                {
                    serverNodeFound.Checked = true;
                }
            }
        }

        private static void TreeviewNodesAndServersAfterCheck(Object sender, TreeViewEventArgs e)
        {
            Server server = Service.Settings.FindServer(e.Node.Name);
            if (server != null)
            {
                server.Checked = e.Node.Checked;
            }

            Node node = Service.Settings.FindNode(e.Node.Name);
            if (node != null)
            {
                node.Checked = e.Node.Checked;
            }
        }

        private void TreeviewNodesAndServersMouseDoubleClick(Object sender, MouseEventArgs mouseEventArgs)
        {
            if (Treeview_NodesAndServers.SelectedNode == null || Treeview_NodesAndServers.SelectedNode.ImageIndex != 1) return;

            Variables.MainForm.AddTerminal(Treeview_NodesAndServers.SelectedNode.Name);
        }
    }
}