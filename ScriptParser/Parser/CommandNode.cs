using ScriptParser.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptParser.Parser
{
    public class CommandNode : IScriptNode
    {
        public string type => "command";
        public string name { get; }
        public List<object> args { get; }

        public CommandNode(string name, List<object> args)
        {
            this.name = name;
            this.args = args;
        }
    }


}