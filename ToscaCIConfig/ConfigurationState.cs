using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToscaCIConfig
{
    public class ConfigurationState
    {
        private string StateName;
        private Dictionary<string, ObservableCollection<Execution>> ExecutionCollectionDict =
            new Dictionary<string, ObservableCollection<Execution>>();
        private Dictionary<string, ObservableCollection<CustomProperty>> PropertyCollectionDict =
            new Dictionary<string, ObservableCollection<CustomProperty>>();

        //Constructor
        public ConfigurationState(string stateName)
        {
            this.StateName = stateName;
        }

        internal void setStateCollections(string mode, string configname, ObservableCollection<Execution> executions, ObservableCollection<CustomProperty> properties)
        {
            var configkey = GetDictKey(mode, configname);
            //add configname as key and current collections as values
            if (ExecutionCollectionDict.ContainsKey(configkey)){return;}
            if (PropertyCollectionDict.ContainsKey(configkey)){return;}

            ExecutionCollectionDict.Add(configkey, executions);
            PropertyCollectionDict.Add(configkey, properties);
        }

        internal void RemoveConfigListViewFromState(string mode, string configname)
        {
            ExecutionCollectionDict.Remove(GetDictKey(mode, configname));
            PropertyCollectionDict.Remove(GetDictKey(mode, configname));
        }

        internal ObservableCollection<Execution> GetExecutionsList(string mode, string configname)
        {
            if (ExecutionCollectionDict.ContainsKey(GetDictKey(mode, configname)))
            {
                return ExecutionCollectionDict[GetDictKey(mode, configname)];
            }

            return null;
        }

        internal ObservableCollection<CustomProperty> GetPropertiesList(string mode, string configname)
        {
            if (PropertyCollectionDict.ContainsKey(GetDictKey(mode, configname)))
            {
                return PropertyCollectionDict[GetDictKey(mode, configname)];
            }

            return null;
        }

        private string GetDictKey(string mode, string configname)
        {
            return "__." + mode + "_" + configname + "_";
        }
    }
}
