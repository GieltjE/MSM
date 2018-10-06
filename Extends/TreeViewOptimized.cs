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

        [DefaultValue(-1), Description("Only show checkbox for specified ImageIndex")]
        public Int32 CustomOnlyShowCheckboxForImageIndex
        {
            get => _customOnlyShowCheckboxForImageIndex;
            set
            {
                DrawMode = value != -1 ? TreeViewDrawMode.OwnerDrawText : TreeViewDrawMode.Normal;
                _customOnlyShowCheckboxForImageIndex = value;
            }
        }
        private Int32 _customOnlyShowCheckboxForImageIndex;

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
                CheckParentNodeIfAllNodesAreChecked(e.Node.Parent);
            }

            if (CustomOnlyAllowParentWhenAllChildsChecked)
            {
                UnCheckAllParentsIfNotAllChildsChecked(e.Node);
            }

            if (e.Node.Checked)
            {
                if (!CheckedItems.Contains(e.Node.Name))
                {
                    CheckedItems.Add(e.Node.Name);
                }
            }
            else
            {
                if (CheckedItems.Contains(e.Node.Name))
                {
                    CheckedItems.Remove(e.Node.Name);
                }
            }

            _onAfterCheckBusy = false;
        }
        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            if (e.Node.ImageIndex != CustomOnlyShowCheckboxForImageIndex)
            {
                HideCheckBox(e.Node);
            }
            e.DrawDefault = true;
            base.OnDrawNode(e);
        }

        public static void HideCheckBox(TreeNode node)
        {
            NativeMethods.TVITEM tvi = new NativeMethods.TVITEM
            {
                hItem = node.Handle,
                mask = 0x8,
                stateMask = 0xF000,
                state = 0
            };
            NativeMethods.SendMessage(node.TreeView.Handle, 0x1100 + 63, IntPtr.Zero, ref tvi);
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

        private static void CheckParentNodeIfAllNodesAreChecked(TreeNode node)
        {
            Boolean allChecked = true;
            Boolean noneChecked = true;

            foreach (TreeNode parentChildNode in node.Nodes)
            {
                if (!parentChildNode.Checked)
                {
                    allChecked = false;
                }
                else
                {
                    noneChecked = false;
                }
            }

            if (allChecked && !node.Checked)
            {
                node.Checked = true;
            }

            // ReSharper disable once RedundantCheckBeforeAssignment
            if (node.Parent != null && !node.Parent.Checked && allChecked)
            {
                node.Checked = true;
                if (node.Parent != null)
                {
                    CheckParentNodeIfAllNodesAreChecked(node.Parent);
                }
            }
            if (node.Checked && noneChecked)
            {
                node.Checked = false;
            }
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

        public HashSet<String> CheckedItems = new HashSet<String>(StringComparer.Ordinal);
        public Dictionary<String, TreeNode> TreeNodes = new Dictionary<String, TreeNode>();
        public void UpdateList()
        {
            TreeNodes.Clear();

            LoadTreeNode(Nodes);
        }
        private void LoadTreeNode(IEnumerable nodes)
        {
            foreach (TreeNode node in nodes)
            {
                TreeNodes.Add(node.Name, node);
                LoadTreeNode(node.Nodes);
            }
        }
    }
}