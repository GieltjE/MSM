using System.Windows.Forms;
using MSM.Extends;

namespace MSM.UIElements
{
    partial class LogControl
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
            this.DataGridView_Logs = new DataGridViewOptimized();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_Logs)).BeginInit();
            this.SuspendLayout();
            // 
            // DataGridView_Logs
            // 
            this.DataGridView_Logs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView_Logs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DataGridView_Logs.Location = new System.Drawing.Point(0, 0);
            this.DataGridView_Logs.Name = "DataGridView_Logs";
            this.DataGridView_Logs.Size = new System.Drawing.Size(200, 200);
            this.DataGridView_Logs.TabIndex = 0;
            this.DataGridView_Logs.EditMode = DataGridViewEditMode.EditProgrammatically;
            this.DataGridView_Logs.AllowUserToAddRows = false;
            // 
            // Logs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.Controls.Add(this.DataGridView_Logs);
            this.Name = "Logs";
            this.Size = new System.Drawing.Size(200, 200);
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_Logs)).EndInit();
            this.ResumeLayout(false);
        }
        #endregion

        private DataGridViewOptimized DataGridView_Logs;
    }
}
