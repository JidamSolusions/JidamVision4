namespace JidamVision4.UIControl
{
    partial class PatternImageEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PatternImageEditor));
            this.btnUpdate = new System.Windows.Forms.Button();
            this.toolbarImageList = new System.Windows.Forms.ImageList(this.components);
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDel = new System.Windows.Forms.Button();
            this.listThumbnail = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // btnUpdate
            // 
            this.btnUpdate.ImageIndex = 0;
            this.btnUpdate.ImageList = this.toolbarImageList;
            this.btnUpdate.Location = new System.Drawing.Point(3, 3);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(37, 32);
            this.btnUpdate.TabIndex = 0;
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.Update_Click);
            // 
            // toolbarImageList
            // 
            this.toolbarImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("toolbarImageList.ImageStream")));
            this.toolbarImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.toolbarImageList.Images.SetKeyName(0, "update-12-32.png");
            this.toolbarImageList.Images.SetKeyName(1, "mask-10-32.png");
            this.toolbarImageList.Images.SetKeyName(2, "edit-60-32.png");
            this.toolbarImageList.Images.SetKeyName(3, "add-45-32.png");
            this.toolbarImageList.Images.SetKeyName(4, "reload-2-6-32.png");
            this.toolbarImageList.Images.SetKeyName(5, "del-8-32.png");
            // 
            // btnAdd
            // 
            this.btnAdd.ImageIndex = 3;
            this.btnAdd.ImageList = this.toolbarImageList;
            this.btnAdd.Location = new System.Drawing.Point(3, 41);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(37, 33);
            this.btnAdd.TabIndex = 0;
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.Add_Click);
            // 
            // btnDel
            // 
            this.btnDel.ImageIndex = 5;
            this.btnDel.ImageList = this.toolbarImageList;
            this.btnDel.Location = new System.Drawing.Point(3, 80);
            this.btnDel.Name = "btnDel";
            this.btnDel.Size = new System.Drawing.Size(37, 33);
            this.btnDel.TabIndex = 0;
            this.btnDel.UseVisualStyleBackColor = true;
            this.btnDel.Click += new System.EventHandler(this.Del_Click);
            // 
            // listThumbnail
            // 
            this.listThumbnail.Alignment = System.Windows.Forms.ListViewAlignment.Left;
            this.listThumbnail.Dock = System.Windows.Forms.DockStyle.Right;
            this.listThumbnail.HideSelection = false;
            this.listThumbnail.Location = new System.Drawing.Point(46, 0);
            this.listThumbnail.MultiSelect = false;
            this.listThumbnail.Name = "listThumbnail";
            this.listThumbnail.Size = new System.Drawing.Size(249, 132);
            this.listThumbnail.TabIndex = 1;
            this.listThumbnail.UseCompatibleStateImageBehavior = false;
            // 
            // PatternImageEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listThumbnail);
            this.Controls.Add(this.btnDel);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnUpdate);
            this.Name = "PatternImageEditor";
            this.Size = new System.Drawing.Size(295, 132);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.ImageList toolbarImageList;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnDel;
        private System.Windows.Forms.ListView listThumbnail;
    }
}
