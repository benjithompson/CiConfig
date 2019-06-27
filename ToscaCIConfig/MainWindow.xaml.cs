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
using System.Xml.Linq;

namespace ToscaCIConfig
{
    public partial class MainWindow : Window
    {
        public ConfigurationState state = new ConfigurationState("defaultState");
        
        private ObservableCollection<TestConfig> DexConfigsCollection;
        private ObservableCollection<TestConfig> RemoteConfigsCollection;
        private ObservableCollection<TestConfig> LocalConfigsCollection;
        private ObservableCollection<string> PropertyNamesCollection;

        private string configDir = "C:\\CiConfigs\\";

        public MainWindow()
        {
            InitializeComponent();

            var mode = cbExecutionMode.Text;
            var configname = cbConfigs.Text;

            //Mode and Configurations
            DexConfigsCollection = new ObservableCollection<TestConfig>();
            RemoteConfigsCollection = new ObservableCollection<TestConfig>();
            LocalConfigsCollection = new ObservableCollection<TestConfig>();
            initConfigsToComboBoxFromDir();
            initListViewCollections();
            initConfigsComboBoxItemSource();

            //ExecutionsCollection and Properties
            tbEvents.Content = cbExecutionMode.Text + " Executions";
            PropertyNamesCollection = new ObservableCollection<string>();
            cbCustomProperties.ItemsSource = PropertyNamesCollection;
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

        private void initConfigsToComboBoxFromDir()
        {

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
                    DexConfigsCollection.Add(new TestConfig("DEX", filename, file));
                }else if(filename.StartsWith("Remote"))
                {
                    filename = Helpers.RemoveExecutionMode(filename, "Remote");
                    RemoteConfigsCollection.Add(new TestConfig("Remote", filename, file));
                }else if (filename.StartsWith("Local"))
                {
                    filename = Helpers.RemoveExecutionMode(filename, "Local");
                    LocalConfigsCollection.Add(new TestConfig("Local", filename, file));
                }
            }
        }

        private void initConfigsComboBoxItemSource()
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

        private void initListViewCollections()
        {
            foreach (var testConfig in DexConfigsCollection)
            {
                var exNodeList = Helpers.getExecutionsListFromTestConfigFile(configDir, testConfig.ConfigName, testConfig.ConfigType);
                var exCollection = Helpers.getExecutionCollectionFromNodeList(exNodeList);
                var propNodeList = Helpers.getPropertiesListFromTestConfigFile(configDir, testConfig.ConfigName, testConfig.ConfigType);
                var propCollection = Helpers.getPropertyCollectionFromNodeList(propNodeList);
                state.setConfigListViewToState("DEX", testConfig.ConfigName, exCollection, propCollection);
            }
            foreach (var testConfig in RemoteConfigsCollection)
            {
                var exNodeList = Helpers.getExecutionsListFromTestConfigFile(configDir, testConfig.ConfigName, testConfig.ConfigType);
                var exCollection = Helpers.getExecutionCollectionFromNodeList(exNodeList);
                var propNodeList = Helpers.getPropertiesListFromTestConfigFile(configDir, testConfig.ConfigName, testConfig.ConfigType);
                var propCollection = Helpers.getPropertyCollectionFromNodeList(propNodeList);
                state.setConfigListViewToState("Remote", testConfig.ConfigName, exCollection, propCollection);
            }
            foreach (var testConfig in LocalConfigsCollection)
            {
                var exNodeList = Helpers.getExecutionsListFromTestConfigFile(configDir, testConfig.ConfigName, testConfig.ConfigType);
                var exCollection = Helpers.getExecutionCollectionFromNodeList(exNodeList);
                var propNodeList = Helpers.getPropertiesListFromTestConfigFile(configDir, testConfig.ConfigName, testConfig.ConfigType);
                var propCollection = Helpers.getPropertyCollectionFromNodeList(propNodeList);
                state.setConfigListViewToState("Local", testConfig.ConfigName, exCollection, propCollection);
            }
        }

        private void NewConfig_OnClick(object sender, RoutedEventArgs e)
        {
            var executionmode = cbExecutionMode.Text;

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

            // Instantiate the dialog box
            Window1 dlg = new Window1(configs);

            // Configure the dialog box
            dlg.Owner = this;

            // Open the dialog box modally 
            if ((bool)dlg.ShowDialog())
            {
                var configname = cbConfigs.Text;
                configs.Add(new TestConfig(executionmode, configname, ""));

                state.setConfigListViewToState(executionmode, configname, new ObservableCollection<Execution>(), new ObservableCollection<CustomProperty>());
                cbConfigs.Text = "";
                lvExecutions.ItemsSource = state.GetExecutionsList(executionmode, configname);
                lvProperties.ItemsSource = state.GetPropertiesList(executionmode, configname);
            }
        }


        //Events

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

                    state.RemoveConfigListViewFromState(executionmode, configname);

