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

        }

        public bool ignoreNonMatchingSurrogateIds { get; set; }
        public bool CleanOldResults { get; set; }
        public string Mode { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string BuildRootFolder { get; set; }
        public string TestMandateName { get; set; }

    }
}
