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
            Mode = type;
            Name = name;
            Path = path;
            ignoreNonMatchingSurrogateIds = true;
            CleanOldResults = true;
            ToscaCiClientPath = "\"C:\\Program Files (x86)\\TRICENTIS\\Tosca Testsuite\\ToscaCI\\Client\\ToscaCIClient.exe\"";
            RemoteExecutionEndpoint = "";
            ReportPath = "";
            CiClientUsername = "";
            CiClientPassword = "";
            CiTestConfigPath = @"C:\CiConfigs\";
        }

        public bool ignoreNonMatchingSurrogateIds { get; set; }
        public bool CleanOldResults { get; set; }
        public string Mode { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string BuildRootFolder { get; set; }
        public string TestMandateName { get; set; }
        public string ToscaCiClientPath { get; set; }
        public string RemoteExecutionEndpoint { get; set; }
        public string ReportPath { get; set; }
        public string CiClientUsername { get; set; }
        public string CiClientPassword { get; set; }
        public string CiTestConfigPath { get; set; }

    }
}
