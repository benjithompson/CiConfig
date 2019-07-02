using System;
using System.CodeDom;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace ToscaCIConfig
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class OptionsDialog : Window
    {
        private TestConfig config;
        public OptionsDialog()
        {
            InitializeComponent();
            config = Helpers.GetTestConfig(((MainWindow) Application.Current.MainWindow)?.cbExecutionMode.Text,
                ((MainWindow) Application.Current.MainWindow)?.cbConfigs.Text);

            setOptionsStateFromConfig();
            tbToscaCiClientPath.Text = config.ToscaCiClientPath;
        }

        private void okButton_onClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Ok clicked");
            config.BuildRootFolder = tbBuildRootFolder.Text;
            config.TestMandateName = tbTestMandateName.Text;
            config.ignoreNonMatchingSurrogateIds = ((bool)rbIgnoreTrue.IsChecked);
            config.CleanOldResults = ((bool)rbCleanTrue.IsChecked);
            DialogResult = true;
        }

        private void setOptionsStateFromConfig()
        {
            if (config.ignoreNonMatchingSurrogateIds)
            {
                rbIgnoreTrue.IsChecked = true;
            }
            else
            {
                rbIgnoreFalse.IsChecked = true;
            }

            if (config.CleanOldResults)
            {
                rbCleanTrue.IsChecked = true;
            }
            else
            {
                rbCleanFalse.IsChecked = true;
            }
            tbBuildRootFolder.Text = config.BuildRootFolder;
            tbTestMandateName.Text = config.TestMandateName;
        }

        private void ButtonOpenCIClient_OnClick(object sender, RoutedEventArgs e)
        {
            var path = config.ToscaCiClientPath;
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = (Directory.Exists(path))? path : @"C:\";

            if (dlg.ShowDialog() == true)
            {
                config.ToscaCiClientPath = dlg.FileName;
                var endpoint = tbRemoteExecutionEndpoint.Text;
                config.RemoteExecutionEndpoint = (endpoint.StartsWith("http://") || 
                                                  endpoint.StartsWith("https://")) ? endpoint : "";
                config.ReportPath = tbReportPath.Text;
                config.CiClientUsername = tbCiClientUsername.Text;
                config.CiClientPassword = tbCiClientPassword.Text;

                tbToscaCiClientPath.Text = config.ToscaCiClientPath;
                Console.WriteLine(dlg.FileName);
            }
        }

        private void ButtonOpenCIConfig_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("openciconfig clicked.");
        }
    }
}
