using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptParser.Parser
{
    public class IfNode : IScriptNode
    {
        public string type => "ifNode";
        public ConditionNode condition { get; }
        public List<IScriptNode> thenBody { get; }
        public List<IScriptNode>? elseBody { get; }

        public IfNode(
            ConditionNode condition,
            List<IScriptNode> thenBody,
            List<IScriptNode>? elseBody)
        {
            this.condition = condition;
            this.thenBody = thenBody;
            this.elseBody = elseBody;
        }
    }
}
