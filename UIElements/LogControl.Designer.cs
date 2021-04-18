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
            this.DataGridView_Logs = new MSM.Extends.DataGridViewOptimized();
            this.toolstripOptimized1 = new MSM.Extends.ToolstripOptimized();
            this.ToolStrip_Filter = new System.Windows.Forms.ToolStripTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_Logs)).BeginInit();
            this.toolstripOptimized1.SuspendLayout();
            this.SuspendLayout();
            // 
            // DataGridView_Logs
            // 
            this.DataGridView_Logs.AllowUserToAddRows = false;
            this.DataGridView_Logs.AllowUserToDeleteRows = false;
            this.DataGridView_Logs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.DataGridView_Logs.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.RaisedVertical;
            this.DataGridView_Logs.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.DataGridView_Logs.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.DataGridView_Logs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView_Logs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DataGridView_Logs.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.DataGridView_Logs.Location = new System.Drawing.Point(0, 25);
            this.DataGridView_Logs.Name = "DataGridView_Logs";
            this.DataGridView_Logs.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.DataGridView_Logs.RowHeadersVisible = false;
            this.DataGridView_Logs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.DataGridView_Logs.Size = new System.Drawing.Size(902, 271);
            this.DataGridView_Logs.TabIndex = 0;
            // 
            // toolstripOptimized1
            // 
            this.toolstripOptimized1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolstripOptimized1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStrip_Filter});
            this.toolstripOptimized1.Location = new System.Drawing.Point(0, 0);
            this.toolstripOptimized1.Name = "toolstripOptimized1";
            this.toolstripOptimized1.Padding = new System.Windows.Forms.Padding(0);
            this.toolstripOptimized1.ShowItemToolTips = false;
            this.toolstripOptimized1.Size = new System.Drawing.Size(902, 25);
            this.toolstripOptimized1.Stretch = true;
            this.toolstripOptimized1.TabIndex = 1;
            // 
            // ToolStrip_Filter
            // 
            this.ToolStrip_Filter.Name = "ToolStrip_Filter";
            this.ToolStrip_Filter.Size = new System.Drawing.Size(200, 25);
            this.ToolStrip_Filter.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ToolStripFilterKeyUp);
            // 
            // LogControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.Controls.Add(this.DataGridView_Logs);
            this.Controls.Add(this.toolstripOptimized1);
            this.Name = "LogControl";
            this.Size = new System.Drawing.Size(902, 296);
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_Logs)).EndInit();
            this.toolstripOptimized1.ResumeLayout(false);
            this.toolstripOptimized1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private DataGridViewOptimized DataGridView_Logs;
        private ToolstripOptimized toolstripOptimized1;
        private ToolStripTextBox ToolStrip_Filter;
    }
}
