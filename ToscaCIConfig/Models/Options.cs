using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ToscaCIConfig
{
    [Serializable]
    public class Options
    {
        
        public Options(string type, string name, string path)
        {
            Mode = type;
            Name = name;
            Path = path;
            IgnoreNonMatchingSurrogateIds = true;
            CleanOldResults = true;
            RemoteExecutionEndpoint = "";
            ReportPath = "";
            CiClientUsername = "";
            CiClientPassword = "";
        }

        public bool IgnoreNonMatchingSurrogateIds { get; set; }
        public bool CleanOldResults { get; set; }
        public string Mode { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string BuildRootFolder { get; set; }
        public string TestMandateName { get; set; }
        public string RemoteExecutionEndpoint { get; set; }
        public string ReportPath { get; set; }
        public string CiClientUsername { get; set; }
        public string CiClientPassword { get; set; }



    }
}
