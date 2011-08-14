namespace GroovesharkPlayer.Controls
{
    partial class CacheControl
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
            this.CacheFileWatcher = new System.IO.FileSystemWatcher();
            this.songsControl = new GroovesharkPlayer.Controls.SongsControl();
            this.backgroundWorker = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.CacheFileWatcher)).BeginInit();
            this.SuspendLayout();
            // 
            // CacheFileWatcher
            // 
            this.CacheFileWatcher.EnableRaisingEvents = true;
            this.CacheFileWatcher.SynchronizingObject = this;
            this.CacheFileWatcher.Changed += new System.IO.FileSystemEventHandler(this.CacheFileWatcherChanged);
            // 
            // songsControl
            // 
            this.songsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.songsControl.Location = new System.Drawing.Point(0, 0);
            this.songsControl.Name = "songsControl";
            this.songsControl.Size = new System.Drawing.Size(484, 240);
            this.songsControl.TabIndex = 0;
            // 
            // backgroundWorker
            // 
            this.backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorkerDoWork);
            this.backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorkerRunWorkerCompleted);
            // 
            // CacheControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.songsControl);
            this.Name = "CacheControl";
            this.Size = new System.Drawing.Size(484, 240);
            ((System.ComponentModel.ISupportInitialize)(this.CacheFileWatcher)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.IO.FileSystemWatcher CacheFileWatcher;
        private SongsControl songsControl;
        private System.ComponentModel.BackgroundWorker backgroundWorker;
    }
}