                    lvExecutions.ItemsSource = null;
                    lvProperties.ItemsSource = null;
                }
            }
        }

        private void cbConfigs_OnDropDownClosed(object sender, EventArgs e)
        {
            string configName;
            
            if (((sender as ComboBox).SelectedValue as TestConfig) == null)
            {
                Console.WriteLine("Combo dropdown closed with no selected value");
                configName = cbConfigs.Text;
                lvExecutions.ItemsSource = null;
                lvProperties.ItemsSource = null;
                return;
            }
 
            configName = ((sender as ComboBox).SelectedValue as TestConfig).ConfigName;
            Console.WriteLine("Combobox dropdown closed with value " + configName);
            cbConfigs.Text = configName;

            var exCollection = state.GetExecutionsList(cbExecutionMode.Text, configName);
            var propCollection = state.GetPropertiesList(cbExecutionMode.Text, configName);

            lvExecutions.ItemsSource = exCollection;
            lvProperties.ItemsSource = propCollection;
        }

        private void CbExecutionMode_OnDropDownClosed(object sender, EventArgs e)
        {
            Console.WriteLine("Dropdown closed with value " + cbExecutionMode.Text);
            //change configs list to only show configs of that type.
            initConfigsComboBoxItemSource();
            tbEvents.Content = cbExecutionMode.Text + " Executions";
            lvProperties.ItemsSource = state.GetPropertiesList(cbExecutionMode.Text, cbConfigs.Text);
            lvExecutions.ItemsSource = state.GetExecutionsList(cbExecutionMode.Text, cbConfigs.Text);
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
            string executionText = tbExecution.Text;
            Console.WriteLine("submitExecution clicked with execution: " + executionText);
            var executionsList = state.GetExecutionsList(cbExecutionMode.Text, cbConfigs.Text);
            var executionMatches = executionsList.Any(p => p.execution == executionText);
            if (!executionMatches)
            {
                
                if (Helpers.MatchesExecutionPattern(executionText))
                {
                    Console.WriteLine("Adding execution to ListView");
                    executionsList.Add(new Execution(executionText));
                    lvExecutions.ItemsSource = executionsList;
                }
                else
                {
                    Console.WriteLine("Execution doesn't match NodePath or surrogateId syntax. Check the Tosca object nodepath or Unique ID again...");
                    MessageBoxResult messageBoxResult = MessageBox.Show("'"+ executionText + "' doesn't match NodePath or SurrogateId", "Incorrect Execution Entered", MessageBoxButton.OK);

                }
            }
            else
            {
                Console.WriteLine("listview already contains name and value pair");
                MessageBoxResult messageBoxResult = MessageBox.Show("'" + executionText +"' already exists in list.", "Duplicate execution entered", MessageBoxButton.OK);
            }
            

        }

        private void SubmitProperty_OnClick(object sender, RoutedEventArgs e)
        {
            string propertyName = cbCustomProperties.Text;
            string propertyValue = tbProperty.Text;
            Console.WriteLine("submitProperty clicked with property " + propertyName + " and value " + propertyValue);
            //add to property listbox
            var prop = state.GetPropertiesList(cbExecutionMode.Text, cbConfigs.Text);
            var nameMatches = prop.Any(p => p.Name == propertyName);
            var valueMatches = prop.Any(p => p.Value == propertyValue);

            if (nameMatches && valueMatches)
            {
                Console.WriteLine("listview already contains name and value pair");
                MessageBoxResult messageBoxResult = MessageBox.Show("'" + propertyName + "' already exists in list.", "Duplicate execution entered", MessageBoxButton.OK);
                //prop.Add(new CustomProperty(propertyName, propertyValue));
            }
            else
            {
                Console.WriteLine("Adding CustomProperty to ListView");
                prop.Add(new CustomProperty(propertyName, propertyValue));
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
            var mode = cbExecutionMode.Text;
            var configName = cbConfigs.Text;
            
            var el = state.GetExecutionsList(cbExecutionMode.Text, cbConfigs.Text);
            var prop = state.GetPropertiesList(cbExecutionMode.Text, cbConfigs.Text);
            var path = configDir + mode + "_" + configName + ".xml";

            using (StreamWriter file =
                new StreamWriter(path))
            {
                file.Write(Properties.Resources.ResourceManager.GetString(mode));
            }

            // load the XML file into an XElement
            XDocument doc = XDocument.Load(path);
            var testEvents = doc.Descendants("TestEvents").FirstOrDefault();
            if (testEvents != null)
            {
                foreach (var ex in el)
                {
                    testEvents.Add(new XElement("TestEvent", ex.execution));
                }
                doc.Save(path);
            }

            var customProperties = doc.Descendants("customProperties").FirstOrDefault();
            if (customProperties != null)
            {
                foreach (var cp in prop)
                {
                    customProperties.Add(new XElement("customProperty", cp.Value));
                    customProperties.Add(new XAttribute("name", cp.Name));
                }
                doc.Save(path);
            }
            //Helpers.writeTestConfigXmlFromList(configDir, mode, configName, el, prop);
        }
    }
}