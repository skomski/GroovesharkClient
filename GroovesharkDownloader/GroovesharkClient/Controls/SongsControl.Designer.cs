namespace GroovesharkPlayer.Controls
{
    partial class SongsControl
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
            this.components = new System.ComponentModel.Container();
            this.SongsListView = new System.Windows.Forms.ListView();
            this.SongIDHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.NameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.AlbumNameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ArtistNameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DurationHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.YearHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.AddSongToQueueMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeSongFromQueueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openArtistTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openAlbumTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // SongsListView
            // 
            this.SongsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.SongIDHeader,
            this.NameHeader,
            this.AlbumNameHeader,
            this.ArtistNameHeader,
            this.DurationHeader,
            this.YearHeader});
            this.SongsListView.ContextMenuStrip = this.contextMenu;
            this.SongsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SongsListView.FullRowSelect = true;
            this.SongsListView.GridLines = true;
            this.SongsListView.Location = new System.Drawing.Point(0, 0);
            this.SongsListView.Name = "SongsListView";
            this.SongsListView.Size = new System.Drawing.Size(368, 160);
            this.SongsListView.TabIndex = 0;
            this.SongsListView.UseCompatibleStateImageBehavior = false;
            this.SongsListView.View = System.Windows.Forms.View.Details;
            this.SongsListView.DoubleClick += new System.EventHandler(this.SongsListViewDoubleClick);
            // 
            // SongIDHeader
            // 
            this.SongIDHeader.Text = "SongID";
            // 
            // NameHeader
            // 
            this.NameHeader.Text = "Name";
            // 
            // AlbumNameHeader
            // 
            this.AlbumNameHeader.Text = "Album";
            // 
            // ArtistNameHeader
            // 
            this.ArtistNameHeader.Text = "Artist";
            // 
            // DurationHeader
            // 
            this.DurationHeader.Text = "Duration";
            // 
            // YearHeader
            // 
            this.YearHeader.Text = "Year";
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddSongToQueueMenuItem,
            this.removeSongFromQueueToolStripMenuItem,
            this.openArtistTabToolStripMenuItem,
            this.openAlbumTabToolStripMenuItem});
            this.contextMenu.Name = "ContextMenu";
            this.contextMenu.Size = new System.Drawing.Size(208, 114);
            this.contextMenu.Text = "Options";
            // 
            // AddSongToQueueMenuItem
            // 
            this.AddSongToQueueMenuItem.Name = "AddSongToQueueMenuItem";
            this.AddSongToQueueMenuItem.Size = new System.Drawing.Size(207, 22);
            this.AddSongToQueueMenuItem.Text = "Add song to queue";
            this.AddSongToQueueMenuItem.Click += new System.EventHandler(this.AddSongToQueueMenuItemClick);
            // 
            // removeSongFromQueueToolStripMenuItem
            // 
            this.removeSongFromQueueToolStripMenuItem.Name = "removeSongFromQueueToolStripMenuItem";
            this.removeSongFromQueueToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.removeSongFromQueueToolStripMenuItem.Text = "Remove song from queue";
            this.removeSongFromQueueToolStripMenuItem.Click += new System.EventHandler(this.RemoveSongFromQueueToolStripMenuItemClick);
            // 
            // openArtistTabToolStripMenuItem
            // 
            this.openArtistTabToolStripMenuItem.Name = "openArtistTabToolStripMenuItem";
            this.openArtistTabToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.openArtistTabToolStripMenuItem.Text = "Open artist tab";
            this.openArtistTabToolStripMenuItem.Click += new System.EventHandler(this.OpenArtistTabToolStripMenuItemClick);
            // 
            // openAlbumTabToolStripMenuItem
            // 
            this.openAlbumTabToolStripMenuItem.Name = "openAlbumTabToolStripMenuItem";
            this.openAlbumTabToolStripMenuItem.Size = new System.Drawing.Size(207, 22);
            this.openAlbumTabToolStripMenuItem.Text = "Open album tab";
            this.openAlbumTabToolStripMenuItem.Click += new System.EventHandler(this.OpenAlbumTabToolStripMenuItemClick);
            // 
            // SongsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.SongsListView);
            this.Name = "SongsControl";
            this.Size = new System.Drawing.Size(368, 160);
            this.contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView SongsListView;
        private System.Windows.Forms.ColumnHeader SongIDHeader;
        private System.Windows.Forms.ColumnHeader NameHeader;
        private System.Windows.Forms.ColumnHeader AlbumNameHeader;
        private System.Windows.Forms.ColumnHeader ArtistNameHeader;
        private System.Windows.Forms.ColumnHeader DurationHeader;
        private System.Windows.Forms.ColumnHeader YearHeader;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem AddSongToQueueMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeSongFromQueueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openArtistTabToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openAlbumTabToolStripMenuItem;
    }
}
