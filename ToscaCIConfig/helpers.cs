using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace ToscaCIConfig
{
    static class Helpers
    {

        public static TestConfig GetTestConfig(string mode, string configName)
        {
            ObservableCollection<TestConfig> testConfigs = GetTestConfigsCollection(mode);
            try
            {
                var match = testConfigs.First(p => (p.Mode == mode && p.Name == configName));
                return match;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        public static ObservableCollection<TestConfig> GetTestConfigsCollection(string mode)
        {
            try { 
            
                ObservableCollection<TestConfig> configs;

                switch (mode)
                {
                    case "DEX":
                        configs = ((MainWindow)Application.Current.MainWindow)?.DexConfigsCollection;
                        break;
                    case "Remote":
                        configs = ((MainWindow)Application.Current.MainWindow)?.RemoteConfigsCollection;
                        break;
                    case "Local":
                        configs = ((MainWindow)Application.Current.MainWindow)?.LocalConfigsCollection;
                        break;
                    default:
                        Console.WriteLine(@"No compatible Execution Mode for implemented exceptions");
                        configs = null;
                        break;
                }

                return configs;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }

        public static string RemoveExecutionMode(string filename, string exType)
        {
            switch (exType)
            {
                case "DEX":
                    filename = filename.Substring(4, filename.Length - 4);
                    break;
                case "Remote":
                    filename = filename.Substring(7, filename.Length - 7);
                    break;
                case "Local":
                    filename = filename.Substring(6, filename.Length - 6);
                    break;
            }
            return filename;
        }

        //Todo: SurrogateIDs, ignoreNonMatchingIds, buildrootfolder, cleanoldresults, testmandatename
        public static XmlNodeList getExecutionsNodeListFromTestConfigFile(string dir, string name, string mode)
        {

            var configpath = dir + mode + "_" + name + ".xml";
            XmlNodeList exList = null;
            XmlDocument configDoc = new XmlDocument();
            try
            {
                configDoc.Load(configpath);
                if (mode == "DEX")
                {
                    exList = configDoc.GetElementsByTagName("TestEvent");
                    for (int i = 0; i < exList.Count; i++)
                    {
                        Console.WriteLine(exList[i].InnerText);
                    }
                }
                else
                {
                    exList = configDoc.GetElementsByTagName("ExecutionTypes");
                    for (int i = 0; i < exList.Count; i++)
                    {
                        Console.WriteLine(exList[i].InnerText);
                    }
                }  
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return exList;
        }

        public static void SetTestConfigOptionsFromNodeList(TestConfig config)
        {

        }
        public static XmlNodeList getSurrogateIdsNodeListFromTestConfigFile(string dir, string name, string mode, string tag)
        {

            var configpath = dir + mode + "_" + name + ".xml";
            XmlNodeList surrogateList = null;
            XmlDocument configDoc = new XmlDocument();
            try
            {
                configDoc.Load(configpath);
                if (mode != "DEX")
                {
                    surrogateList = configDoc.GetElementsByTagName(tag);
                    for (int i = 0; i < surrogateList.Count; i++)
                    {
                        Console.WriteLine(surrogateList[i].InnerText);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return surrogateList;

        }
        

        public static XmlNodeList getPropertiesNodeListFromTestConfigFile(string dir, string name, string mode)
        {

            var configpath = dir + mode + "_" + name + ".xml";
            XmlDocument configDoc = new XmlDocument();
            try
            {
                configDoc.Load(configpath);
                XmlNodeList customPropertyList = configDoc.GetElementsByTagName("property");
                for (int i = 0; i < customPropertyList.Count; i++)
                {
                    Console.WriteLine(customPropertyList[i].InnerText);
                }

                return customPropertyList;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static void setTestConfigOptionsFromFile(TestConfig config)
        {

            string dir = config.Path;
            string mode = config.Mode;

            XmlNodeList modeNode = null;
            XmlDocument xml = new XmlDocument();
            try
            {
                xml.Load(dir);
                if (mode != "DEX")
                {
                    modeNode = xml.GetElementsByTagName("ignoreNonMatchingIds");
                    config.ignoreNonMatchingSurrogateIds = modeNode[0].InnerText.ToLower() == "true";
                    modeNode = xml.GetElementsByTagName("cleanoldresults");
                    config.CleanOldResults = modeNode[0].InnerText.ToLower() == "true";
                    modeNode = xml.GetElementsByTagName("buildrootfolder");
                    config.BuildRootFolder = modeNode[0].InnerText;
                    modeNode = xml.GetElementsByTagName("testMandateName");
                    config.TestMandateName = modeNode[0].InnerText;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static ObservableCollection<Execution> GetExecutionCollectionFromNodeList(XmlNodeList nodelist)
        {
            var c = new ObservableCollection<Execution>();
            for (int i = 0; i < nodelist.Count; i++)
            {
                if (nodelist[i].InnerText != "")
                {
                    c.Add(new Execution(nodelist[i].InnerText));
                }
            }
            return c;
        }

        public static ObservableCollection<SurrogateId> GetSurrogateIdsCollectionFromNodeList(XmlNodeList nodelist)
        {
            var c = new ObservableCollection<SurrogateId>();
            for (int i = 0; i < nodelist.Count; i++)
            {
                if (nodelist[i].InnerText != "")
                {
                    c.Add(new SurrogateId(nodelist[i].InnerText));
                }
            }
            return c;
        }

        public static ObservableCollection<CustomProperty> GetPropertyCollectionFromNodeList(XmlNodeList nodelist)
        {
            var c = new ObservableCollection<CustomProperty>();
            for (int i = 0; i < nodelist.Count; i++)
            {
                if (nodelist[i].InnerText != "")
                {
                    c.Add(new CustomProperty(nodelist[i].Attributes["name"].Value,nodelist[i].InnerText));
                }
            }
            return c;
        }

        public static void writeTestConfigXmlFromList(string dir, string configName, string mode, ObservableCollection<Execution> el, ObservableCollection<CustomProperty> prop)
        {
            foreach (var executionObj in el)
            {
                writeListViewsToTestConfigFile(executionObj.execution);
            }
            foreach (var propertyObj in prop)
            {
                writeListViewsToTestConfigFile(propertyObj.Name, propertyObj.Value);
            }
        }

        private static void writeListViewsToTestConfigFile(string executionid)
        {
            
            Console.WriteLine("todo: write execution string to xml");
        }

        private static void writeListViewsToTestConfigFile(string propertyName, string propertyValue)
        {
            Console.WriteLine("todo: write propertyname and value to xml");
        }
        public static bool ExecutionPatternIsValid(string ex)
        {
            return (IsNodePath(ex) || IsSurrogateId(ex));
        }

        public static bool IsNodePath(string exStr)
        {
            return Regex.IsMatch(exStr, "^(/.*).*$");
        }

        public static bool IsSurrogateId(string exStr)
        {
            return Regex.IsMatch(exStr, "[0-9a-fA-F]{8}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{12}");
        }
    }
}
