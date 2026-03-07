using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Events.RuntimeNode
{
    public class ChoiceNode : NodeBase
    {
        public List<ChoiceOption> Options { get; set; }
    }

    public class ChoiceOption
    {
        public string Text { get; set; }
        public string Jump { get; set; }
    }
}
