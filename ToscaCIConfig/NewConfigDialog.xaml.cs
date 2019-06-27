using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ToscaCIConfig
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        ObservableCollection<TestConfig> configs;
        public bool isValid;
        public Window1(ObservableCollection<TestConfig> configs)
        {
            InitializeComponent();
            this.configs = configs;
        }

        private void okButton_onClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Ok clicked");
            if (IsValid(this))
            {
                Console.WriteLine("is valid");
                this.DialogResult = true;
                var cbConfig = ((MainWindow)Application.Current.MainWindow).cbConfigs;
                cbConfig.Text = tbConfigName.Text;
                return;
            }

            lStatus.Content = "Config Name already exists or is empty";
            DialogResult = null;
            return;


        }

        private void textChangedEventHandler(object sender, RoutedEventArgs e)
        {
            lStatus.Content = "";
        }

        private bool IsValid(DependencyObject node)
        {
            var configname = tbConfigName.Text;
            var matches = configs.Where(p => p.ConfigName == configname);

            if (!matches.Any() && configname != "")
                return true;
            return false;
        }
    }
}
