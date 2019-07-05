using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ToscaCIConfig
{
    [Serializable()]
    public class TestConfig : ISerializable
    {
        public TestConfig(string type, string name, string path)
        {
            Mode = type;
            Name = name;
            Path = path;
            ignoreNonMatchingSurrogateIds = true;
            CleanOldResults = true;
            ToscaCiClientPath = "";
            RemoteExecutionEndpoint = "";
            ReportPath = "";
            CiClientUsername = "";
            CiClientPassword = "";
            CiTestConfigPath = "";
        }

        public TestConfig(SerializationInfo info, StreamingContext ctxt)
        {
            info.GetValue("Mode", typeof(string));
            info.GetValue("Name", typeof(string));
            info.GetValue("Path", typeof(string));
            info.GetValue("IgnoreNonMatchingSurrogateIds", typeof(bool));
            info.GetValue("CleanOldResults", typeof(bool));
            info.GetValue("ToscaCiClientPath", typeof(string));
            info.GetValue("RemoteExecutionEndpoint", typeof(string));
            info.GetValue("ReportPath", typeof(string));
            info.GetValue("CiClientUsername", typeof(string));
            info.GetValue("CiClientPassword", typeof(string));
            info.GetValue("CiTestConfigPath", typeof(string));
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

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Mode", Mode);
            info.AddValue("Name", Name);
            info.AddValue("Path", Path);
            info.AddValue("IgnoreNonMatchingSurrogateIds", ignoreNonMatchingSurrogateIds);
            info.AddValue("CleanOldResults", CleanOldResults);
            info.AddValue("ToscaCiClientPath", ToscaCiClientPath);
            info.AddValue("RemoteExecutionEndpoint", RemoteExecutionEndpoint);
            info.AddValue("ReportPath", ReportPath);
            info.AddValue("CiClientUsername", CiClientUsername);
            info.AddValue("CiClientPassword", CiClientPassword);
            info.AddValue("CiTestConfigPath", CiTestConfigPath);

        }
    }
}
