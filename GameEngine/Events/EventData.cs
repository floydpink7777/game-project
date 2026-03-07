using GameEngine.Events.RuntimeNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Events
{
    public class EventData
    {
        public EventMeta Event { get; set; }

        public List<RuntimeScene> Scenes { get; set; }
    }

    public class EventMeta
    {
        public string EventID { get; set; }
        public string EventName { get; set; }
        public string EventType { get; set; }
        public List<string> Members { get; set; }
    }
}
