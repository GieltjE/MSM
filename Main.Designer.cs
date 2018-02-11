using MSM.Extends;
using WeifenLuo.WinFormsUI.Docking;

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
            this.ToolStrip = new MSM.Extends.ToolstripOptimized();
            this.ToolStripButton_Help = new MSM.Extends.ToolStripDropDownButtonOptimized();
            this.ToolStripMenuItem_About = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButtonOptimized1 = new MSM.Extends.ToolStripDropDownButtonOptimized();
            this.ToolStripMenuItem_Light = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_Blue = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_Dark = new System.Windows.Forms.ToolStripMenuItem();
            this.StatusStrip = new MSM.Extends.StatusStripOptimized();
            this.DockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();
            this.ToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // ToolStrip
            // 
            this.ToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.ToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripButton_Help,
            this.toolStripDropDownButtonOptimized1});
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
            // toolStripDropDownButtonOptimized1
            // 
            this.toolStripDropDownButtonOptimized1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButtonOptimized1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_Light,
            this.ToolStripMenuItem_Blue,
            this.ToolStripMenuItem_Dark});
            this.toolStripDropDownButtonOptimized1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButtonOptimized1.Name = "toolStripDropDownButtonOptimized1";
            this.toolStripDropDownButtonOptimized1.Size = new System.Drawing.Size(45, 22);
            this.toolStripDropDownButtonOptimized1.Text = "View";
            // 
            // ToolStripMenuItem_Light
            // 
            this.ToolStripMenuItem_Light.Name = "ToolStripMenuItem_Light";
            this.ToolStripMenuItem_Light.Size = new System.Drawing.Size(152, 22);
            this.ToolStripMenuItem_Light.Text = "Light";
            this.ToolStripMenuItem_Light.Click += new System.EventHandler(this.SetTheme);
            // 
            // ToolStripMenuItem_Blue
            // 
            this.ToolStripMenuItem_Blue.Name = "ToolStripMenuItem_Blue";
            this.ToolStripMenuItem_Blue.Size = new System.Drawing.Size(152, 22);
            this.ToolStripMenuItem_Blue.Text = "Blue";
            this.ToolStripMenuItem_Blue.Click += new System.EventHandler(this.SetTheme);
            // 
            // ToolStripMenuItem_Dark
            // 
            this.ToolStripMenuItem_Dark.Name = "ToolStripMenuItem_Dark";
            this.ToolStripMenuItem_Dark.Size = new System.Drawing.Size(152, 22);
            this.ToolStripMenuItem_Dark.Text = "Dark";
            this.ToolStripMenuItem_Dark.Click += new System.EventHandler(this.SetTheme);
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
            // DockPanel
            // 
            this.DockPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DockPanel.Location = new System.Drawing.Point(0, 25);
            this.DockPanel.Margin = new System.Windows.Forms.Padding(0);
            this.DockPanel.Name = "DockPanel";
            this.DockPanel.Size = new System.Drawing.Size(882, 551);
            this.DockPanel.TabIndex = 4;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(882, 598);
            this.Controls.Add(this.DockPanel);
            this.Controls.Add(this.StatusStrip);
            this.Controls.Add(this.ToolStrip);
            this.Name = "Main";
            this.Text = "Multi Server Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFormClosing);
            this.ToolStrip.ResumeLayout(false);
            this.ToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private ToolstripOptimized ToolStrip;
        private StatusStripOptimized StatusStrip;
        private ToolStripDropDownButtonOptimized ToolStripButton_Help;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_About;
        private DockPanel DockPanel;
        private ToolStripDropDownButtonOptimized toolStripDropDownButtonOptimized1;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_Light;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_Blue;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_Dark;
    }
}