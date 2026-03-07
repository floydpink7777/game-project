using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Events.RuntimeNode
{
    public class RuntimeScene
    {
        public string Label { get; set; }

        public List<NodeBase> Nodes { get; set; }
    }

}
