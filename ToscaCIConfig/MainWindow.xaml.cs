using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.XmlConfiguration;
using Microsoft.Win32;
using Path = System.IO.Path;
using System.Globalization;

namespace ToscaCIConfig
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<TestConfig> _configs;


        private string configDir = "C:\\CiConfigs\\";
        public MainWindow()
        {

            InitializeComponent();
            _configs = new ObservableCollection<TestConfig>();
            cmbxConfigs.ItemsSource = _configs;
            addConfigsToListFromDir(ref _configs, configDir);
            this.Loaded += new RoutedEventHandler(MainWindowLoaded);
        }

        void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            cmbxConfigs.ItemsSource = _configs;
            BindingList<TestConfig> tcBindingList = new BindingList<TestConfig>(_configs);
            tcBindingList.RaiseListChangedEvents = true;
        }

        private void addConfigsToListFromDir(ref ObservableCollection<TestConfig> _configs, string dir)
        {
            string cbConfigValue = cbExecutionMode.Text;

            //open folder or create if doesn't exist
            Directory.CreateDirectory(configDir);
            foreach (string file in Directory.EnumerateFiles(dir, "*.xml"))
            {
                _configs.Add(new TestConfig(cbConfigValue, Path.GetFileNameWithoutExtension(file), file));
            }
        }


        private void NewConfig_OnClick(object sender, RoutedEventArgs e)
        {

            var configname = cmbxConfigs.Text;
            var matches = _configs.Where(p => p.ConfigName == configname);

            if (!matches.Any() && configname != "")
            {
                var path = configDir + configname + ".xml";
                _configs.Add(new TestConfig(cbExecutionMode.Text, configname, path));
                using (StreamWriter file =
                    new StreamWriter(path))
                {
                    file.Write("<xml>\n<testEvent></testEvent>");
                }
               
                //update combobox list
                
            }
        }

        private void RemoveConfig_OnClick(object sender, RoutedEventArgs e)
        {
            if (cmbxConfigs.Text != "")
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    var configname = cmbxConfigs.Text;
                    var matches = _configs.Where(p => p.ConfigName == configname);
                    if (matches.Any())
                    {
                        var path = matches.First().ConfigPath;
                        File.Delete(path);
                        _configs.Remove(matches.First());
                    }
                }
            }
        }
    }

    public class TestConfig
    {
        public TestConfig(string type, string name, string path)
        {
            ConfigType = type;
            ConfigName = name;
            ConfigPath = path;
        }
        public string ConfigType { get; set; }
        public string ConfigName { get; set; }
        public string ConfigPath { get; set; }
    }

    public class IndexToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}