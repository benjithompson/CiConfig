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
using System.Xml;

namespace ToscaCIConfig
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<TestConfig> DexConfigsCollection;
        private ObservableCollection<TestConfig> RemoteConfigsCollection;
        private ObservableCollection<TestConfig> LocalConfigsCollection;
        private ObservableCollection<Execution> ExecutionsCollection;
        private ObservableCollection<CustomProperty> CustomPropertiesCollection;
        private ObservableCollection<string> PropertyNamesCollection;


        private string configDir = "C:\\CiConfigs\\";
        public MainWindow()
        {
            InitializeComponent();

            //Mode and Configurations
            DexConfigsCollection = new ObservableCollection<TestConfig>();
            RemoteConfigsCollection = new ObservableCollection<TestConfig>();
            LocalConfigsCollection = new ObservableCollection<TestConfig>();
            addConfigsToListFromDir();
            setcbConfigsItemSource();

            //ExecutionsCollection and Properties
            tbEvents.Content = cbExecutionMode.Text + " Executions";
            ExecutionsCollection = new ObservableCollection<Execution>();
            CustomPropertiesCollection = new ObservableCollection<CustomProperty>();
            PropertyNamesCollection = new ObservableCollection<string>();

            cbCustomProperties.ItemsSource = PropertyNamesCollection;

            //set ItemsSournce
            lvExecutions.ItemsSource = ExecutionsCollection;
            lvProperties.ItemsSource = CustomPropertiesCollection;
        }

        void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            cbCustomProperties.ItemsSource = PropertyNamesCollection;
            cbConfigs.ItemsSource = DexConfigsCollection;
            BindingList<TestConfig> dexBindingList = new BindingList<TestConfig>(DexConfigsCollection)
            {
                RaiseListChangedEvents = true
            };
            BindingList<TestConfig> remoteBindingList = new BindingList<TestConfig>(DexConfigsCollection)
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
                if (filename == null)
                {
                    return;
                }
                if (filename.StartsWith("DEX"))
                {
                    filename = Helpers.RemoveExecutionMode(filename, "DEX");
                    //test file for execution mode using filename
                    DexConfigsCollection.Add(new TestConfig(cbConfigValue, filename, file));
                }else if(filename.StartsWith("Remote"))
                {
                    filename = Helpers.RemoveExecutionMode(filename, "Remote");
                    RemoteConfigsCollection.Add(new TestConfig(cbConfigValue, filename, file));
                }else if (filename.StartsWith("Local"))
                {
                    filename = Helpers.RemoveExecutionMode(filename, "Local");
                    LocalConfigsCollection.Add(new TestConfig(cbConfigValue, filename, file));
                }
            }
        }

        private void setcbConfigsItemSource()
        {
            switch (cbExecutionMode.Text)
            {
                case "DEX":
                    cbConfigs.ItemsSource = DexConfigsCollection;
                    break;
                case "Remote":
                    cbConfigs.ItemsSource = RemoteConfigsCollection;
                    break;
                case "Local":
                    cbConfigs.ItemsSource = LocalConfigsCollection;
                    break;
            }
        }

        private void NewConfig_OnClick(object sender, RoutedEventArgs e)
        {
            var configname = cbConfigs.Text;
            var executionmode = cbExecutionMode.Text;
            var filename = executionmode + "_" + configname;

            ObservableCollection<TestConfig> configs;

            if (executionmode == "DEX")
            {
                configs = DexConfigsCollection;
            }else 
            if (executionmode == "Remote")
            {
                configs = RemoteConfigsCollection;
            }
            else
            {
                configs = LocalConfigsCollection;
            }

            var matches = configs.Where(p => p.ConfigName == configname);

            if (!matches.Any() && configname != "")
            {
                var path = configDir + executionmode + "_" + configname + ".xml";
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
                cbConfigs_OnDropDownClosed(sender, e);
            }
        }

        private void RemoveConfig_OnClick(object sender, RoutedEventArgs e)
        {
            var executionmode = cbExecutionMode.Text;
            ObservableCollection<TestConfig> configs;

            if (executionmode == "DEX")
            {
                configs = DexConfigsCollection;
            }
            else
            if (executionmode == "Remote")
            {
                configs = RemoteConfigsCollection;
            }
            else
            {
                configs = LocalConfigsCollection;
            }
            if (cbConfigs.Text != "")
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Delete TestConfig?", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    var configname = cbConfigs.Text;
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

        private void cbConfigs_OnDropDownClosed(object sender, EventArgs e)
        {
            Console.WriteLine("Combobox dropdown closed with value " + cbConfigs.Text);
            if (cbConfigs.Text == ""){return;}
            ExecutionsCollection.Clear();
            XmlNodeList testEvents = Helpers.getExecutionsListFromTestConfigFile(configDir, cbConfigs.Text, cbExecutionMode.Text);
            for (int i = 0; i < testEvents.Count; i++)
            {
                ExecutionsCollection.Add(new Execution(testEvents[i].InnerText));
            }
            
        }

        private void CbExecutionMode_OnDropDownClosed(object sender, EventArgs e)
        {
            Console.WriteLine("Dropdown closed with value " + cbExecutionMode.Text);
            //change configs list to only show configs of that type.
            setcbConfigsItemSource();
            tbEvents.Content = cbExecutionMode.Text + " Executions";
        }

        private void CbCustomProperties_OnDropDownClosed(object sender, EventArgs e)
        {
            Console.WriteLine("Property Combobox closed with value " + cbCustomProperties.Text);

        }

        private void CbCustomProperties_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                Console.WriteLine("Delete Key pressed on property combobox");
                PropertyNamesCollection.Remove(cbCustomProperties.Text);
            }

            if (e.Key == Key.Enter)
            {
                Console.WriteLine("EnterKey Pressed on property combobox");
                if (!PropertyNamesCollection.Contains(cbCustomProperties.Text))
                {
                    PropertyNamesCollection.Add(cbCustomProperties.Text);
                }
            }
        }

        private void TbProperty_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SubmitProperty_OnClick(sender, e);
            }
        }
        private void SubmitExecution_OnClick(object sender, RoutedEventArgs e)
        {
            string execution = tbExecution.Text;
            Console.WriteLine("submitExecution clicked with execution: " + execution);
            var executionMatches = ExecutionsCollection.Any(p => p.execution == execution);
            if (!executionMatches)
            {
                
                if (Helpers.MatchesExecutionPattern(execution))
                {
                    Console.WriteLine("Adding execution to ListView");
                    ExecutionsCollection.Add(new Execution(execution));
                }
                else
                {
                    Console.WriteLine("Execution doesn't match NodePath or surrogateId");
                    MessageBoxResult messageBoxResult = MessageBox.Show("'"+ execution + "' doesn't match NodePath or SurrogateId", "Incorrect Execution Entered", MessageBoxButton.OK);

                }
            }
            else
            {
                Console.WriteLine("listview already contains name and value pair");
                MessageBoxResult messageBoxResult = MessageBox.Show("'" + execution +"' already exists in list.", "Duplicate execution entered", MessageBoxButton.OK);
            }
            

        }

        private void SubmitProperty_OnClick(object sender, RoutedEventArgs e)
        {
            string propertyName = cbCustomProperties.Text;
            string propertyValue = tbProperty.Text;
            Console.WriteLine("submitProperty clicked with property " + propertyName + " and value " + propertyValue);
            //add to property listbox
            var nameMatches = CustomPropertiesCollection.Any(p => p.Name == propertyName);
            var valueMatches = CustomPropertiesCollection.Any(p => p.Value == propertyValue);

            if (!nameMatches && !valueMatches)
            {
                Console.WriteLine("Adding CustomProperty to ListView");
                CustomPropertiesCollection.Add(new CustomProperty(propertyName, propertyValue));
            }
            else
            {
                Console.WriteLine("listview already contains name and value pair");
            }
        }

        private void TbExecution_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SubmitExecution_OnClick(sender, e);
            }
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            Helpers.writeTestConfigFromList(ExecutionsCollection);
        }
    }
}