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
            Treeview_NodesAndServers.AfterExpand += TreeviewNodesAndServersAfterExpandOrCollapse;
            Treeview_NodesAndServers.AfterCollapse += TreeviewNodesAndServersAfterExpandOrCollapse;
            Treeview_NodesAndServers.MouseDoubleClick += TreeviewNodesAndServersMouseDoubleClick;
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
            
            foreach (KeyValuePair<String, TreeNode> treeNode in Treeview_NodesAndServers.TreeNodes)
            {
                if (!Service.Settings.AllNodes.ContainsKey(treeNode.Key) && !Service.Settings.AllServers.ContainsKey(treeNode.Key))
                {
                    treeNode.Value.Remove();
                }
            }

            Treeview_NodesAndServers.UpdateList();

            _firstLoad = false;
        }
        private void AddNode(CollectionConverter<Node> nodes, TreeNode parent = null)
        {
            foreach (Node nodeToUpdateOrAdd in nodes)
            {
                TreeNode nodeFound;
                if (Treeview_NodesAndServers.TreeNodes.ContainsKey(nodeToUpdateOrAdd.NodeID))
                {
                    nodeFound = Treeview_NodesAndServers.TreeNodes[nodeToUpdateOrAdd.NodeID];
                    nodeFound.Text = nodeToUpdateOrAdd.NodeName;
                }
                else
                {
                    nodeFound = parent == null ? Treeview_NodesAndServers.Nodes.Add(nodeToUpdateOrAdd.NodeID, nodeToUpdateOrAdd.NodeName, 0, 0) : parent.Nodes.Add(nodeToUpdateOrAdd.NodeID, nodeToUpdateOrAdd.NodeName, 0, 0);
                }
                if (_firstLoad && Service.Settings.Values.SaveCheckedNodes && nodeToUpdateOrAdd.Checked)
                {
                    nodeFound.Checked = true;
                }
                if (_firstLoad && Service.Settings.Values.SaveExpandedNodes && nodeToUpdateOrAdd.Expanded)
                {
                    nodeFound.Expand();
                }

                AddNode(nodeToUpdateOrAdd.NodeList, nodeFound);
                AddServers(nodeToUpdateOrAdd.ServerList, nodeFound);
            }
        }
        private void AddServers(CollectionConverter<Server> serverList, TreeNode node)
        {
            foreach (Server server in serverList)
            {
                TreeNode serverNodeFound;
                if (Treeview_NodesAndServers.TreeNodes.ContainsKey(server.NodeID))
                {
                    serverNodeFound = Treeview_NodesAndServers.TreeNodes[server.NodeID];
                    serverNodeFound.Text = server.DisplayName;
                }
                else
                {
                    serverNodeFound = node != null ? node.Nodes.Add(server.NodeID, server.DisplayName, 1, 1) : Treeview_NodesAndServers.Nodes.Add(server.NodeID, server.DisplayName, 1, 1);
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
        private static void TreeviewNodesAndServersAfterExpandOrCollapse(Object sender, TreeViewEventArgs e)
        {
            Node node = Service.Settings.FindNode(e.Node.Name);
            if (node != null)
            {
                node.Expanded = e.Node.IsExpanded;
            }
        }
        private void TreeviewNodesAndServersMouseDoubleClick(Object sender, MouseEventArgs mouseEventArgs)
        {
            if (Treeview_NodesAndServers.SelectedNode == null || Treeview_NodesAndServers.SelectedNode.ImageIndex != 1) return;

            Variables.MainForm.AddServer(Service.Settings.FindServer(Treeview_NodesAndServers.SelectedNode.Name));
        }
    }
}