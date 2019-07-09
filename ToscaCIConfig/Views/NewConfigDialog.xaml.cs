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
    public partial class NewConfigDialog : Window
    {
        ObservableCollection<Options> configs;
        public bool isValid;
        public NewConfigDialog(ObservableCollection<Options> configs)
        {
            InitializeComponent();
            this.configs = configs;
            tbConfigName.Focus();
        }

        private void okButton_onClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Ok clicked");
            if (IsValid(this))
            {
                Console.WriteLine("is valid");
                DialogResult = true;
                var cbConfig = ((MainWindow)Application.Current.MainWindow).cbConfigs;
                cbConfig.Text = tbConfigName.Text;
                return;
            }
            lStatus.Foreground = Brushes.Red;
            lStatus.Content = "Config Name already exists or is empty";
            DialogResult = null;
        }

        private void textChangedEventHandler(object sender, RoutedEventArgs e)
        {
            lStatus.Content = "";
        }

        private bool IsValid(DependencyObject node)
        {
            var configname = tbConfigName.Text;
            var matches = configs.Where(p => p.Name == configname);

            if (!matches.Any() && configname != "")
                return true;
            return false;
        }

    }
}
