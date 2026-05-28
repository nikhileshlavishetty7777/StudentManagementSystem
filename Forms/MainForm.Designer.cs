#nullable enable
using System.Windows.Forms;

namespace StudentManagementSystem.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer? components = null;
        private Panel? contentPanel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ResumeLayout(false);
        }
    }
}

