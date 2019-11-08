using System;
using System.IO;

namespace LogMonitor.UserControls
{
    public partial class LogMonitorControl
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
            if (disposing)
            {
                if (components != null) components.Dispose();
                ClearMonitoringMethod();
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
            this._textBoxContents = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // _textBoxContents
            // 
            this._textBoxContents.Dock = System.Windows.Forms.DockStyle.Fill;
            this._textBoxContents.HideSelection = false;
            this._textBoxContents.Location = new System.Drawing.Point(0, 0);
            this._textBoxContents.Multiline = true;
            this._textBoxContents.Name = "_textBoxContents";
            this._textBoxContents.ReadOnly = true;
            this._textBoxContents.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this._textBoxContents.Size = new System.Drawing.Size(419, 333);
            this._textBoxContents.TabIndex = 12;
            this._textBoxContents.WordWrap = false;
            // 
            // LogMonitorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._textBoxContents);
            this.Name = "LogMonitorControl";
            this.Size = new System.Drawing.Size(419, 333);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox _textBoxContents;
    }
}
