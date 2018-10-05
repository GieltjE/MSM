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

            foreach (Node node in Service.Settings.Values.Nodes)
            {
                AddNode(node, null);
            }

retry:
            foreach (TreeNode treeNode in Treeview_NodesAndServers.Nodes)
            {
                if (DeleteNode(treeNode, Service.Settings.Values.Nodes.ToArray()))
                {
                    goto retry;
                }
            }

            _firstLoad = false;
        }

        private static Boolean DeleteNode(TreeNode node, Node[] nodes)
        {
            foreach (Node nodeToCheck in nodes)
            {
                if (String.Equals(node.Name, nodeToCheck.NodeID, StringComparison.Ordinal))
                {
                    foreach (TreeNode extraNode in node.Nodes)
                    {
                        if (extraNode.ImageIndex != 1)
                        {
                            DeleteNode(extraNode, nodeToCheck.Nodes.ToArray());
                        }
                    }

                    return false;
                }

                foreach (Server server in nodeToCheck.ServerList)
                {
                    if (String.Equals(node.Name, server.NodeID, StringComparison.Ordinal))
                    {
                        return false;
                    }
                }
            }

            node.Remove();
            return true;
        }
        private void AddNode(Node node, TreeNode parent)
        {
            TreeNode newParent;
            TreeNode nodeFound = FindTreeNode(node.NodeID, Treeview_NodesAndServers.Nodes);

            if (nodeFound != null)
            {
                nodeFound.Text = node.NodeName;
                newParent = nodeFound;
            }
            else
            {
                newParent = parent != null ? parent.Nodes.Add(node.NodeID, node.NodeName, 0, 0) : Treeview_NodesAndServers.Nodes.Add(node.NodeID, node.NodeName, 0, 0);
                if (_firstLoad && Service.Settings.Values.SaveCheckedServers && Service.Settings.Values.CheckedNodes.Contains(node.NodeID))
                {
                    newParent.Checked = true;
                }
            }

            foreach (Server server in node.ServerList)
            {
                TreeNode serverNodeFound = FindTreeNode(server.NodeID, newParent.Nodes, false);
                if (serverNodeFound != null)
                {
                    serverNodeFound.Text = server.DisplayName;
                }
                else
                {
                    TreeNode newNode = newParent.Nodes.Add(server.NodeID, server.DisplayName, 1, 1);
                    if (_firstLoad && Service.Settings.Values.SaveCheckedServers && Service.Settings.Values.CheckedNodes.Contains(node.NodeID))
                    {
                        newNode.Checked = true;
                    }
                }
            }

            foreach (Node nextNode in node.Nodes)
            {
                AddNode(nextNode, newParent);
            }
        }
        private static TreeNode FindTreeNode(String key, TreeNodeCollection collection, Boolean recursive = true)
        {
            foreach (TreeNode node in collection)
            {
                if (String.Equals(key, node.Name, StringComparison.Ordinal))
                {
                    return node;
                }

                if (node.Nodes.Count <= 0 || !recursive) continue;

                TreeNode foundNode = FindTreeNode(key, node.Nodes);
                if (foundNode != null)
                {
                    return foundNode;
                }
            }

            return null;
        }

        private static void TreeviewNodesAndServersAfterCheck(Object sender, TreeViewEventArgs e)
        {
            if (!e.Node.Checked)
            {
                if (!Service.Settings.Values.CheckedNodes.Contains(e.Node.Name)) return;

                Service.Settings.Values.CheckedNodes.Remove(e.Node.Name);
                Service.Settings.Flush();
            }
            else
            {
                if (Service.Settings.Values.CheckedNodes.Contains(e.Node.Name)) return;

                Service.Settings.Values.CheckedNodes.Add(e.Node.Name);
                Service.Settings.Flush();
            }
        }

        private void TreeviewNodesAndServersMouseDoubleClick(Object sender, MouseEventArgs mouseEventArgs)
        {
            if (Treeview_NodesAndServers.SelectedNode == null || Treeview_NodesAndServers.SelectedNode.ImageIndex != 1) return;

            Variables.MainForm.AddTerminal(Treeview_NodesAndServers.SelectedNode.Name);
        }
    }
}