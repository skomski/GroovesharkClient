namespace GroovesharkPlayer.Controls
{
    partial class SettingsControl
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
            this.UserNameLabel = new System.Windows.Forms.Label();
            this.UserNameTextBox = new System.Windows.Forms.TextBox();
            this.UserSettings = new System.Windows.Forms.GroupBox();
            this.LoginButton = new System.Windows.Forms.Button();
            this.PasswordTextBox = new System.Windows.Forms.TextBox();
            this.PasswordLabel = new System.Windows.Forms.Label();
            this.ApplicationSettings = new System.Windows.Forms.GroupBox();
            this.CacheCheckBox = new System.Windows.Forms.CheckBox();
            this.backgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.UserSettings.SuspendLayout();
            this.ApplicationSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // UserNameLabel
            // 
            this.UserNameLabel.AutoSize = true;
            this.UserNameLabel.Location = new System.Drawing.Point(6, 29);
            this.UserNameLabel.Name = "UserNameLabel";
            this.UserNameLabel.Size = new System.Drawing.Size(58, 13);
            this.UserNameLabel.TabIndex = 0;
            this.UserNameLabel.Text = "Username:";
            // 
            // UserNameTextBox
            // 
            this.UserNameTextBox.Location = new System.Drawing.Point(70, 26);
            this.UserNameTextBox.Name = "UserNameTextBox";
            this.UserNameTextBox.Size = new System.Drawing.Size(124, 20);
            this.UserNameTextBox.TabIndex = 1;
            // 
            // UserSettings
            // 
            this.UserSettings.Controls.Add(this.LoginButton);
            this.UserSettings.Controls.Add(this.PasswordTextBox);
            this.UserSettings.Controls.Add(this.PasswordLabel);
            this.UserSettings.Controls.Add(this.UserNameTextBox);
            this.UserSettings.Controls.Add(this.UserNameLabel);
            this.UserSettings.Location = new System.Drawing.Point(3, 3);
            this.UserSettings.Name = "UserSettings";
            this.UserSettings.Size = new System.Drawing.Size(200, 113);
            this.UserSettings.TabIndex = 2;
            this.UserSettings.TabStop = false;
            this.UserSettings.Text = "User";
            // 
            // LoginButton
            // 
            this.LoginButton.Location = new System.Drawing.Point(9, 84);
            this.LoginButton.Name = "LoginButton";
            this.LoginButton.Size = new System.Drawing.Size(185, 23);
            this.LoginButton.TabIndex = 4;
            this.LoginButton.Text = "Login and Save";
            this.LoginButton.UseVisualStyleBackColor = true;
            this.LoginButton.Click += new System.EventHandler(this.LoginButtonClick);
            // 
            // PasswordTextBox
            // 
            this.PasswordTextBox.Location = new System.Drawing.Point(70, 53);
            this.PasswordTextBox.Name = "PasswordTextBox";
            this.PasswordTextBox.Size = new System.Drawing.Size(124, 20);
            this.PasswordTextBox.TabIndex = 3;
            this.PasswordTextBox.UseSystemPasswordChar = true;
            // 
            // PasswordLabel
            // 
            this.PasswordLabel.AutoSize = true;
            this.PasswordLabel.Location = new System.Drawing.Point(6, 56);
            this.PasswordLabel.Name = "PasswordLabel";
            this.PasswordLabel.Size = new System.Drawing.Size(56, 13);
            this.PasswordLabel.TabIndex = 2;
            this.PasswordLabel.Text = "Password:";
            // 
            // ApplicationSettings
            // 
            this.ApplicationSettings.Controls.Add(this.CacheCheckBox);
            this.ApplicationSettings.Location = new System.Drawing.Point(209, 3);
            this.ApplicationSettings.Name = "ApplicationSettings";
            this.ApplicationSettings.Size = new System.Drawing.Size(111, 113);
            this.ApplicationSettings.TabIndex = 3;
            this.ApplicationSettings.TabStop = false;
            this.ApplicationSettings.Text = "Application";
            // 
            // CacheCheckBox
            // 
            this.CacheCheckBox.AutoSize = true;
            this.CacheCheckBox.Location = new System.Drawing.Point(9, 19);
            this.CacheCheckBox.Name = "CacheCheckBox";
            this.CacheCheckBox.Size = new System.Drawing.Size(99, 17);
            this.CacheCheckBox.TabIndex = 0;
            this.CacheCheckBox.Text = "Activate Cache";
            this.CacheCheckBox.UseVisualStyleBackColor = true;
            // 
            // backgroundWorker
            // 
            this.backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorkerDoWork);
            this.backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorkerRunWorkerCompleted);
            // 
            // SettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ApplicationSettings);
            this.Controls.Add(this.UserSettings);
            this.Name = "SettingsControl";
            this.Size = new System.Drawing.Size(328, 119);
            this.UserSettings.ResumeLayout(false);
            this.UserSettings.PerformLayout();
            this.ApplicationSettings.ResumeLayout(false);
            this.ApplicationSettings.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label UserNameLabel;
        private System.Windows.Forms.TextBox UserNameTextBox;
        private System.Windows.Forms.GroupBox UserSettings;
        private System.Windows.Forms.TextBox PasswordTextBox;
        private System.Windows.Forms.Label PasswordLabel;
        private System.Windows.Forms.GroupBox ApplicationSettings;
        private System.Windows.Forms.CheckBox CacheCheckBox;
        private System.Windows.Forms.Button LoginButton;
        private System.ComponentModel.BackgroundWorker backgroundWorker;
    }
}
