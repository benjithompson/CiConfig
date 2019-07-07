using System;

namespace ToscaCIConfig.Models
{
    [Serializable]
    public class Preferences
    {

        public string ToscaCiClientPath { get; set; }
        public string TestConfigurationsPath { get; set; }

        public Preferences(string toscaCiClientPath, string testConfigurationsPath)
        {
            this.ToscaCiClientPath = toscaCiClientPath;
            this.TestConfigurationsPath = testConfigurationsPath;
        }
    }
}
