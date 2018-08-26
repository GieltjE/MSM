namespace MSM.UIElements
{
    partial class Servers
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

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Treeview_NodesAndServers = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // Treeview_NodesAndServers
            // 
            this.Treeview_NodesAndServers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Treeview_NodesAndServers.Location = new System.Drawing.Point(0, 0);
            this.Treeview_NodesAndServers.Name = "Treeview_NodesAndServers";
            this.Treeview_NodesAndServers.PathSeparator = "/";
            this.Treeview_NodesAndServers.Size = new System.Drawing.Size(200, 200);
            this.Treeview_NodesAndServers.TabIndex = 0;
            // 
            // Servers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.Controls.Add(this.Treeview_NodesAndServers);
            this.Name = "Servers";
            this.Size = new System.Drawing.Size(200, 200);
            this.ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.TreeView Treeview_NodesAndServers;
    }
}