using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Events.RuntimeNode
{
    public class DialogueNode : NodeBase
    {
        public string Speaker { get; set; }
        public string Text { get; set; }
        //public string Emotion { get; set; } // enum にしても良い
    }
}
