using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace ToscaCIConfig
{
    static class Helpers
    {
        public static bool MatchesExecutionPattern(string execution)
        {
            if(Regex.IsMatch(execution, "^(/.*).*$") || Regex.IsMatch(execution, "[0-9a-fA-F]{8}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{12}"))
            {
                return true;
            }else
            {
                return false;
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

        public static XmlNodeList getExecutionsListFromTestConfigFile(string configDir, string configname, string executionmode)
        {

            var configpath = configDir + executionmode + "_" + configname + ".xml";
            XmlDocument configDoc = new XmlDocument();
            try
            {
                configDoc.Load(configpath);
                XmlNodeList testEventsList = configDoc.GetElementsByTagName("TestEvent");
                for (int i = 0; i < testEventsList.Count; i++)
                {
                    Console.WriteLine(testEventsList[i].InnerText);
                }

                return testEventsList;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static XmlNodeList getPropertiesListFromTestConfigFile(string configDir, string configname, string executionmode)
        {

            var configpath = configDir + executionmode + "_" + configname + ".xml";
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

        public static ObservableCollection<Execution> getExecutionCollectionFromNodeList(XmlNodeList nodelist)
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

        public static ObservableCollection<CustomProperty> getPropertyCollectionFromNodeList(XmlNodeList nodelist)
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
    }
}
