using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Generic;

namespace ScriptParser.Parser
{
    public class EventDefinition
    {
        public string? EventID { get; set; }
        public string? EventName { get; set; }
        public string? EventType { get; set; }
        public List<string> Members { get; set; } = new List<string>();
    }
}
