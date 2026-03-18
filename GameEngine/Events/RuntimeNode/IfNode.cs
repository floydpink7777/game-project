using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Events.RuntimeNode
{
    public class IfNode : NodeBase
    {
        public ConditionNode Condition { get; set; }
        public List<NodeBase> ThenBody { get; set; }
        public List<NodeBase>? ElseBody { get; set; }
    }
}
