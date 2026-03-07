using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptParser.Parser
{
    public class SceneNode
    {
        public string Label { get; }
        public List<IScriptNode> Nodes { get; } = new();

        public SceneNode(string label)
        {
            Label = label;
        }
    }
}
