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
        private readonly Dictionary<string, ObservableCollection<Execution>> _executionCollectionDict =
            new Dictionary<string, ObservableCollection<Execution>>();
        private readonly Dictionary<string, ObservableCollection<CustomProperty>> _propertyCollectionDict =
            new Dictionary<string, ObservableCollection<CustomProperty>>();

        internal void setStateCollections(string mode, string configname, ObservableCollection<Execution> executions, ObservableCollection<CustomProperty> properties)
        {
            var configkey = GetDictKey(mode, configname);
            //add configname as key and current collections as values
            if (_executionCollectionDict.ContainsKey(configkey)){return;}
            if (_propertyCollectionDict.ContainsKey(configkey)){return;}

            _executionCollectionDict.Add(configkey, executions);
            _propertyCollectionDict.Add(configkey, properties);
        }

        internal void RemoveConfigListViewFromState(string mode, string configname)
        {
            _executionCollectionDict.Remove(GetDictKey(mode, configname));
            _propertyCollectionDict.Remove(GetDictKey(mode, configname));
        }

        internal ObservableCollection<Execution> GetExecutionsList(string mode, string configname)
        {
            if (_executionCollectionDict.ContainsKey(GetDictKey(mode, configname)))
            {
                return _executionCollectionDict[GetDictKey(mode, configname)];
            }

            return null;
        }

        internal ObservableCollection<CustomProperty> GetPropertiesList(string mode, string configname)
        {
            if (_propertyCollectionDict.ContainsKey(GetDictKey(mode, configname)))
            {
                return _propertyCollectionDict[GetDictKey(mode, configname)];
            }

            return null;
        }

        private string GetDictKey(string mode, string configname)
        {
            return "__." + mode + "_" + configname + "_";
        }
    }
}
