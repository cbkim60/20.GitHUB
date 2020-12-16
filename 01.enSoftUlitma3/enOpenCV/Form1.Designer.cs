namespace enOpenCV
{
    partial class Form1
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

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnOpen = new System.Windows.Forms.Button();
            this.txtFILE = new System.Windows.Forms.TextBox();
            this.lbCONVERT = new System.Windows.Forms.ListBox();
            this.lblELAPSE = new System.Windows.Forms.Label();
            this.checkContinue = new System.Windows.Forms.CheckBox();
            this.btnReload = new System.Windows.Forms.Button();
            this.cmbDEV = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Location = new System.Drawing.Point(415, 8);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(536, 456);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(11, 8);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(90, 27);
            this.btnOpen.TabIndex = 1;
            this.btnOpen.Text = "button1";
            this.btnOpen.UseVisualStyleBackColor = true;
            // 
            // txtFILE
            // 
            this.txtFILE.Location = new System.Drawing.Point(107, 12);
            this.txtFILE.Name = "txtFILE";
            this.txtFILE.Size = new System.Drawing.Size(302, 21);
            this.txtFILE.TabIndex = 2;
            // 
            // lbCONVERT
            // 
            this.lbCONVERT.AllowDrop = true;
            this.lbCONVERT.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lbCONVERT.Font = new System.Drawing.Font("굴림", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbCONVERT.FormattingEnabled = true;
            this.lbCONVERT.HorizontalScrollbar = true;
            this.lbCONVERT.ItemHeight = 15;
            this.lbCONVERT.Location = new System.Drawing.Point(11, 62);
            this.lbCONVERT.Name = "lbCONVERT";
            this.lbCONVERT.Size = new System.Drawing.Size(398, 364);
            this.lbCONVERT.TabIndex = 3;
            // 
            // lblELAPSE
            // 
            this.lblELAPSE.Location = new System.Drawing.Point(9, 36);
            this.lblELAPSE.Name = "lblELAPSE";
            this.lblELAPSE.Size = new System.Drawing.Size(166, 23);
            this.lblELAPSE.TabIndex = 4;
            this.lblELAPSE.Text = "label1";
            this.lblELAPSE.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // checkContinue
            // 
            this.checkContinue.AutoSize = true;
            this.checkContinue.Location = new System.Drawing.Point(323, 40);
            this.checkContinue.Name = "checkContinue";
            this.checkContinue.Size = new System.Drawing.Size(83, 16);
            this.checkContinue.TabIndex = 6;
            this.checkContinue.Text = "Read Seq.";
            this.checkContinue.UseVisualStyleBackColor = true;
            // 
            // btnReload
            // 
            this.btnReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnReload.Location = new System.Drawing.Point(11, 444);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(117, 27);
            this.btnReload.TabIndex = 7;
            this.btnReload.Text = "Reload";
            this.btnReload.UseVisualStyleBackColor = true;
            // 
            // cmbDEV
            // 
            this.cmbDEV.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmbDEV.FormattingEnabled = true;
            this.cmbDEV.Location = new System.Drawing.Point(134, 448);
            this.cmbDEV.Name = "cmbDEV";
            this.cmbDEV.Size = new System.Drawing.Size(129, 20);
            this.cmbDEV.TabIndex = 8;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(963, 475);
            this.Controls.Add(this.cmbDEV);
            this.Controls.Add(this.btnReload);
            this.Controls.Add(this.checkContinue);
            this.Controls.Add(this.lblELAPSE);
            this.Controls.Add(this.lbCONVERT);
            this.Controls.Add(this.txtFILE);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.pictureBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.TextBox txtFILE;
        private System.Windows.Forms.ListBox lbCONVERT;
        private System.Windows.Forms.Label lblELAPSE;
        private System.Windows.Forms.CheckBox checkContinue;
        private System.Windows.Forms.Button btnReload;
        private System.Windows.Forms.ComboBox cmbDEV;
    }
}

