using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptParser.Parser
{
    public class DialogueNode : IScriptNode
    {
        public string type => "dialogue";
        public string speaker { get; }
        public string text { get; }

        public DialogueNode(string speaker, string text)
        {
            this.speaker = speaker;
            this.text = text;
        }
    }
}
