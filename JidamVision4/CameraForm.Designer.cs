namespace JidamVision4
{
    partial class CameraForm
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
            this.imageViewCtrl1 = new JidamVision4.UIControl.ImageViewCtrl();
            this.imageViewer = new JidamVision4.UIControl.ImageViewCtrl();
            this.SuspendLayout();
            // 
            // imageViewCtrl1
            // 
            this.imageViewCtrl1.Location = new System.Drawing.Point(0, 0);
            this.imageViewCtrl1.Name = "imageViewCtrl1";
            this.imageViewCtrl1.Size = new System.Drawing.Size(150, 150);
            this.imageViewCtrl1.TabIndex = 0;
            // 
            // imageViewer
            // 
            this.imageViewer.Dock = System.Windows.Forms.DockStyle.Left;
            this.imageViewer.Location = new System.Drawing.Point(0, 0);
            this.imageViewer.Name = "imageViewer";
            this.imageViewer.Size = new System.Drawing.Size(546, 562);
            this.imageViewer.TabIndex = 1;
            // 
            // CameraForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(914, 562);
            this.Controls.Add(this.imageViewer);
            this.Controls.Add(this.imageViewCtrl1);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "CameraForm";
            this.Text = "CameraForm";
            this.ResumeLayout(false);

        }

        #endregion

        private UIControl.ImageViewCtrl imageViewCtrl1;
        private UIControl.ImageViewCtrl imageViewer;
    }
}