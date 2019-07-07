using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Forms;
using ToscaCIConfig.Models;
using Application = System.Windows.Application;
using Path = System.IO.Path;

namespace ToscaCIConfig.Views
{
    /// <summary>
    /// Interaction logic for PreferencesDialog.xaml
    /// </summary>
    public partial class PreferencesDialog : Window
    {
        protected Preferences Preferences;

        public PreferencesDialog(Preferences preferences)
        {
            InitializeComponent();
            this.Preferences = preferences;
            tbToscaCiClientPath.Text = preferences.ToscaCiClientPath;
            tbTestConfigPath.Text = preferences.TestConfigurationsPath;
        }
        private void OkButton_onClick(object sender, RoutedEventArgs e)
        {
            //TestConifg Options
            Console.WriteLine("Ok clicked");
            Preferences.ToscaCiClientPath = tbToscaCiClientPath.Text.Trim();
            Preferences.TestConfigurationsPath = tbTestConfigPath.Text.Trim();

            SavePreferences();
            DialogResult = true;
        }

        private void ButtonOpenCIClient_OnClick(object sender, RoutedEventArgs e)
        {
            var path = Preferences.ToscaCiClientPath;
            OpenFileDialog dlg = new OpenFileDialog {InitialDirectory = (Directory.Exists(path)) ? path : @"C:\"};

            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.None)
            {
                Preferences.ToscaCiClientPath = dlg.FileName;
                tbToscaCiClientPath.Text = Preferences.ToscaCiClientPath;
            }
        }

        private void ButtonOpenTestConfigPath_OnClick(object sender, RoutedEventArgs e)
        {
            var path = Preferences.TestConfigurationsPath;
            CommonOpenFileDialog dlg = new CommonOpenFileDialog
            {
                IsFolderPicker = true, InitialDirectory = (Directory.Exists(path)) ? path : @"C:\"
            };

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Preferences.TestConfigurationsPath = dlg.FileName;
                tbToscaCiClientPath.Text = Preferences.TestConfigurationsPath;
            }
        }

        public void SavePreferences()
        {
            try
            {
                var filename = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Tricentis_GmbH\\CiConfig\\preferences.conf";
                Directory.CreateDirectory(Path.GetDirectoryName(filename));
                Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write);
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, Preferences);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
