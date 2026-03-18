using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Events.RuntimeNode
{
    public class ConditionNode
    {
        public List<OrGroup> OrGroups { get; set; } = new();
    }

    public class OrGroup
    {
        public List<AndGroup> AndGroups { get; set; } = new();
    }

    public class AndGroup
    {
        public List<SingleCondition> Terms { get; set; } = new();
    }

    // ---- 単一条件の基底 ----
    public abstract class SingleCondition { }

    // ---- 比較条件 ----
    public class ComparisonCondition : SingleCondition
    {
        public string Left { get; set; } = "";
        public string Operator { get; set; } = "";
        public object? RightValue { get; set; }
    }

    // ---- NOT 条件 ----
    public class NotCondition : SingleCondition
    {
        public SingleCondition Target { get; set; }
    }
}
