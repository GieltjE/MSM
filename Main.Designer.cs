using MSM.Extends;
using MSM.Extends.Themes;

namespace MSM
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.DockPanel_Main = new MSM.Extends.DockPanelOptimized();
            this.ToolStrip = new MSM.Extends.ToolstripOptimized();
            this.ToolStripButton_Help = new MSM.Extends.ToolStripDropDownButtonOptimized();
            this.ToolStripMenuItem_About = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStrip_General = new MSM.Extends.ToolStripDropDownButtonOptimized();
            this.ToolStrip_Settings = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStrip_ShowServerList = new System.Windows.Forms.ToolStripMenuItem();
            this.StatusStrip = new MSM.Extends.StatusStripOptimized();
            this.ToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // DockPanel_Main
            // 
            this.DockPanel_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DockPanel_Main.DockBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.DockPanel_Main.Location = new System.Drawing.Point(0, 25);
            this.DockPanel_Main.Margin = new System.Windows.Forms.Padding(0);
            this.DockPanel_Main.Name = "DockPanel_Main";
            this.DockPanel_Main.ShowAutoHideContentOnHover = false;
            this.DockPanel_Main.Size = new System.Drawing.Size(882, 551);
            this.DockPanel_Main.TabIndex = 4;
            this.DockPanel_Main.ContentRemoved += new System.EventHandler<WeifenLuo.WinFormsUI.Docking.DockContentEventArgs>(this.DockPanelMainContentRemoved);
            // 
            // ToolStrip
            // 
            this.ToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.ToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripButton_Help,
            this.ToolStrip_General});
            this.ToolStrip.Location = new System.Drawing.Point(0, 0);
            this.ToolStrip.Name = "ToolStrip";
            this.ToolStrip.Padding = new System.Windows.Forms.Padding(0);
            this.ToolStrip.ShowItemToolTips = false;
            this.ToolStrip.Size = new System.Drawing.Size(882, 25);
            this.ToolStrip.Stretch = true;
            this.ToolStrip.TabIndex = 0;
            // 
            // ToolStripButton_Help
            // 
            this.ToolStripButton_Help.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.ToolStripButton_Help.CustomDropDownDirection = System.Windows.Forms.ToolStripDropDownDirection.BelowLeft;
            this.ToolStripButton_Help.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.ToolStripButton_Help.DropDownDirection = System.Windows.Forms.ToolStripDropDownDirection.BelowLeft;
            this.ToolStripButton_Help.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_About});
            this.ToolStripButton_Help.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolStripButton_Help.Name = "ToolStripButton_Help";
            this.ToolStripButton_Help.Size = new System.Drawing.Size(45, 22);
            this.ToolStripButton_Help.Text = "Help";
            // 
            // ToolStripMenuItem_About
            // 
            this.ToolStripMenuItem_About.Name = "ToolStripMenuItem_About";
            this.ToolStripMenuItem_About.Size = new System.Drawing.Size(107, 22);
            this.ToolStripMenuItem_About.Text = "About";
            this.ToolStripMenuItem_About.Click += new System.EventHandler(this.ToolStripMenuItemAboutClick);
            // 
            // ToolStrip_General
            // 
            this.ToolStrip_General.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.ToolStrip_General.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStrip_Settings,
            this.ToolStrip_ShowServerList});
            this.ToolStrip_General.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolStrip_General.Name = "ToolStrip_General";
            this.ToolStrip_General.Size = new System.Drawing.Size(60, 22);
            this.ToolStrip_General.Text = "General";
            // 
            // ToolStrip_Settings
            // 
            this.ToolStrip_Settings.Name = "ToolStrip_Settings";
            this.ToolStrip_Settings.Size = new System.Drawing.Size(148, 22);
            this.ToolStrip_Settings.Text = "Settings";
            this.ToolStrip_Settings.Click += new System.EventHandler(this.ToolStripSettingsClick);
            // 
            // ToolStrip_ShowServerList
            // 
            this.ToolStrip_ShowServerList.CheckOnClick = true;
            this.ToolStrip_ShowServerList.Name = "ToolStrip_ShowServerList";
            this.ToolStrip_ShowServerList.Size = new System.Drawing.Size(148, 22);
            this.ToolStrip_ShowServerList.Text = "View serverlist";
            this.ToolStrip_ShowServerList.Click += new System.EventHandler(this.ToolStripShowServerListClick);
            // 
            // StatusStrip
            // 
            this.StatusStrip.AllowMerge = false;
            this.StatusStrip.AutoSize = false;
            this.StatusStrip.GripMargin = new System.Windows.Forms.Padding(0);
            this.StatusStrip.ImageScalingSize = new System.Drawing.Size(0, 0);
            this.StatusStrip.Location = new System.Drawing.Point(0, 576);
            this.StatusStrip.Name = "StatusStrip";
            this.StatusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.StatusStrip.Size = new System.Drawing.Size(882, 22);
            this.StatusStrip.SizingGrip = false;
            this.StatusStrip.TabIndex = 3;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(882, 598);
            this.Controls.Add(this.DockPanel_Main);
            this.Controls.Add(this.StatusStrip);
            this.Controls.Add(this.ToolStrip);
            this.Name = "Main";
            this.Text = "Multi Server Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainClosing);
            this.Shown += new System.EventHandler(this.MainShown);
            this.Resize += new System.EventHandler(this.MainResize);
            this.ToolStrip.ResumeLayout(false);
            this.ToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        public ToolstripOptimized ToolStrip;
        private StatusStripOptimized StatusStrip;
        private ToolStripDropDownButtonOptimized ToolStripButton_Help;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_About;
        internal DockPanelOptimized DockPanel_Main;
        private ToolStripDropDownButtonOptimized ToolStrip_General;
        private System.Windows.Forms.ToolStripMenuItem ToolStrip_Settings;
        private System.Windows.Forms.ToolStripMenuItem ToolStrip_ShowServerList;
    }
}