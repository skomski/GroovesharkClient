namespace GroovesharkDownloader.Controls
{
    partial class PlaylistControl
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
            this.PlayListIDLabel = new System.Windows.Forms.Label();
            this.PlayListName = new System.Windows.Forms.Label();
            this.songsControl = new GroovesharkDownloader.Controls.SongsControl();
            this.SuspendLayout();
            // 
            // PlayListIDLabel
            // 
            this.PlayListIDLabel.AutoSize = true;
            this.PlayListIDLabel.Location = new System.Drawing.Point(6, 7);
            this.PlayListIDLabel.Name = "PlayListIDLabel";
            this.PlayListIDLabel.Size = new System.Drawing.Size(68, 13);
            this.PlayListIDLabel.TabIndex = 0;
            this.PlayListIDLabel.Text = "PLAYLISTID";
            // 
            // PlayListName
            // 
            this.PlayListName.AutoSize = true;
            this.PlayListName.Location = new System.Drawing.Point(184, 7);
            this.PlayListName.Name = "PlayListName";
            this.PlayListName.Size = new System.Drawing.Size(88, 13);
            this.PlayListName.TabIndex = 2;
            this.PlayListName.Text = "PLAYLISTNAME";
            // 
            // songsControl
            // 
            this.songsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.songsControl.Location = new System.Drawing.Point(0, 26);
            this.songsControl.Name = "songsControl";
            this.songsControl.Size = new System.Drawing.Size(365, 258);
            this.songsControl.TabIndex = 3;
            // 
            // PlaylistControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.songsControl);
            this.Controls.Add(this.PlayListName);
            this.Controls.Add(this.PlayListIDLabel);
            this.Name = "PlaylistControl";
            this.Size = new System.Drawing.Size(365, 284);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label PlayListIDLabel;
        private System.Windows.Forms.Label PlayListName;
        private SongsControl songsControl;

    }
}
