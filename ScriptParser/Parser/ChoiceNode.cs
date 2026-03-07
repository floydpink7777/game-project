using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptParser.Parser
{
    public class ChoiceNode : IScriptNode
    {
        public string type => "choice";
        public List<ChoiceOption> options { get; } = new();
    }

    public class ChoiceOption
    {
        public string text { get; }
        public string jump { get; }

        public ChoiceOption(string text, string jump)
        {
            this.text = text;
            this.jump = jump;
        }
    }
}
