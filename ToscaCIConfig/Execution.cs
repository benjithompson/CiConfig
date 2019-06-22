using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToscaCIConfig
{
    internal class Execution
    {
        public Execution(string nodePath, string surrogateId)
        {
            this.NodePath = nodePath;
            this.SurrogateId = surrogateId;
        }
        public string NodePath { get; set; }
        public string SurrogateId { get; set; }
    }
}

