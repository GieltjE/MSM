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
using MSM.Extends;
using MSM.Service;

namespace MSM.UIElements
{
    public partial class Servers : UserControlOptimized
    {
        public Servers()
        {
            InitializeComponent();

            LoadOrUpdateTree();
            Service.Settings.OnSettingsUpdatedEvent += LoadOrUpdateTree;
        }

        private delegate void LoadOrUpdateTreeCallback();
        private void LoadOrUpdateTree()
        {
            if(Treeview_NodesAndServers.InvokeRequired)
            {
                Invoke(new LoadOrUpdateTreeCallback(LoadOrUpdateTree));
            }
            else
            {
                Treeview_NodesAndServers.Nodes.Clear();

                foreach (Node node in Service.Settings.Values.Nodes)
                {
                    String[] splitted = node.NodeName.TrimStart('/').Split('/');
                    TreeNode lastNode = null;
                    foreach (String part in splitted)
                    {
                        String toFind = "/" + part;

                        if (lastNode == null)
                        {
                            foreach (TreeNode nodeFound in Treeview_NodesAndServers.Nodes)
                            {
                                if (!String.Equals(nodeFound.Text, toFind, StringComparison.Ordinal)) continue;

                                lastNode = nodeFound;
                                break;
                            }

                            if (lastNode == null)
                            {
                                lastNode = Treeview_NodesAndServers.Nodes.Add(toFind);
                            }
                        }
                        else
                        {
                            Boolean found = false;
                            foreach (TreeNode nodeFound in lastNode.Nodes)
                            {
                                if (!String.Equals(nodeFound.Text, toFind, StringComparison.Ordinal)) continue;

                                found = true;
                                lastNode = nodeFound;
                                break;
                            }

                            if (!found)
                            {
                                lastNode = lastNode.Nodes.Add(toFind);
                            }
                        }
                    }

                    foreach (Server server in node.ServerList)
                    {
                        lastNode.Nodes.Add(server.DisplayName);
                    }
                }
            }
        }

        private void TreeviewNodesAndServersMouseDoubleClick(Object sender, MouseEventArgs e)
        {
        }
    }
}