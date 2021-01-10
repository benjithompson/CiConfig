using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Path = System.IO.Path;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Xml.Linq;
using ToscaCIConfig.Models;
using ToscaCIConfig.Views;
using Clipboard = System.Windows.Clipboard;
using ComboBox = System.Windows.Controls.ComboBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace ToscaCIConfig
{

    public partial class MainWindow : Window
    {
        public ConfigurationState State = new ConfigurationState();
        public Preferences Preference;

        public ObservableCollection<Options> DexCollection;
        public ObservableCollection<Options> RemoteCollection;
        public ObservableCollection<Options> LocalCollection;
        public ObservableCollection<string> PropertyNamesCollection;

        public MainWindow()
        {
            InitializeComponent();

            var mode = cbExecutionMode.Text;
            Preference = GetPreferences();

            SetListViewHeader(mode);

            //Mode and Configurations
            DexCollection = GetOptionsCollection("DEX");
            RemoteCollection = GetOptionsCollection("Remote");
            LocalCollection = GetOptionsCollection("Local");
            //InitConfigCollectionsFromConfigFile();
            InitTestConfigStateFromCollections();
            InitConfigsComboBoxItemSource();

            //ExecutionsCollection and Properties
            tbEvents.Content = mode + " Executions";
            PropertyNamesCollection = new ObservableCollection<string>();
            cbCustomProperties.ItemsSource = PropertyNamesCollection;
            lstatus.Foreground = Brushes.Green;
            lstatus.Content = "Ready!";
        }

        #region LocalMethods 

        private ObservableCollection<Options> GetOptionsCollection(string mode)
        {
            var filename = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\CiConfig\\" + mode + "_options.conf";
            if(File.Exists(filename))
            {
                using (Stream stream = File.Open(filename, FileMode.Open))
                {
                    var formatter = new BinaryFormatter();
                    ObservableCollection<Options> collection = (ObservableCollection<Options>)formatter.Deserialize(stream);
                    return collection;
                }
            }
            else
            {
                return new ObservableCollection<Options>();
            }

        }

        private void SetOptions(string mode, ObservableCollection<Options> collection)
        {
            var filename = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\CiConfig\\" + mode + "_options.conf";
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            using (Stream stream = File.Open(filename, FileMode.Create))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, collection);
            }
        }

        private void InitOptionsCollectionsFromConfigFile(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return;
            }
            lstatus.Foreground = Brushes.Green;
            lstatus.Content = "Loading Test Configurations...";
            //open folder or create if doesn't exist
            var dir = Path.GetDirectoryName(path);
            Directory.CreateDirectory(dir);
            foreach (string file in Directory.EnumerateFiles(dir, "*.xml"))
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
                    DexCollection.Add(new Options("DEX", configName, file, Preference.TestConfigurationsPath + "DEX_" + configName + ".xml"));
                }
                else if (configName.StartsWith("Remote"))
                {
                    configName = Helpers.RemoveExecutionMode(configName, "Remote");
                    RemoteCollection.Add(new Options("Remote", configName, file, "Remote_" + configName + ".xml"));
                }
                else if (configName.StartsWith("Local"))
                {
                    configName = Helpers.RemoveExecutionMode(configName, "Local");
                    LocalCollection.Add(new Options("Local", configName, file, "Local_" + configName + ".xml"));
                }
            }
        }

        private void InitConfigsComboBoxItemSource()
        {
            switch (cbExecutionMode.Text)
            {
                case "DEX":
                    cbConfigs.ItemsSource = DexCollection;
                    break;
                case "Remote":
                    cbConfigs.ItemsSource = RemoteCollection;
                    break;
                case "Local":
                    cbConfigs.ItemsSource = LocalCollection;
                    break;
            }
        }

        private void InitTestConfigStateFromCollections()
        {
            lstatus.Foreground = Brushes.Green;
            lstatus.Content = "Loading Test Configurations...";
            foreach (var testConfig in DexCollection)
            {
                var exNodeList =
                    Helpers.GetExecutionsNodeListFromTestConfigFile(Preference.TestConfigurationsPath, testConfig.Name,
                        testConfig.Mode);
                var exCollection = Helpers.GetExecutionCollectionFromNodeList(exNodeList);
                var propNodeList =
                    Helpers.GetPropertiesNodeListFromTestConfigFile(Preference.TestConfigurationsPath, testConfig.Name,
                        testConfig.Mode);
                var propCollection = Helpers.GetPropertyCollectionFromNodeList(propNodeList);
                
                //Todo: Handle SurrogateIDs
                State.setStateCollections("DEX", testConfig.Name, exCollection, propCollection);
            }

            foreach (var testConfig in RemoteCollection)
            {
                var exNodeList =
                    Helpers.GetExecutionsNodeListFromTestConfigFile(Preference.TestConfigurationsPath, testConfig.Name,
                        testConfig.Mode);
                var exCollection = Helpers.GetExecutionCollectionFromNodeList(exNodeList);
                var propNodeList =
                    Helpers.GetPropertiesNodeListFromTestConfigFile(Preference.TestConfigurationsPath, testConfig.Name,
                        testConfig.Mode);
                var propCollection = Helpers.GetPropertyCollectionFromNodeList(propNodeList);
                var surrogateList =
                    Helpers.GetSurrogateIdsNodeListFromTestConfigFile(Preference.TestConfigurationsPath, testConfig.Name, testConfig.Mode,
                        "surrogateIds");
                var surrogateIdsCollection = Helpers.GetSurrogateIdsCollectionFromNodeList(surrogateList);
                Helpers.SetTestConfigOptionsFromFile(testConfig);
                State.setStateCollections("Remote", testConfig.Name, exCollection, propCollection);
            }

            foreach (var testConfig in LocalCollection)
            {
                var exNodeList =
                    Helpers.GetExecutionsNodeListFromTestConfigFile(Preference.TestConfigurationsPath, testConfig.Name,
                        testConfig.Mode);
                var exCollection = Helpers.GetExecutionCollectionFromNodeList(exNodeList);
                var propNodeList =
                    Helpers.GetPropertiesNodeListFromTestConfigFile(Preference.TestConfigurationsPath, testConfig.Name,
                        testConfig.Mode);
                var propCollection = Helpers.GetPropertyCollectionFromNodeList(propNodeList);
                //Todo:get options
                Helpers.SetTestConfigOptionsFromFile(testConfig);
                State.setStateCollections("Local", testConfig.Name, exCollection, propCollection);
            }
        }

        private Preferences GetPreferences()
        {
            //get pref from serialization file

            
            try
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)+ "\\CiConfig\\preferences.conf";
                Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                IFormatter formatter = new BinaryFormatter();
                Preferences pf = (Preferences)formatter.Deserialize(stream);
                Console.WriteLine("Preferences Loaded:");
                Console.WriteLine(pf.ToscaCiClientPath);
                Console.WriteLine(pf.TestConfigurationsPath);
                return pf;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new Preferences(@"C:\Program Files (x86)\TRICENTIS\CIClient\ToscaCIClient.exe", @"C:\CiConfigs\");
            }
            
        }

        private string GetCiClientCommandString()
        {

            var mode = cbExecutionMode.Text;
            var name = cbConfigs.Text;
            var config = Helpers.GetOptions(mode, name);
            var cipath = (Preference.ToscaCiClientPath != "")? "\""+ Preference.ToscaCiClientPath+"\"" : @"'%TRICENTIS_HOME%\ToscaCI\Client\ToscaCIClient.exe'";
            var endpoint = (config.RemoteExecutionEndpoint != "")? " -e " + config.RemoteExecutionEndpoint: "";
            var reportpath = (config.ReportPath != "")? " -r " + "\""+ config.ReportPath + "\"": "";
            var username = (config.CiClientUsername != "")? " -l " + config.CiClientUsername: "";
            var password = (config.CiClientPassword != "")? " -p " + config.CiClientPassword: "";
            var configflag = " -c " + "\"" + Preference.TestConfigurationsPath + mode + "_" + name + ".xml" + "\"";
            mode = (mode == "Local") ? " -m local" : " -m distributed";

            return cipath + mode + configflag + endpoint + reportpath + username + password;
        }

        private void SetListViewHeader(string mode)
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

        public void SaveConfigFiles()
        {

            foreach (var config in DexCollection)
            {
                SaveConfigFile(config.Name, config.Mode);
            }

            foreach (var config in RemoteCollection)
            {
                SaveConfigFile(config.Name, config.Mode);
            }

            foreach (var config in LocalCollection)
            {
                SaveConfigFile(config.Name, config.Mode);
            }
            SetOptions("DEX", DexCollection);
            SetOptions("Remote", RemoteCollection);
            SetOptions("Local", LocalCollection);
        }

        //TODO: Handle file path creation and permission
        public void SaveConfigFile(string configName, string mode)
        {

            var config = Helpers.GetOptions(mode, configName);
            var el = State.GetExecutionsList(mode, configName);
            var prop = State.GetPropertiesList(mode, configName);
           
            XElement executions;
            XElement surrogateIds;

            if (!Directory.Exists(config.Path))
                Directory.CreateDirectory(config.Path);
            using (StreamWriter file =
                new StreamWriter(config.FilePath))
            {
                file.Write(Properties.Resources.ResourceManager.GetString(mode));
            }

            // load the XML file into an XElement
            XDocument doc = XDocument.Load(config.FilePath);

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
                        executions.Add(new XElement("ExecutionType", ex.execution));
                    }
                }
            }

            var customProperties = doc.Descendants("customProperties").First();
            if (customProperties != null)
            {
                foreach (var cp in prop)
                {
                    customProperties.Add(new XElement("property", cp.Value, new XAttribute("name", cp.Name)));
                }
            }

            if (config.Mode != "DEX")
            {
                try
                {
                    var option = doc.Descendants("ignoreNonMatchingIds").First();
                    option.Value = config.IgnoreNonMatchingSurrogateIds.ToString().ToLower();
                    option = doc.Descendants("cleanoldresults").First();
                    option.Value = config.CleanOldResults.ToString().ToLower();
                    option = doc.Descendants("buildrootfolder").First();
                    option.Value = config.BuildRootFolder;
                    option = doc.Descendants("testMandateName").First();
                    option.Value = config.TestMandateName;
                }
                catch (Exception e)
                {

                    Console.WriteLine(e.Message);
                }
            }
            doc.Save(config.FilePath);
            lstatus.Foreground = Brushes.Green;
            lstatus.Content = "Test Configurations Saved to " + config.FilePath;
        }
        #endregion

        #region Event Handlers

        private void NewConfig_OnClick(object sender, RoutedEventArgs e)
        {
            var mode = cbExecutionMode.Text;
            ObservableCollection<Options> configs = Helpers.GetTestConfigsCollection(mode);

            // Instantiate the dialog box
            NewConfigDialog dlg = new NewConfigDialog(configs) {Owner = this};

            // Open the dialog box modally 
            if ((bool) dlg.ShowDialog())
            {
                var configname = cbConfigs.Text;
                var path = Preference.TestConfigurationsPath;
                var filepath = Preference.TestConfigurationsPath + mode + "_" + configname + ".xml";
                configs.Add(new Options(mode, configname, path, filepath));

                State.setStateCollections(mode, configname, new ObservableCollection<Execution>(),
                    new ObservableCollection<CustomProperty>());
                cbConfigs.Text = "";
                lvExecutions.ItemsSource = State.GetExecutionsList(mode, configname);
                lvProperties.ItemsSource = State.GetPropertiesList(mode, configname);
                lstatus.Foreground = Brushes.Green;
                lstatus.Content = "Test Configuration '" + configname + "' created";
            }
        }

        private void RemoveConfig_OnClick(object sender, RoutedEventArgs e)
        {
            var mode = cbExecutionMode.Text;
            var configname = cbConfigs.Text;
            ObservableCollection<Options> configs = Helpers.GetTestConfigsCollection(mode);

            if (cbConfigs.Text != "")
            {
                MessageBoxResult messageBoxResult =
                    MessageBox.Show("Are you sure?", "Delete TestConfig?", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    
                    var match = Helpers.GetOptions(mode, configname);
                    if (match != null)
                    {
                        var path = match.Path + match.Mode + "_" + configname + ".xml";

                        try
                        {
                            File.Delete(path);
                            configs.Remove(match);
                            SetOptions("DEX", DexCollection);
                            SetOptions("Remote", RemoteCollection);
                            SetOptions("Local", LocalCollection);
                        }
                        catch (Exception)
                        {

                            throw;
                        }
                        
                    }

                    State.RemoveConfigListViewFromState(mode, configname);

                    lvExecutions.ItemsSource = null;
                    lvProperties.ItemsSource = null;
                    lstatus.Foreground = Brushes.Green;
                    lstatus.Content = "Test Configuration '" + configname + "' deleted";
                }
            }
        }

        private void CbConfigs_OnDropDownClosed(object sender, EventArgs e)
        {
            string mode = cbExecutionMode.Text;

            if (((sender as ComboBox).SelectedValue as Options) == null)
            {
                Console.WriteLine("Combo dropdown closed with no selected value");
                lvExecutions.ItemsSource = null;
                lvProperties.ItemsSource = null;
                return;
            }

            var name = ((sender as ComboBox).SelectedValue as Options).Name;
            Console.WriteLine("Combobox dropdown closed with value " + name);
            //cbConfigs.Text = configName;

            var exCollection = State.GetExecutionsList(mode, name);
            var propCollection = State.GetPropertiesList(mode, name);

            lvExecutions.ItemsSource = exCollection;
            lvProperties.ItemsSource = propCollection;
            SetListViewHeader(mode);
            lstatus.Foreground = Brushes.Green;
            lstatus.Content = "Test Configuration changed to '" + name + "'";
        }

        private void CbExecutionMode_OnDropDownClosed(object sender, EventArgs e)
        {
            Console.WriteLine("Dropdown closed with value " + cbExecutionMode.Text);

            var mode = cbExecutionMode.Text;

            //change configs list to only show configs of that type.
            InitConfigsComboBoxItemSource();
            var configname = cbConfigs.Text;
            tbEvents.Content = cbExecutionMode.Text + " Executions";
            lstatus.Foreground = Brushes.Green;
            lstatus.Content = "Test Configuration mode changed to " + mode;
            lvProperties.ItemsSource = State.GetPropertiesList(mode, configname);
            lvExecutions.ItemsSource = State.GetExecutionsList(mode, configname);
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
                lstatus.Foreground = Brushes.Green;
                lstatus.Content = "Property Removed from Combobox";
            }

            if (e.Key == Key.Enter)
            {
                Console.WriteLine(@"EnterKey Pressed on property combobox");
                if (!PropertyNamesCollection.Contains(cbCustomProperties.Text))
                {
                    PropertyNamesCollection.Add(cbCustomProperties.Text);
                    lstatus.Foreground = Brushes.Green;
                    lstatus.Content = "Property Name Added to Combobox";
                }
            }
        }

        private void SubmitExecution_OnClick(object sender, RoutedEventArgs e)
        {
            string executionText = tbExecution.Text;
            Console.WriteLine("submitExecution clicked with execution: " + executionText);
            var executionsList = State.GetExecutionsList(cbExecutionMode.Text, cbConfigs.Text);
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
                        lstatus.Foreground = Brushes.Green;
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
            var prop = State.GetPropertiesList(cbExecutionMode.Text, cbConfigs.Text);
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
                lstatus.Foreground = Brushes.Green;
                lstatus.Content = "Property Added";
            }
        }

        private void TbExecution_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SubmitExecution_OnClick(sender, e);
            }
            lstatus.Foreground = Brushes.Green;
            lstatus.Content = "Execution Added";
        }

        private void TbProperty_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && tbProperty.Text != "")
            {
                SubmitProperty_OnClick(sender, e);
            }
            lstatus.Foreground = Brushes.Green;
            lstatus.Content = "Property Added";
        }

        private void ButtonOptions_OnClick(object sender, RoutedEventArgs e)
        {
            //open options dialog
            if (cbConfigs.Text == "")
            {
                MessageBox.Show("Select a Configuration for Options.", "Error", MessageBoxButton.OK);
                return;
            }

            OptionsDialog dlg = new OptionsDialog();
            if (dlg.ShowDialog() == dlg.DialogResult)
            {
                SetOptions("DEX", DexCollection);
                SetOptions("Remote", RemoteCollection);
                SetOptions("Local", LocalCollection);
            }
        }

        private void LvExecutions_RemoveSelectedItems(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                var mode = cbExecutionMode.Text;
                var name = cbConfigs.Text;
                var exList = State.GetExecutionsList(mode, name);
                List<Execution> RemovedItems = new List<Execution>();
                foreach (Execution removedItem in lvExecutions.SelectedItems)
                {
                    RemovedItems.Add(removedItem);
                }

                foreach (var removedItem in RemovedItems)
                {
                    exList.Remove(removedItem);
                }
                lstatus.Foreground = Brushes.Green;
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
                var exList = State.GetPropertiesList(mode, name);
                List<CustomProperty> RemovedItems = new List<CustomProperty>();
                foreach (CustomProperty removedItem in lvProperties.SelectedItems)
                {
                    RemovedItems.Add(removedItem);
                }

                foreach (var removedItem in RemovedItems)
                {
                    exList.Remove(removedItem);
                }
                lstatus.Foreground = Brushes.Green;
                lstatus.Content = "Properties Removed";
                lvExecutions.Items.Refresh();
            }
        }

        private void ButtonCopy_OnClick(object sender, RoutedEventArgs e)
        {
            if (cbConfigs.Text == "")
            {
                lstatus.Foreground = Brushes.Red;
                lstatus.Content = "Select a Test Configuration!";
                return;
            }
            var cmd = GetCiClientCommandString();

            Clipboard.SetDataObject(cmd);
            lstatus.Foreground = Brushes.Green;
            lstatus.Content = "ToscaCiClient CMD Copied to Clipboard!";
        }

        private void MenuPreferences_OnClick(object sender, RoutedEventArgs e)
        {
            PreferencesDialog dlg = new PreferencesDialog(Preference);
            dlg.ShowDialog();
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            lstatus.Foreground = Brushes.Green;
            lstatus.Content = "Saving Configurations...";
            SaveConfigFiles();
            MessageBox.Show("Test Configs saved!", "File Saved", MessageBoxButton.OK);
        }

        private void ImportItem_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { InitialDirectory = (Directory.Exists(Preference.TestConfigurationsPath)) ? Preference.TestConfigurationsPath : @"C:\" };

            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.None)
            {
                var dir = dlg.FileName;
                InitOptionsCollectionsFromConfigFile(dir);
            }
        }

        private void OnExit_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Would you like to save your changes?", "Save Configurations", MessageBoxButton.YesNoCancel);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    ButtonOk_OnClick(sender, e);
                    break;
                case MessageBoxResult.No:
                    this.Close();
                    break;
                case MessageBoxResult.Cancel:
                    break;
            }
            return;
        }
    }
    #endregion
}