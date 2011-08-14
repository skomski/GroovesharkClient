namespace GroovesharkPlayer
{
    partial class SearchControl
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
            this.QueryTextBox = new System.Windows.Forms.TextBox();
            this.SearchButton = new System.Windows.Forms.Button();
            this.TypeComboBox = new System.Windows.Forms.ComboBox();
            this.ResultListView = new System.Windows.Forms.ListView();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addSongToQueueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addArtistToQueueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addPlaylistToQueueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.BackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.contextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // QueryTextBox
            // 
            this.QueryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.QueryTextBox.Location = new System.Drawing.Point(3, 225);
            this.QueryTextBox.Name = "QueryTextBox";
            this.QueryTextBox.Size = new System.Drawing.Size(386, 20);
            this.QueryTextBox.TabIndex = 0;
            // 
            // SearchButton
            // 
            this.SearchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchButton.Location = new System.Drawing.Point(520, 225);
            this.SearchButton.Name = "SearchButton";
            this.SearchButton.Size = new System.Drawing.Size(75, 20);
            this.SearchButton.TabIndex = 1;
            this.SearchButton.Text = "Search";
            this.SearchButton.UseVisualStyleBackColor = true;
            this.SearchButton.Click += new System.EventHandler(this.SearchButtonClick);
            // 
            // TypeComboBox
            // 
            this.TypeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.TypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TypeComboBox.FormattingEnabled = true;
            this.TypeComboBox.Location = new System.Drawing.Point(393, 225);
            this.TypeComboBox.Name = "TypeComboBox";
            this.TypeComboBox.Size = new System.Drawing.Size(121, 21);
            this.TypeComboBox.TabIndex = 2;
            // 
            // ResultListView
            // 
            this.ResultListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ResultListView.ContextMenuStrip = this.contextMenu;
            this.ResultListView.FullRowSelect = true;
            this.ResultListView.GridLines = true;
            this.ResultListView.Location = new System.Drawing.Point(3, 3);
            this.ResultListView.Name = "ResultListView";
            this.ResultListView.Size = new System.Drawing.Size(598, 217);
            this.ResultListView.TabIndex = 3;
            this.ResultListView.UseCompatibleStateImageBehavior = false;
            this.ResultListView.View = System.Windows.Forms.View.Details;
            this.ResultListView.SelectedIndexChanged += new System.EventHandler(this.ResultListViewSelectedIndexChanged);
            this.ResultListView.DoubleClick += new System.EventHandler(this.ResultListViewDoubleClick);
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addSongToQueueToolStripMenuItem,
            this.addArtistToQueueToolStripMenuItem,
            this.addPlaylistToQueueToolStripMenuItem});
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.Size = new System.Drawing.Size(186, 92);
            this.contextMenu.Text = "Menu";
            // 
            // addSongToQueueToolStripMenuItem
            // 
            this.addSongToQueueToolStripMenuItem.Name = "addSongToQueueToolStripMenuItem";
            this.addSongToQueueToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.addSongToQueueToolStripMenuItem.Text = "Add song to queue";
            this.addSongToQueueToolStripMenuItem.Visible = false;
            this.addSongToQueueToolStripMenuItem.Click += new System.EventHandler(this.AddSongToQueueToolStripMenuItemClick);
            // 
            // addArtistToQueueToolStripMenuItem
            // 
            this.addArtistToQueueToolStripMenuItem.Name = "addArtistToQueueToolStripMenuItem";
            this.addArtistToQueueToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.addArtistToQueueToolStripMenuItem.Text = "Add artist to queue";
            this.addArtistToQueueToolStripMenuItem.Visible = false;
            this.addArtistToQueueToolStripMenuItem.Click += new System.EventHandler(this.AddArtistToQueueToolStripMenuItemClick);
            // 
            // addPlaylistToQueueToolStripMenuItem
            // 
            this.addPlaylistToQueueToolStripMenuItem.Name = "addPlaylistToQueueToolStripMenuItem";
            this.addPlaylistToQueueToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.addPlaylistToQueueToolStripMenuItem.Text = "Add playlist to queue";
            this.addPlaylistToQueueToolStripMenuItem.Visible = false;
            // 
            // BackgroundWorker
            // 
            this.BackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorkerDoWork);
            this.BackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorkerRunWorkerCompleted);
            // 
            // SearchControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ResultListView);
            this.Controls.Add(this.TypeComboBox);
            this.Controls.Add(this.SearchButton);
            this.Controls.Add(this.QueryTextBox);
            this.Name = "SearchControl";
            this.Size = new System.Drawing.Size(604, 250);
            this.contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox QueryTextBox;
        private System.Windows.Forms.Button SearchButton;
        private System.Windows.Forms.ComboBox TypeComboBox;
        private System.Windows.Forms.ListView ResultListView;
        private System.ComponentModel.BackgroundWorker BackgroundWorker;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem addSongToQueueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addArtistToQueueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addPlaylistToQueueToolStripMenuItem;
    }
}
