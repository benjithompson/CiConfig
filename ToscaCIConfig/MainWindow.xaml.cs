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

    //Todo: SurrogateIDs, ignoreNonMatchingIds, buildrootfolder, cleanoldresults, testmandatename

    public partial class MainWindow : Window
    {
        public ConfigurationState state = new ConfigurationState("defaultState");

        public ObservableCollection<TestConfig> DexConfigsCollection;
        public ObservableCollection<TestConfig> RemoteConfigsCollection;
        public ObservableCollection<TestConfig> LocalConfigsCollection;
        private ObservableCollection<string> PropertyNamesCollection;

        private string configDir = "C:\\CiConfigs\\";

        public string executionModeHeader = "";

        public MainWindow()
        {
            InitializeComponent();

            var mode = cbExecutionMode.Text;
            var configname = cbConfigs.Text;

            setListViewHeader(mode);

            //Mode and Configurations
            DexConfigsCollection = new ObservableCollection<TestConfig>();
            RemoteConfigsCollection = new ObservableCollection<TestConfig>();
            LocalConfigsCollection = new ObservableCollection<TestConfig>();
            initConfigCollectionsFromConfigFile();
            InitState();
            initConfigsComboBoxItemSource();

            //ExecutionsCollection and Properties
            tbEvents.Content = mode + " Executions";
            PropertyNamesCollection = new ObservableCollection<string>();
            cbCustomProperties.ItemsSource = PropertyNamesCollection;
            lstatus.Content = "Ready!";
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

        private void initConfigCollectionsFromConfigFile()
        {
            lstatus.Content = "Loading Test Configurations...";
            //open folder or create if doesn't exist
            Directory.CreateDirectory(configDir);
            foreach (string file in Directory.EnumerateFiles(configDir, "*.xml"))
            {
                var configName = Path.GetFileNameWithoutExtension(file);
                if (configName == null)
                {
                    return;
                }

                if (configName.StartsWith("DEX"))
                {
                    configName = Helpers.RemoveExecutionMode(configName, "DEX");
                    //test file for execution mode using filename
                    DexConfigsCollection.Add(new TestConfig("DEX", configName, file));
                }
                else if (configName.StartsWith("Remote"))
                {
                    configName = Helpers.RemoveExecutionMode(configName, "Remote");
                    RemoteConfigsCollection.Add(new TestConfig("Remote", configName, file));
                }
                else if (configName.StartsWith("Local"))
                {
                    configName = Helpers.RemoveExecutionMode(configName, "Local");
                    LocalConfigsCollection.Add(new TestConfig("Local", configName, file));
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

        private void InitState()
        {
            lstatus.Content = "Loading Test Configurations...";
            foreach (var testConfig in DexConfigsCollection)
            {
                var exNodeList =
                    Helpers.getExecutionsNodeListFromTestConfigFile(configDir, testConfig.Name,
                        testConfig.Mode);
                var exCollection = Helpers.GetExecutionCollectionFromNodeList(exNodeList);
                var propNodeList =
                    Helpers.getPropertiesNodeListFromTestConfigFile(configDir, testConfig.Name,
                        testConfig.Mode);
                var propCollection = Helpers.GetPropertyCollectionFromNodeList(propNodeList);
                
                //Todo: Handle SurrogateIDs
                state.setStateCollections("DEX", testConfig.Name, exCollection, propCollection);
            }

            foreach (var testConfig in RemoteConfigsCollection)
            {
                var exNodeList =
                    Helpers.getExecutionsNodeListFromTestConfigFile(configDir, testConfig.Name,
                        testConfig.Mode);
                var exCollection = Helpers.GetExecutionCollectionFromNodeList(exNodeList);
                var propNodeList =
                    Helpers.getPropertiesNodeListFromTestConfigFile(configDir, testConfig.Name,
                        testConfig.Mode);
                var propCollection = Helpers.GetPropertyCollectionFromNodeList(propNodeList);
                //Todo:get options
                var surrogateList =
                    Helpers.getSurrogateIdsNodeListFromTestConfigFile(configDir, testConfig.Name, testConfig.Mode,
                        "surrogateIds");
                var surrogateIdsCollection = Helpers.GetSurrogateIdsCollectionFromNodeList(surrogateList);
                Helpers.setTestConfigOptionsFromFile(testConfig);
                state.setStateCollections("Remote", testConfig.Name, exCollection, propCollection);
            }

            foreach (var testConfig in LocalConfigsCollection)
            {
                var exNodeList =
                    Helpers.getExecutionsNodeListFromTestConfigFile(configDir, testConfig.Name,
                        testConfig.Mode);
                var exCollection = Helpers.GetExecutionCollectionFromNodeList(exNodeList);
                var propNodeList =
                    Helpers.getPropertiesNodeListFromTestConfigFile(configDir, testConfig.Name,
                        testConfig.Mode);
                var propCollection = Helpers.GetPropertyCollectionFromNodeList(propNodeList);
                //Todo:get options
                Helpers.setTestConfigOptionsFromFile(testConfig);
                state.setStateCollections("Local", testConfig.Name, exCollection, propCollection);
            }
        }


        private string GetCiClientCommandString()
        {
            var ciclient = "ToscaCiClient.exe";
            var mode = "";
            var config = "";

            if (cbExecutionMode.Text == "Local")
            {
                mode = "-m local";
            }
            else
            {
                mode = "-m distributed ";
            }

            config = "-c " + "\"" + configDir + cbExecutionMode.Text + "_" + cbConfigs.Text + ".xml" + "\"";

            return ciclient + " " + mode + config;
        }

        private void setListViewHeader(string mode)
        {
            if (mode == "DEX")
            {
                ((GridView)lvExecutions.View).Columns[0].Header = "NodePath/SurrogateId";
            }
            else
            {
                ((GridView)lvExecutions.View).Columns[0].Header = "Execution Type";
            }
        }



        //Events

        private void NewConfig_OnClick(object sender, RoutedEventArgs e)
        {
            var mode = cbExecutionMode.Text;
            var configname = "";
            ObservableCollection<TestConfig> configs = Helpers.GetTestConfigsCollection(mode);

            // Instantiate the dialog box
            NewConfigDialog dlg = new NewConfigDialog(configs);

            // Configure the dialog box
            dlg.Owner = this;

            // Open the dialog box modally 
            if ((bool) dlg.ShowDialog())
            {
                configname = cbConfigs.Text;
                configs.Add(new TestConfig(mode, configname, configDir));

                state.setStateCollections(mode, configname, new ObservableCollection<Execution>(),
                    new ObservableCollection<CustomProperty>());
                cbConfigs.Text = "";
                lvExecutions.ItemsSource = state.GetExecutionsList(mode, configname);
                lvProperties.ItemsSource = state.GetPropertiesList(mode, configname);
                lstatus.Content = "Test Configuration '" + configname + "' created";
            }
        }

        private void RemoveConfig_OnClick(object sender, RoutedEventArgs e)
        {
            var mode = cbExecutionMode.Text;
            var configname = cbConfigs.Text;
            ObservableCollection<TestConfig> configs = Helpers.GetTestConfigsCollection(mode);

            if (cbConfigs.Text != "")
            {
                MessageBoxResult messageBoxResult =
                    MessageBox.Show("Are you sure?", "Delete TestConfig?", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    
                    var match = Helpers.GetTestConfig(mode, configname);
                    if (match != null)
                    {
                        var path = match.Path;
                        File.Delete(path);
                        configs.Remove(match);
                    }

                    state.RemoveConfigListViewFromState(mode, configname);

                    lvExecutions.ItemsSource = null;
                    lvProperties.ItemsSource = null;
                    lstatus.Content = "Test Configuration '" + configname + "' deleted";
                }
            }
        }

        private void cbConfigs_OnDropDownClosed(object sender, EventArgs e)
        {
            string name = "";
            string mode = cbExecutionMode.Text;

            if (((sender as ComboBox).SelectedValue as TestConfig) == null)
            {
                Console.WriteLine("Combo dropdown closed with no selected value");
                name = cbConfigs.Text;
                lvExecutions.ItemsSource = null;
                lvProperties.ItemsSource = null;
                return;
            }

            name = ((sender as ComboBox).SelectedValue as TestConfig).Name;
            Console.WriteLine("Combobox dropdown closed with value " + name);
            //cbConfigs.Text = configName;

            var exCollection = state.GetExecutionsList(mode, name);
            var propCollection = state.GetPropertiesList(mode, name);

            lvExecutions.ItemsSource = exCollection;
            lvProperties.ItemsSource = propCollection;
            setListViewHeader(mode);
            lstatus.Content = "Test Configuration changed to '" + name + "'";
        }

        private void CbExecutionMode_OnDropDownClosed(object sender, EventArgs e)
        {
            Console.WriteLine("Dropdown closed with value " + cbExecutionMode.Text);

            var mode = cbExecutionMode.Text;
            var configname = cbConfigs.Text;

            //change configs list to only show configs of that type.
            initConfigsComboBoxItemSource();
            tbEvents.Content = cbExecutionMode.Text + " Executions";
            lvProperties.ItemsSource = state.GetPropertiesList(mode, configname);
            lvExecutions.ItemsSource = state.GetExecutionsList(mode, configname);
            lstatus.Content = "Test Configuration mode changed to " + mode;
        }

        private void CbCustomProperties_OnDropDownClosed(object sender, EventArgs e)
        {
            Console.WriteLine(@"Property Combobox closed with value " + cbCustomProperties.Text);

        }

        private void CbCustomProperties_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                Console.WriteLine(@"Delete Key pressed on property combobox");
                PropertyNamesCollection.Remove(cbCustomProperties.Text);
                lstatus.Content = "Property Removed from Combobox";
            }

            if (e.Key == Key.Enter)
            {
                Console.WriteLine(@"EnterKey Pressed on property combobox");
                if (!PropertyNamesCollection.Contains(cbCustomProperties.Text))
                {
                    PropertyNamesCollection.Add(cbCustomProperties.Text);
                    lstatus.Content = "Property Name Added to Combobox";
                }
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
                if (cbExecutionMode.Text == "DEX")
                {
                    if (Helpers.ExecutionPatternIsValid(executionText))
                    {
                        Console.WriteLine("Adding execution to ListView");
                        executionsList.Add(new Execution(executionText));
                        lvExecutions.ItemsSource = executionsList;
                        lstatus.Content = "Execution Added";
                    }
                    else
                    {
                        Console.WriteLine(
                            "TestEvents require NodePath or surrogateId. Make sure the nodepath or SurrogateID is correct.");
                        MessageBoxResult messageBoxResult =
                            MessageBox.Show("'" + executionText + "' doesn't match NodePath or SurrogateId",
                                "Incorrect Execution for DEX Entered", MessageBoxButton.OK);
                    }
                }
                else
                {
                    Console.WriteLine("Adding execution to ListView");
                    executionsList.Add(new Execution(executionText));
                    lvExecutions.ItemsSource = executionsList;
                    
                }

            }
            else
            {
                Console.WriteLine("listview already contains name and value pair");
                MessageBoxResult messageBoxResult = MessageBox.Show("'" + executionText + "' already exists in list.",
                    "Duplicate execution entered", MessageBoxButton.OK);
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
                MessageBoxResult messageBoxResult = MessageBox.Show("'" + propertyName + "' already exists in list.",
                    "Duplicate execution entered", MessageBoxButton.OK);
                //prop.Add(new CustomProperty(propertyName, propertyValue));
            }
            else
            {
                Console.WriteLine("Adding CustomProperty to ListView");
                prop.Add(new CustomProperty(propertyName, propertyValue));
                lvProperties.ItemsSource = prop;
                lstatus.Content = "Property Added";
            }
        }

        private void TbExecution_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SubmitExecution_OnClick(sender, e);
            }
            lstatus.Content = "Execution Added";
        }

        private void TbProperty_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && tbProperty.Text != "")
            {
                SubmitProperty_OnClick(sender, e);
            }
            lstatus.Content = "Property Added";
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            lstatus.Content = "Saving Configurations...";
            if (cbConfigs.Text == "")
            {
                MessageBoxResult msg =
                    MessageBox.Show("Load a Configuration before saving", "No Config Loaded", MessageBoxButton.OK);
                return;
            }
            SaveConfigFiles();
            MessageBoxResult msgsave =
                MessageBox.Show("Test Configs saved!", "File Saved", MessageBoxButton.OK);

        }

        public void SaveConfigFiles()
        {
            
            foreach (var config in DexConfigsCollection)
            {
                SaveConfigFile(config.Name, config.Mode);
            }

            foreach (var config in RemoteConfigsCollection)
            {
                SaveConfigFile(config.Name, config.Mode);
            }

            foreach (var config in LocalConfigsCollection)
            {
                SaveConfigFile(config.Name, config.Mode);
            }
        }

        public void SaveConfigFile(string configName, string mode)
        {
            
            var config = Helpers.GetTestConfig(mode, configName);
            var el = state.GetExecutionsList(mode, configName);
            var prop = state.GetPropertiesList(mode, configName);
            var path = configDir + mode + "_" + configName + ".xml";
            XElement executions;
            XElement surrogateIds;
            using (StreamWriter file =
                new StreamWriter(path))
            {
                file.Write(Properties.Resources.ResourceManager.GetString(mode));
            }

            // load the XML file into an XElement
            XDocument doc = XDocument.Load(path);

            foreach (var ex in el)
            {
                if (mode == "DEX")
                {
                    executions = doc.Descendants("TestEvents").First();
                    executions.Add(new XElement("TestEvent", ex.execution));

                }
                else
                {
                    executions = doc.Descendants("ExecutionTypes").First();
                    surrogateIds = doc.Descendants("surrogateIds").First();
                    if (Helpers.IsSurrogateId(ex.execution))
                    {
                        surrogateIds.Add(new XElement("surrogateId", ex.execution));
                    }
                    else
                    {
                        executions.Add(new XElement("EventTypes", ex.execution));
                    }
                }
            }

            var customProperties = doc.Descendants("customProperties").First();
            if (customProperties != null)
            {
                foreach (var cp in prop)
                {
                    customProperties.Add(new XElement("customProperty", cp.Value, new XAttribute("name", cp.Name)));
                }

            }

            if (config.Mode != "DEX")
            {
                try
                {
                    var option = doc.Descendants("ignoreNonMatchingIds").First();
                    option.Value = config.ignoreNonMatchingSurrogateIds.ToString();
                    option = doc.Descendants("buildrootfolder").First();
                    option.Value = config.BuildRootFolder;
                    option = doc.Descendants("cleanoldresults").First();
                    option.Value = config.CleanOldResults.ToString();
                    option = doc.Descendants("testMandateName").First();
                    option.Value = config.TestMandateName;
                }
                catch (Exception e)
                {
                    
                    Console.WriteLine("Option not found.");
                }

            }
                
            doc.Save(path);
            lstatus.Content = "Test Configurations Saved to " + path;
        }

        private void ButtonOptions_OnClick(object sender, RoutedEventArgs e)
        {

            //open options dialog
            if (cbConfigs.Text == "")
            {
                MessageBoxResult messageBoxResult =
                    MessageBox.Show("Load a Test Local or Remote Config for Options.", "Error", MessageBoxButton.OK);
                return;
            }

            if (cbExecutionMode.Text == "DEX")
            {
                MessageBoxResult messageBoxResult =
                    MessageBox.Show("DEX doesn't have options", "Error", MessageBoxButton.OK);
                return;
            }

            OptionsDialog dlg = new OptionsDialog();
            dlg.ShowDialog();

        }

        private void LvExecutions_RemoveSelectedItems(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Delete)
            {
                var mode = cbExecutionMode.Text;
                var name = cbConfigs.Text;
                var exList = state.GetExecutionsList(mode, name);
                List<Execution> RemovedItems = new List<Execution>();
                foreach (Execution removedItem in lvExecutions.SelectedItems)
                {
                    RemovedItems.Add(removedItem);
                }

                foreach (var removedItem in RemovedItems)
                {
                    exList.Remove(removedItem);
                }

                lstatus.Content = "Execution Removed";
                lvExecutions.Items.Refresh();
            }

        }

        private void LvProperties_RemoveSelectedItems(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                var mode = cbExecutionMode.Text;
                var name = cbConfigs.Text;
                var exList = state.GetPropertiesList(mode, name);
                List<CustomProperty> RemovedItems = new List<CustomProperty>();
                foreach (CustomProperty removedItem in lvProperties.SelectedItems)
                {
                    RemovedItems.Add(removedItem);
                }

                foreach (var removedItem in RemovedItems)
                {
                    exList.Remove(removedItem);
                }

                lstatus.Content = "Properties Removed";
                lvExecutions.Items.Refresh();
            }
        }

        private void ButtonCopy_OnClick(object sender, RoutedEventArgs e)
        {
            if (cbConfigs.Text == "")
            {
                lstatus.Content = "Select a Test Configuration!";
                lstatus.Foreground = Brushes.White;
                return;
            }
            var cmd = GetCiClientCommandString();

            Clipboard.SetDataObject(cmd);
            lstatus.Content = "ToscaCiClient CMD Copied to Clipboard!";
            
        }
    }
}