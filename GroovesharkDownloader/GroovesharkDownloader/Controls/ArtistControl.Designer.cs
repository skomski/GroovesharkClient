namespace GroovesharkDownloader
{
    partial class ArtistControl
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
            this.songsControl = new GroovesharkDownloader.Controls.SongsControl();
            this.NameLabel = new System.Windows.Forms.Label();
            this.backgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // songsControl
            // 
            this.songsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.songsControl.Location = new System.Drawing.Point(0, 18);
            this.songsControl.Name = "songsControl";
            this.songsControl.Size = new System.Drawing.Size(508, 257);
            this.songsControl.TabIndex = 0;
            // 
            // NameLabel
            // 
            this.NameLabel.AutoSize = true;
            this.NameLabel.Location = new System.Drawing.Point(26, 2);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(58, 13);
            this.NameLabel.TabIndex = 1;
            this.NameLabel.Text = "ArtistName";
            // 
            // backgroundWorker
            // 
            this.backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorkerDoWork);
            this.backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorkerRunWorkerCompleted);
            // 
            // ArtistControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.NameLabel);
            this.Controls.Add(this.songsControl);
            this.Name = "ArtistControl";
            this.Size = new System.Drawing.Size(508, 275);
            this.Load += new System.EventHandler(this.ArtistLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.SongsControl songsControl;
        private System.Windows.Forms.Label NameLabel;
        private System.ComponentModel.BackgroundWorker backgroundWorker;
    }
}
