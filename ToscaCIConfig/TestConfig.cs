using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToscaCIConfig
{
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
}
