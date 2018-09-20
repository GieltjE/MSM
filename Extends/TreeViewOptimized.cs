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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MSM.Data;

namespace MSM.Extends
{
    [DefaultProperty("CustomSmarterCheckboxHandling"), ToolboxBitmap(typeof(TreeView))]
    public class TreeViewOptimized : TreeView
    {
        public TreeViewOptimized()
        {
            // userpaint poses some problems
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor, true);
            BorderStyle = BorderStyle.FixedSingle;
            Sorted = true;

            if (!DesignMode && Variables.ColorPalette != null)
            {
                // ReSharper disable RedundantBaseQualifier
                base.BackColor = Variables.ColorPalette.ToolWindowCaptionActive.Background;
                base.ForeColor = Variables.ColorPalette.ToolWindowCaptionActive.Text;
                // ReSharper restore RedundantBaseQualifier
            }
        }

        [DefaultValue(BorderStyle.FixedSingle)]
        public new BorderStyle BorderStyle { get => base.BorderStyle; set => base.BorderStyle = value; }

        [DefaultValue(true), Description("Wether to only allow parent selection when all childs are checked")]
        public Boolean CustomOnlyAllowParentWhenAllChildsChecked { get; set; } = true;

        [DefaultValue(true), Description("Wether to force parent selection when all childs are checked")]
        public Boolean CustomForceParentSelectionOnAllChildSelection { get; set; } = true;

        [DefaultValue(true), Description("Wether to do smarter checkbox handling")]
        public Boolean CustomSmarterCheckboxHandling { get; set; } = true;

        private List<TreeNode> _treeNodeList = new List<TreeNode>();
        private void LoadTreeNode(IEnumerable nodes)
        {
            foreach (TreeNode node in nodes)
            {
                _treeNodeList.Add(node);
                LoadTreeNode(node.Nodes);
            }
        }

        private Boolean _onAfterCheckBusy;
        protected override void OnAfterCheck(TreeViewEventArgs e)
        {
            base.OnAfterCheck(e);

            if (!CustomSmarterCheckboxHandling) return;
            if (_onAfterCheckBusy) return;
            _onAfterCheckBusy = true;

            CheckAllNodes(e.Node.Nodes, e.Node.Checked);

            if (CustomForceParentSelectionOnAllChildSelection && e.Node.Parent != null)
            {
                Boolean allChecked = true;
                Boolean noneChecked = true;
                foreach (TreeNode node in e.Node.Parent.Nodes)
                {
                    if (!node.Checked)
                    {
                        allChecked = false;
                    }
                    else
                    {
                        noneChecked = false;
                    }
                }
                // ReSharper disable once RedundantCheckBeforeAssignment
                if (!e.Node.Parent.Checked && allChecked)
                {
                    e.Node.Parent.Checked = true;
                }
                if (e.Node.Parent.Checked && noneChecked)
                {
                    e.Node.Parent.Checked = false;
                }
            }

            if (CustomOnlyAllowParentWhenAllChildsChecked)
            {
                UnCheckAllParentsIfNotAllChildsChecked(e.Node);
            }

            _onAfterCheckBusy = false;
        }
        private static void CheckAllNodes(ICollection nodes, Boolean check)
        {
            if (nodes.Count == 0) return;
            foreach (TreeNode node in nodes)
            {
                node.Checked = check;
                CheckAllNodes(node.Nodes, check);
            }
        }
        private static Boolean AllNodesChecked(IEnumerable nodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (!node.Checked)
                {
                    return false;
                }
                if (node.Nodes.Count <= 0) continue;
                if (!AllNodesChecked(node.Nodes))
                {
                    return false;
                }
            }

            return true;
        }
        private static void UnCheckAllParentsIfNotAllChildsChecked(TreeNode node)
        {
            if (!AllNodesChecked(node.Nodes))
            {
                node.Checked = false;
            }
            if (node.Parent != null)
            {
                UnCheckAllParentsIfNotAllChildsChecked(node.Parent);
            }
        }

        public void CheckItems(List<String> itemsToCheck, Boolean toLower)
        {
            _treeNodeList = new List<TreeNode>();
            LoadTreeNode(Nodes);

            foreach (TreeNode treeNode in from treeNode in _treeNodeList let searchItem = toLower ? treeNode.Text.ToLowerInvariant() : treeNode.Text where itemsToCheck.Contains(searchItem) select treeNode)
            {
                treeNode.Checked = true;
                treeNode.Expand();

                TreeNode treeNode1 = treeNode.Parent;
                while (treeNode1 != null)
                {
                    treeNode1.Expand();
                    treeNode1 = treeNode.Parent;
                }
            }
        }
        public IEnumerable<String> GetCheckedItems()
        {
            _treeNodeList = new List<TreeNode>();
            LoadTreeNode(Nodes);

            return (from treeNode in _treeNodeList where treeNode.Checked select treeNode.Text).ToList();
        }

        public TreeNode FindTreeNode(String value, TreeNode treeNode, Boolean recursive)
        {
            if (treeNode.Text == value)
            {
                return treeNode;
            }

            if (treeNode.Nodes.Count == 0 || !recursive) return new TreeNode();
            for (Int32 i = 0; i < treeNode.Nodes.Count; i++)
            {
                TreeNode guess2 = FindTreeNode(value, treeNode.Nodes[i], true);
                if (guess2.GetNodeCount(true) != 0)
                {
                    return guess2;
                }
            }

            return new TreeNode();
        }
    }
}