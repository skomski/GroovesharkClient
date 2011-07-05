using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GroovesharkDownloader.Controls
{
    public partial class SettingsControl : UserControl
    {
        public SettingsControl()
        {
            InitializeComponent();

            UserNameTextBox.Text = Properties.Settings.Default.Username;
            PasswordTextBox.Text = Properties.Settings.Default.Password;
        }

        private void LoginButtonClick(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(UserNameTextBox.Text) || String.IsNullOrWhiteSpace(PasswordTextBox.Text))
            {
                MessageBox.Show("Please enter a username and a password!");
                return;
            }

            backgroundWorker.RunWorkerAsync(new Tuple<string, string>(UserNameTextBox.Text, PasswordTextBox.Text));
        }

        private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var parameters = e.Argument as Tuple<string, string>;
            e.Result = GroovesharkAPI.Client.Instance.AuthenticateUser(parameters.Item1, parameters.Item2);
        }

        private void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((bool)e.Result)
            {
                Properties.Settings.Default.Username = UserNameTextBox.Text;
                Properties.Settings.Default.Password = PasswordTextBox.Text;
                Properties.Settings.Default.Save();

                MessageBox.Show("Login success!");
            }
            else
            {
                MessageBox.Show("Login failed!");
            }
        }
    
    }
}
