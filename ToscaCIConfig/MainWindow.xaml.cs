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
        private ObservableCollection<TestConfig> dexConfigs;
        private ObservableCollection<TestConfig> remoteConfigs;
        private ObservableCollection<TestConfig> localConfigs;


        private string configDir = "C:\\CiConfigs\\";
        public MainWindow()
        {

            InitializeComponent();
            dexConfigs = new ObservableCollection<TestConfig>();
            remoteConfigs = new ObservableCollection<TestConfig>();
            localConfigs = new ObservableCollection<TestConfig>();
            addConfigsToListFromDir();
            setcbConfigsItemSource();
            this.Loaded += new RoutedEventHandler(MainWindowLoaded);
        }

        void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            cmbxConfigs.ItemsSource = dexConfigs;
            BindingList<TestConfig> dexBindingList = new BindingList<TestConfig>(dexConfigs)
            {
                RaiseListChangedEvents = true
            };
            BindingList<TestConfig> remoteBindingList = new BindingList<TestConfig>(dexConfigs)
            {
                RaiseListChangedEvents = true
            };
        }

        private void addConfigsToListFromDir()
        {
            string cbConfigValue = cbExecutionMode.Text;

            //open folder or create if doesn't exist
            Directory.CreateDirectory(configDir);
            foreach (string file in Directory.EnumerateFiles(configDir, "*.xml"))
            {
                var filename = Path.GetFileNameWithoutExtension(file);
                if (filename.StartsWith("DEX"))
                {
                    filename = removeExecutionMode(filename, "DEX");
                    //test file for execution mode using filename
                    dexConfigs.Add(new TestConfig(cbConfigValue, filename, file));
                }else if(filename.StartsWith("Remote"))
                {
                    filename = removeExecutionMode(filename, "Remote");
                    remoteConfigs.Add(new TestConfig(cbConfigValue, filename, file));
                }else if (filename.StartsWith("Local"))
                {
                    filename = removeExecutionMode(filename, "Local");
                    localConfigs.Add(new TestConfig(cbConfigValue, filename, file));
                }
            }
        }


        private void NewConfig_OnClick(object sender, RoutedEventArgs e)
        {


            var path = "";
            var configname = cmbxConfigs.Text;
            var executionmode = cbExecutionMode.Text;
            var filename = executionmode + "_" + configname;

            ObservableCollection<TestConfig> configs;

            if (executionmode == "DEX")
            {
                configs = dexConfigs;
            }else 
            if (executionmode == "Remote")
            {
                configs = remoteConfigs;
            }
            else
            {
                configs = localConfigs;
            }
             

            var matches = configs.Where(p => p.ConfigName == configname);

            if (!matches.Any() && configname != "")
            {
                path = configDir + executionmode + "_" + configname + ".xml";
                MessageBoxResult msgBoxResult = MessageBox.Show("Creating TestConfig at " + path, "New TestConfig", MessageBoxButton.OKCancel);
                if (msgBoxResult == MessageBoxResult.OK)
                {
                    configs.Add(new TestConfig(executionmode, configname, path));
                    using (StreamWriter file =
                        new StreamWriter(path))
                    {
                        file.Write(Properties.Resources.ResourceManager.GetString(executionmode));
                    }
                }
               

            }
        }

        private void RemoveConfig_OnClick(object sender, RoutedEventArgs e)
        {
            var executionmode = cbExecutionMode.Text;
            ObservableCollection<TestConfig> configs;

            if (executionmode == "DEX")
            {
                configs = dexConfigs;
            }
            else
            if (executionmode == "Remote")
            {
                configs = remoteConfigs;
            }
            else
            {
                configs = localConfigs;
            }
            if (cmbxConfigs.Text != "")
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Delete TestConfig?", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    var configname = cmbxConfigs.Text;
                    var matches = configs.Where(p => p.ConfigName == configname);
                    if (matches.Any())
                    {
                        var path = matches.First().ConfigPath;
                        File.Delete(path);
                        configs.Remove(matches.First());
                    }
                }
            }
        }

        private void CmbxConfigs_OnDropDownClosed(object sender, EventArgs e)
        {
            Console.WriteLine("Combobox dropdown closed with value " + cmbxConfigs.Text);
        }

        private void CbExecutionMode_OnDropDownClosed(object sender, EventArgs e)
        {
            Console.WriteLine("Dropdown closed with value " + cbExecutionMode.Text);
            //change configs list to only show configs of that type.
            setcbConfigsItemSource();
        }

        private void setcbConfigsItemSource()
        {
            switch (cbExecutionMode.Text)
            {
                case "DEX":
                    cmbxConfigs.ItemsSource = dexConfigs;
                    break;
                case "Remote":
                    cmbxConfigs.ItemsSource = remoteConfigs;
                    break;
                case "Local":
                    cmbxConfigs.ItemsSource = localConfigs;
                    break;
            }
        }

        private string removeExecutionMode(string filename, string exType)
        {
            switch (exType)
            {
                case "DEX":
                    filename = filename.Substring(4, filename.Length-4);
                    break;
                case "Remote":
                    filename = filename.Substring(7, filename.Length-7);
                    break;
                case "Local":
                    filename = filename.Substring(6, filename.Length-6);
                    break;
            }
            return filename;
        }
    }




    public class RemoveIndexToBoolConverter : IValueConverter
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