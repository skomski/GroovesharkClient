namespace GroovesharkDownloader
{
    partial class MainWindow
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

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.InitWorker = new System.ComponentModel.BackgroundWorker();
            this.MainTabControl = new System.Windows.Forms.TabControl();
            this.PlayerTabPage = new System.Windows.Forms.TabPage();
            this.PlaylistTab = new System.Windows.Forms.TabPage();
            this.SearchTab = new System.Windows.Forms.TabPage();
            this.PopularTabPage = new System.Windows.Forms.TabPage();
            this.SettingsTabPage = new System.Windows.Forms.TabPage();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.BufferStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.PlayButton = new System.Windows.Forms.Button();
            this.PauseButton = new System.Windows.Forms.Button();
            this.SeekBar = new System.Windows.Forms.TrackBar();
            this.TimeLabel = new System.Windows.Forms.Label();
            this.AudioInformationTimer = new System.Windows.Forms.Timer(this.components);
            this.PreviousButton = new System.Windows.Forms.Button();
            this.NextButton = new System.Windows.Forms.Button();
            this.NameLabel = new System.Windows.Forms.Label();
            this.AlbumLabel = new System.Windows.Forms.Label();
            this.ArtistLabel = new System.Windows.Forms.Label();
            this.VolumeTrackBar = new System.Windows.Forms.TrackBar();
            this.CacheTabPage = new System.Windows.Forms.TabPage();
            this.playerControl = new GroovesharkDownloader.Controls.QueueControl();
            this.playlistControl = new GroovesharkDownloader.UserPlaylistControl();
            this.popularControl = new GroovesharkDownloader.Controls.PopularControl();
            this.searchControl = new GroovesharkDownloader.SearchControl();
            this.settings = new GroovesharkDownloader.Controls.SettingsControl();
            this.cacheControl = new GroovesharkDownloader.Controls.CacheControl();
            this.MainTabControl.SuspendLayout();
            this.PlayerTabPage.SuspendLayout();
            this.PlaylistTab.SuspendLayout();
            this.SearchTab.SuspendLayout();
            this.PopularTabPage.SuspendLayout();
            this.SettingsTabPage.SuspendLayout();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SeekBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.VolumeTrackBar)).BeginInit();
            this.CacheTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // InitWorker
            // 
            this.InitWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorkerDoWork);
            this.InitWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorkerRunWorkerCompleted);
            // 
            // MainTabControl
            // 
            this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.MainTabControl.Controls.Add(this.PlayerTabPage);
            this.MainTabControl.Controls.Add(this.PlaylistTab);
            this.MainTabControl.Controls.Add(this.PopularTabPage);
            this.MainTabControl.Controls.Add(this.SearchTab);
            this.MainTabControl.Controls.Add(this.CacheTabPage);
            this.MainTabControl.Controls.Add(this.SettingsTabPage);
            this.MainTabControl.Location = new System.Drawing.Point(7, 6);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(707, 267);
            this.MainTabControl.TabIndex = 13;
            // 
            // PlayerTabPage
            // 
            this.PlayerTabPage.Controls.Add(this.playerControl);
            this.PlayerTabPage.Location = new System.Drawing.Point(4, 22);
            this.PlayerTabPage.Name = "PlayerTabPage";
            this.PlayerTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.PlayerTabPage.Size = new System.Drawing.Size(699, 241);
            this.PlayerTabPage.TabIndex = 4;
            this.PlayerTabPage.Text = "Player";
            this.PlayerTabPage.UseVisualStyleBackColor = true;
            // 
            // PlaylistTab
            // 
            this.PlaylistTab.Controls.Add(this.playlistControl);
            this.PlaylistTab.Location = new System.Drawing.Point(4, 22);
            this.PlaylistTab.Name = "PlaylistTab";
            this.PlaylistTab.Padding = new System.Windows.Forms.Padding(3);
            this.PlaylistTab.Size = new System.Drawing.Size(699, 241);
            this.PlaylistTab.TabIndex = 0;
            this.PlaylistTab.Text = "Playlists";
            this.PlaylistTab.UseVisualStyleBackColor = true;
            // 
            // SearchTab
            // 
            this.SearchTab.Controls.Add(this.searchControl);
            this.SearchTab.Location = new System.Drawing.Point(4, 22);
            this.SearchTab.Name = "SearchTab";
            this.SearchTab.Padding = new System.Windows.Forms.Padding(3);
            this.SearchTab.Size = new System.Drawing.Size(699, 241);
            this.SearchTab.TabIndex = 3;
            this.SearchTab.Text = "Search";
            this.SearchTab.UseVisualStyleBackColor = true;
            // 
            // PopularTabPage
            // 
            this.PopularTabPage.Controls.Add(this.popularControl);
            this.PopularTabPage.Location = new System.Drawing.Point(4, 22);
            this.PopularTabPage.Name = "PopularTabPage";
            this.PopularTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.PopularTabPage.Size = new System.Drawing.Size(699, 241);
            this.PopularTabPage.TabIndex = 6;
            this.PopularTabPage.Text = "Popular";
            this.PopularTabPage.UseVisualStyleBackColor = true;
            // 
            // SettingsTabPage
            // 
            this.SettingsTabPage.Controls.Add(this.settings);
            this.SettingsTabPage.Location = new System.Drawing.Point(4, 22);
            this.SettingsTabPage.Name = "SettingsTabPage";
            this.SettingsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.SettingsTabPage.Size = new System.Drawing.Size(699, 241);
            this.SettingsTabPage.TabIndex = 5;
            this.SettingsTabPage.Text = "Settings";
            this.SettingsTabPage.UseVisualStyleBackColor = true;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel,
            this.BufferStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 320);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(726, 22);
            this.statusStrip.TabIndex = 14;
            this.statusStrip.Text = "statusStrip1";
            // 
            // StatusLabel
            // 
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(62, 17);
            this.StatusLabel.Text = "Loading...";
            // 
            // BufferStatusLabel
            // 
            this.BufferStatusLabel.Name = "BufferStatusLabel";
            this.BufferStatusLabel.Size = new System.Drawing.Size(649, 17);
            this.BufferStatusLabel.Spring = true;
            this.BufferStatusLabel.Text = "Buffer";
            this.BufferStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // PlayButton
            // 
            this.PlayButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.PlayButton.Location = new System.Drawing.Point(14, 276);
            this.PlayButton.Name = "PlayButton";
            this.PlayButton.Size = new System.Drawing.Size(75, 20);
            this.PlayButton.TabIndex = 15;
            this.PlayButton.Text = "Play";
            this.PlayButton.UseVisualStyleBackColor = true;
            this.PlayButton.Click += new System.EventHandler(this.PlayButtonClick);
            // 
            // PauseButton
            // 
            this.PauseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.PauseButton.Location = new System.Drawing.Point(95, 276);
            this.PauseButton.Name = "PauseButton";
            this.PauseButton.Size = new System.Drawing.Size(75, 20);
            this.PauseButton.TabIndex = 16;
            this.PauseButton.Text = "Pause";
            this.PauseButton.UseVisualStyleBackColor = true;
            this.PauseButton.Click += new System.EventHandler(this.PauseButtonClick);
            // 
            // SeekBar
            // 
            this.SeekBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.SeekBar.AutoSize = false;
            this.SeekBar.LargeChange = 1;
            this.SeekBar.Location = new System.Drawing.Point(284, 275);
            this.SeekBar.Name = "SeekBar";
            this.SeekBar.Size = new System.Drawing.Size(423, 21);
            this.SeekBar.TabIndex = 17;
            this.SeekBar.TickFrequency = 10000;
            this.SeekBar.Scroll += new System.EventHandler(this.SeekBarScroll);
            this.SeekBar.ValueChanged += new System.EventHandler(this.SeekBarValueChanged);
            this.SeekBar.Leave += new System.EventHandler(this.SeekBarLeave);
            // 
            // TimeLabel
            // 
            this.TimeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.TimeLabel.AutoSize = true;
            this.TimeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TimeLabel.Location = new System.Drawing.Point(185, 280);
            this.TimeLabel.Name = "TimeLabel";
            this.TimeLabel.Size = new System.Drawing.Size(76, 13);
            this.TimeLabel.TabIndex = 18;
            this.TimeLabel.Text = "0:00 0:00 0:00";
            // 
            // AudioInformationTimer
            // 
            this.AudioInformationTimer.Enabled = true;
            this.AudioInformationTimer.Interval = 500;
            this.AudioInformationTimer.Tick += new System.EventHandler(this.AudioInformationTimerTick);
            // 
            // PreviousButton
            // 
            this.PreviousButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.PreviousButton.Location = new System.Drawing.Point(14, 297);
            this.PreviousButton.Name = "PreviousButton";
            this.PreviousButton.Size = new System.Drawing.Size(75, 20);
            this.PreviousButton.TabIndex = 19;
            this.PreviousButton.Text = "Previous";
            this.PreviousButton.UseVisualStyleBackColor = true;
            this.PreviousButton.Click += new System.EventHandler(this.PreviousButtonClick);
            // 
            // NextButton
            // 
            this.NextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.NextButton.Location = new System.Drawing.Point(95, 297);
            this.NextButton.Name = "NextButton";
            this.NextButton.Size = new System.Drawing.Size(75, 20);
            this.NextButton.TabIndex = 20;
            this.NextButton.Text = "Next";
            this.NextButton.UseVisualStyleBackColor = true;
            this.NextButton.Click += new System.EventHandler(this.NextButtonClick);
            // 
            // NameLabel
            // 
            this.NameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.NameLabel.AutoSize = true;
            this.NameLabel.BackColor = System.Drawing.Color.Transparent;
            this.NameLabel.Location = new System.Drawing.Point(185, 301);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(35, 13);
            this.NameLabel.TabIndex = 21;
            this.NameLabel.Text = "Name";
            this.NameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // AlbumLabel
            // 
            this.AlbumLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.AlbumLabel.AutoEllipsis = true;
            this.AlbumLabel.AutoSize = true;
            this.AlbumLabel.BackColor = System.Drawing.Color.Transparent;
            this.AlbumLabel.Location = new System.Drawing.Point(281, 301);
            this.AlbumLabel.Name = "AlbumLabel";
            this.AlbumLabel.Size = new System.Drawing.Size(36, 13);
            this.AlbumLabel.TabIndex = 22;
            this.AlbumLabel.Text = "Album";
            this.AlbumLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ArtistLabel
            // 
            this.ArtistLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ArtistLabel.AutoSize = true;
            this.ArtistLabel.BackColor = System.Drawing.Color.Transparent;
            this.ArtistLabel.Location = new System.Drawing.Point(385, 301);
            this.ArtistLabel.Name = "ArtistLabel";
            this.ArtistLabel.Size = new System.Drawing.Size(30, 13);
            this.ArtistLabel.TabIndex = 23;
            this.ArtistLabel.Text = "Artist";
            this.ArtistLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // VolumeTrackBar
            // 
            this.VolumeTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.VolumeTrackBar.AutoSize = false;
            this.VolumeTrackBar.LargeChange = 1;
            this.VolumeTrackBar.Location = new System.Drawing.Point(573, 301);
            this.VolumeTrackBar.Maximum = 100;
            this.VolumeTrackBar.Name = "VolumeTrackBar";
            this.VolumeTrackBar.Size = new System.Drawing.Size(132, 16);
            this.VolumeTrackBar.TabIndex = 24;
            this.VolumeTrackBar.TickFrequency = 1000;
            this.VolumeTrackBar.Scroll += new System.EventHandler(this.VolumeTrackBarScroll);
            // 
            // CacheTabPage
            // 
            this.CacheTabPage.Controls.Add(this.cacheControl);
            this.CacheTabPage.Location = new System.Drawing.Point(4, 22);
            this.CacheTabPage.Name = "CacheTabPage";
            this.CacheTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.CacheTabPage.Size = new System.Drawing.Size(699, 241);
            this.CacheTabPage.TabIndex = 7;
            this.CacheTabPage.Text = "Cache";
            this.CacheTabPage.UseVisualStyleBackColor = true;
            // 
            // playerControl
            // 
            this.playerControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.playerControl.Location = new System.Drawing.Point(3, 3);
            this.playerControl.Name = "playerControl";
            this.playerControl.Size = new System.Drawing.Size(693, 235);
            this.playerControl.TabIndex = 0;
            // 
            // playlistControl
            // 
            this.playlistControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.playlistControl.Location = new System.Drawing.Point(3, 3);
            this.playlistControl.Name = "playlistControl";
            this.playlistControl.Size = new System.Drawing.Size(693, 235);
            this.playlistControl.TabIndex = 0;
            // 
            // popularControl
            // 
            this.popularControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.popularControl.Location = new System.Drawing.Point(3, 3);
            this.popularControl.Name = "popularControl";
            this.popularControl.Size = new System.Drawing.Size(693, 235);
            this.popularControl.TabIndex = 0;
            // 
            // searchControl
            // 
            this.searchControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.searchControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.searchControl.Location = new System.Drawing.Point(3, 3);
            this.searchControl.Name = "searchControl";
            this.searchControl.Size = new System.Drawing.Size(693, 235);
            this.searchControl.TabIndex = 0;
            // 
            // settings
            // 
            this.settings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settings.Location = new System.Drawing.Point(3, 3);
            this.settings.Name = "settings";
            this.settings.Size = new System.Drawing.Size(693, 235);
            this.settings.TabIndex = 0;
            // 
            // cacheControl
            // 
            this.cacheControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cacheControl.Location = new System.Drawing.Point(3, 3);
            this.cacheControl.Name = "cacheControl";
            this.cacheControl.Size = new System.Drawing.Size(693, 235);
            this.cacheControl.TabIndex = 0;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(726, 342);
            this.Controls.Add(this.VolumeTrackBar);
            this.Controls.Add(this.ArtistLabel);
            this.Controls.Add(this.AlbumLabel);
            this.Controls.Add(this.NameLabel);
            this.Controls.Add(this.NextButton);
            this.Controls.Add(this.PreviousButton);
            this.Controls.Add(this.TimeLabel);
            this.Controls.Add(this.SeekBar);
            this.Controls.Add(this.PauseButton);
            this.Controls.Add(this.PlayButton);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.MainTabControl);
            this.MinimumSize = new System.Drawing.Size(400, 250);
            this.Name = "MainWindow";
            this.ShowIcon = false;
            this.Text = "Grooveshark";
            this.MainTabControl.ResumeLayout(false);
            this.PlayerTabPage.ResumeLayout(false);
            this.PlaylistTab.ResumeLayout(false);
            this.SearchTab.ResumeLayout(false);
            this.PopularTabPage.ResumeLayout(false);
            this.SettingsTabPage.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SeekBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.VolumeTrackBar)).EndInit();
            this.CacheTabPage.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.ComponentModel.BackgroundWorker InitWorker;
        private System.Windows.Forms.TabControl MainTabControl;
        private System.Windows.Forms.TabPage PlaylistTab;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.TabPage SearchTab;
        private SearchControl searchControl;
        private UserPlaylistControl playlistControl;
        private System.Windows.Forms.TabPage PlayerTabPage;
        private Controls.QueueControl playerControl;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.TabPage SettingsTabPage;
        private System.Windows.Forms.ToolStripStatusLabel BufferStatusLabel;
        private Controls.SettingsControl settings;
        private System.Windows.Forms.Button PlayButton;
        private System.Windows.Forms.Button PauseButton;
        private System.Windows.Forms.TrackBar SeekBar;
        private System.Windows.Forms.Label TimeLabel;
        private System.Windows.Forms.Timer AudioInformationTimer;
        private System.Windows.Forms.Button PreviousButton;
        private System.Windows.Forms.Button NextButton;
        private System.Windows.Forms.Label NameLabel;
        private System.Windows.Forms.Label AlbumLabel;
        private System.Windows.Forms.Label ArtistLabel;
        private System.Windows.Forms.TrackBar VolumeTrackBar;
        private System.Windows.Forms.TabPage PopularTabPage;
        private Controls.PopularControl popularControl;
        private System.Windows.Forms.TabPage CacheTabPage;
        private Controls.CacheControl cacheControl;
    }
}

