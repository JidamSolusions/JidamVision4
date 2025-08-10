namespace JidamVision4.UIControl
{
    partial class MainViewToolbar
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainViewToolbar));
            this.imageListToolbar = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // imageListToolbar
            // 
            this.imageListToolbar.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListToolbar.ImageStream")));
            this.imageListToolbar.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListToolbar.Images.SetKeyName(0, "ImageOpen");
            this.imageListToolbar.Images.SetKeyName(1, "ImageSave");
            this.imageListToolbar.Images.SetKeyName(2, "Capture");
            this.imageListToolbar.Images.SetKeyName(3, "Measure");
            this.imageListToolbar.Images.SetKeyName(4, "ShowROI");
            this.imageListToolbar.Images.SetKeyName(5, "Color");
            this.imageListToolbar.Images.SetKeyName(6, "Gray");
            this.imageListToolbar.Images.SetKeyName(7, "Red");
            this.imageListToolbar.Images.SetKeyName(8, "Green");
            this.imageListToolbar.Images.SetKeyName(9, "Blue");
            // 
            // MainViewToolbar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "MainViewToolbar";
            this.Size = new System.Drawing.Size(109, 418);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList imageListToolbar;
    }
}
