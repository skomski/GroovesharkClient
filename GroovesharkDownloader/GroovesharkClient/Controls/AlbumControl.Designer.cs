namespace GroovesharkPlayer.Controls
{
    partial class AlbumControl
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.songsControl = new GroovesharkPlayer.Controls.SongsControl();
            this.NameLabel = new System.Windows.Forms.Label();
            this.backgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.CloseButton = new System.Windows.Forms.Button();
            this.AlbumPictureBox = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.AlbumPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // songsControl
            // 
            this.songsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.songsControl.Location = new System.Drawing.Point(0, 56);
            this.songsControl.Name = "songsControl";
            this.songsControl.Size = new System.Drawing.Size(511, 233);
            this.songsControl.TabIndex = 0;
            // 
            // NameLabel
            // 
            this.NameLabel.AutoSize = true;
            this.NameLabel.Location = new System.Drawing.Point(130, 7);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(35, 13);
            this.NameLabel.TabIndex = 1;
            this.NameLabel.Text = "Name";
            // 
            // backgroundWorker
            // 
            this.backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorkerDoWork);
            this.backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorkerRunWorkerCompleted);
            // 
            // CloseButton
            // 
            this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CloseButton.Location = new System.Drawing.Point(433, 3);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(75, 20);
            this.CloseButton.TabIndex = 2;
            this.CloseButton.Text = "Close";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButtonClick);
            // 
            // AlbumPictureBox
            // 
            this.AlbumPictureBox.Location = new System.Drawing.Point(3, 3);
            this.AlbumPictureBox.Name = "AlbumPictureBox";
            this.AlbumPictureBox.Size = new System.Drawing.Size(100, 50);
            this.AlbumPictureBox.TabIndex = 3;
            this.AlbumPictureBox.TabStop = false;
            // 
            // AlbumControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.AlbumPictureBox);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.NameLabel);
            this.Controls.Add(this.songsControl);
            this.Name = "AlbumControl";
            this.Size = new System.Drawing.Size(511, 289);
            ((System.ComponentModel.ISupportInitialize)(this.AlbumPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private SongsControl songsControl;
        private System.Windows.Forms.Label NameLabel;
        private System.ComponentModel.BackgroundWorker backgroundWorker;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.PictureBox AlbumPictureBox;
    }
}
