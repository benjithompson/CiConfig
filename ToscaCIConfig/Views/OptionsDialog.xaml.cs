using System;
using System.CodeDom;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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
using Path = System.IO.Path;

namespace ToscaCIConfig
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class OptionsDialog : Window
    {
        private Options _options;
        public OptionsDialog()
        {
            InitializeComponent();
            _options = Helpers.GetOptions(((MainWindow)Application.Current.MainWindow)?.cbExecutionMode.Text,
                ((MainWindow)Application.Current.MainWindow)?.cbConfigs.Text);

            SetOptionsState();
        }

        private void OkButton_onClick(object sender, RoutedEventArgs e)
        {
            //TestConifg Options
            Console.WriteLine("Ok clicked");
            _options.BuildRootFolder = tbBuildRootFolder.Text.Trim();
            _options.TestMandateName = tbTestMandateName.Text.Trim();
            _options.IgnoreNonMatchingSurrogateIds = ((bool)rbIgnoreTrue.IsChecked);
            _options.CleanOldResults = ((bool)rbCleanTrue.IsChecked);

            //ClientSettings
            //TODO: validate remote execution endpoint
            _options.RemoteExecutionEndpoint = tbRemoteExecutionEndpoint.Text;
            _options.ReportPath = tbReportPath.Text.Trim();
            _options.CiClientUsername = tbCiClientUsername.Text.Trim();
            _options.CiClientPassword = tbCiClientPassword.Password;
            DialogResult = true;
        }

        private void SetOptionsState()
        {
            if (_options.IgnoreNonMatchingSurrogateIds)
            {
                rbIgnoreTrue.IsChecked = true;
            }
            else
            {
                rbIgnoreFalse.IsChecked = true;
            }

            if (_options.CleanOldResults)
            {
                rbCleanTrue.IsChecked = true;
            }
            else
            {
                rbCleanFalse.IsChecked = true;
            }
            tbBuildRootFolder.Text = _options.BuildRootFolder;
            tbTestMandateName.Text = _options.TestMandateName;
            tbRemoteExecutionEndpoint.Text = _options.RemoteExecutionEndpoint;
            tbReportPath.Text = _options.ReportPath;
            tbCiClientUsername.Text = _options.CiClientUsername;
            tbCiClientPassword.Password = _options.CiClientPassword;
        }

        private void ButtonOpenCIConfig_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("openciconfig clicked.");
        }
    }
}
