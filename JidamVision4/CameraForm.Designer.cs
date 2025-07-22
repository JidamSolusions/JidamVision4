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
            this.imageViewer = new JidamVision4.UIControl.ImageViewCtrl();
            this.imageViewCtrl1 = new JidamVision4.UIControl.ImageViewCtrl();
            this.SuspendLayout();
            // 
            // imageViewer
            // 
            this.imageViewer.Dock = System.Windows.Forms.DockStyle.Left;
            this.imageViewer.Location = new System.Drawing.Point(0, 0);
            this.imageViewer.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.imageViewer.Name = "imageViewer";
            this.imageViewer.Size = new System.Drawing.Size(478, 450);
            this.imageViewer.TabIndex = 1;
            // 
            // imageViewCtrl1
            // 
            this.imageViewCtrl1.Location = new System.Drawing.Point(0, 0);
            this.imageViewCtrl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.imageViewCtrl1.Name = "imageViewCtrl1";
            this.imageViewCtrl1.Size = new System.Drawing.Size(131, 120);
            this.imageViewCtrl1.TabIndex = 0;
            // 
            // CameraForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.imageViewer);
            this.Controls.Add(this.imageViewCtrl1);
            this.Name = "CameraForm";
            this.Text = "CameraForm";
            this.Resize += new System.EventHandler(this.CameraForm_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private UIControl.ImageViewCtrl imageViewCtrl1;
        private UIControl.ImageViewCtrl imageViewer;
    }
}