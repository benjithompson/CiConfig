using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    }
}
