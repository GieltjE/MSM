namespace MSM.UIElements
{
    partial class SettingControl
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
            this.PropertyGrid_Settings = new System.Windows.Forms.PropertyGrid();
            this.SuspendLayout();
            // 
            // PropertyGrid_Settings
            // 
            this.PropertyGrid_Settings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PropertyGrid_Settings.HelpVisible = false;
            this.PropertyGrid_Settings.Location = new System.Drawing.Point(0, 0);
            this.PropertyGrid_Settings.Name = "PropertyGrid_Settings";
            this.PropertyGrid_Settings.Size = new System.Drawing.Size(200, 200);
            this.PropertyGrid_Settings.TabIndex = 0;
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.Controls.Add(this.PropertyGrid_Settings);
            this.Name = "Settings";
            this.Size = new System.Drawing.Size(200, 200);
            this.ResumeLayout(false);
        }
        #endregion

        public System.Windows.Forms.PropertyGrid PropertyGrid_Settings;
    }
}
