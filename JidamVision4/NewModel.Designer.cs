namespace JidamVision4
{
    partial class NewModel
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
            this.btnCreate = new System.Windows.Forms.Button();
            this.txtModelInfo = new System.Windows.Forms.RichTextBox();
            this.txtModelName = new System.Windows.Forms.TextBox();
            this.lbModelInfo = new System.Windows.Forms.Label();
            this.lbModelName = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnCreate
            // 
            this.btnCreate.Location = new System.Drawing.Point(187, 143);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(79, 30);
            this.btnCreate.TabIndex = 9;
            this.btnCreate.Text = "만들기";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // txtModelInfo
            // 
            this.txtModelInfo.Location = new System.Drawing.Point(77, 33);
            this.txtModelInfo.Name = "txtModelInfo";
            this.txtModelInfo.Size = new System.Drawing.Size(190, 104);
            this.txtModelInfo.TabIndex = 8;
            this.txtModelInfo.Text = "";
            // 
            // txtModelName
            // 
            this.txtModelName.Location = new System.Drawing.Point(77, 6);
            this.txtModelName.Name = "txtModelName";
            this.txtModelName.Size = new System.Drawing.Size(190, 21);
            this.txtModelName.TabIndex = 7;
            // 
            // lbModelInfo
            // 
            this.lbModelInfo.AutoSize = true;
            this.lbModelInfo.Location = new System.Drawing.Point(12, 36);
            this.lbModelInfo.Name = "lbModelInfo";
            this.lbModelInfo.Size = new System.Drawing.Size(57, 12);
            this.lbModelInfo.TabIndex = 6;
            this.lbModelInfo.Text = "모델 정보";
            // 
            // lbModelName
            // 
            this.lbModelName.AutoSize = true;
            this.lbModelName.Location = new System.Drawing.Point(12, 9);
            this.lbModelName.Name = "lbModelName";
            this.lbModelName.Size = new System.Drawing.Size(41, 12);
            this.lbModelName.TabIndex = 5;
            this.lbModelName.Text = "모델명";
            // 
            // NewModel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(273, 179);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.txtModelInfo);
            this.Controls.Add(this.txtModelName);
            this.Controls.Add(this.lbModelInfo);
            this.Controls.Add(this.lbModelName);
            this.Name = "NewModel";
            this.Text = "NewModel";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.RichTextBox txtModelInfo;
        private System.Windows.Forms.TextBox txtModelName;
        private System.Windows.Forms.Label lbModelInfo;
        private System.Windows.Forms.Label lbModelName;
    }
}