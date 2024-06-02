namespace RunThisAsAdmin
{
    partial class AboutForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
            this.ProductNameLabel = new System.Windows.Forms.Label();
            this.ProductVersionLabel = new System.Windows.Forms.Label();
            this.LogoPictureBox = new System.Windows.Forms.PictureBox();
            this.GitHubPictureBox = new System.Windows.Forms.PictureBox();
            this.CheckForUpdatesPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.LogoPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GitHubPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CheckForUpdatesPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // ProductNameLabel
            // 
            this.ProductNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ProductNameLabel.Location = new System.Drawing.Point(146, 20);
            this.ProductNameLabel.Margin = new System.Windows.Forms.Padding(3);
            this.ProductNameLabel.Name = "ProductNameLabel";
            this.ProductNameLabel.Size = new System.Drawing.Size(226, 25);
            this.ProductNameLabel.TabIndex = 0;
            this.ProductNameLabel.Text = "ProductName";
            this.ProductNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ProductVersionLabel
            // 
            this.ProductVersionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ProductVersionLabel.Location = new System.Drawing.Point(146, 51);
            this.ProductVersionLabel.Margin = new System.Windows.Forms.Padding(3);
            this.ProductVersionLabel.Name = "ProductVersionLabel";
            this.ProductVersionLabel.Size = new System.Drawing.Size(226, 26);
            this.ProductVersionLabel.TabIndex = 1;
            this.ProductVersionLabel.Text = "ProductVersion";
            this.ProductVersionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LogoPictureBox
            // 
            this.LogoPictureBox.Image = global::RunThisAsAdmin.Properties.Resources.Logo;
            this.LogoPictureBox.Location = new System.Drawing.Point(12, 9);
            this.LogoPictureBox.Name = "LogoPictureBox";
            this.LogoPictureBox.Size = new System.Drawing.Size(128, 128);
            this.LogoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LogoPictureBox.TabIndex = 3;
            this.LogoPictureBox.TabStop = false;
            // 
            // GitHubPictureBox
            // 
            this.GitHubPictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.GitHubPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("GitHubPictureBox.Image")));
            this.GitHubPictureBox.Location = new System.Drawing.Point(332, 97);
            this.GitHubPictureBox.Name = "GitHubPictureBox";
            this.GitHubPictureBox.Size = new System.Drawing.Size(40, 40);
            this.GitHubPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.GitHubPictureBox.TabIndex = 5;
            this.GitHubPictureBox.TabStop = false;
            this.GitHubPictureBox.Click += new System.EventHandler(this.GitHubPictureBox_Click);
            this.GitHubPictureBox.MouseEnter += new System.EventHandler(this.GitHubPictureBox_MouseEnter);
            this.GitHubPictureBox.MouseLeave += new System.EventHandler(this.GitHubPictureBox_MouseLeave);
            // 
            // CheckForUpdatesPictureBox
            // 
            this.CheckForUpdatesPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CheckForUpdatesPictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.CheckForUpdatesPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("CheckForUpdatesPictureBox.Image")));
            this.CheckForUpdatesPictureBox.Location = new System.Drawing.Point(146, 97);
            this.CheckForUpdatesPictureBox.Name = "CheckForUpdatesPictureBox";
            this.CheckForUpdatesPictureBox.Size = new System.Drawing.Size(180, 40);
            this.CheckForUpdatesPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.CheckForUpdatesPictureBox.TabIndex = 6;
            this.CheckForUpdatesPictureBox.TabStop = false;
            this.CheckForUpdatesPictureBox.Click += new System.EventHandler(this.CheckForUpdatesPictureBox_Click);
            this.CheckForUpdatesPictureBox.MouseEnter += new System.EventHandler(this.CheckForUpdatesPictureBox_MouseEnter);
            this.CheckForUpdatesPictureBox.MouseLeave += new System.EventHandler(this.CheckForUpdatesPictureBox_MouseLeave);
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 149);
            this.Controls.Add(this.CheckForUpdatesPictureBox);
            this.Controls.Add(this.GitHubPictureBox);
            this.Controls.Add(this.LogoPictureBox);
            this.Controls.Add(this.ProductVersionLabel);
            this.Controls.Add(this.ProductNameLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "About";
            this.Load += new System.EventHandler(this.AboutForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.LogoPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GitHubPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CheckForUpdatesPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label ProductNameLabel;
        private System.Windows.Forms.Label ProductVersionLabel;
        private System.Windows.Forms.PictureBox LogoPictureBox;
        private System.Windows.Forms.PictureBox GitHubPictureBox;
        private System.Windows.Forms.PictureBox CheckForUpdatesPictureBox;
    }
}
