using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Events.RuntimeNode
{
    public class CommandNode : NodeBase
    {
        public string Name { get; set; }
        public List<object> Args { get; set; }

        //public CommandNode(string name, List<object> args)
        //{
        //    Name = name;
        //    Args = args;
        //}
    }
}

