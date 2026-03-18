using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptParser.Parser
{
    public class ConditionNode
    {
        public List<OrGroup> OrGroups { get; }

        public ConditionNode(List<OrGroup> orGroups)
        {
            OrGroups = orGroups;
        }
    }

    public class OrGroup
    {
        public List<AndGroup> AndGroups { get; }

        public OrGroup(List<AndGroup> andGroups)
        {
            AndGroups = andGroups;
        }
    }

    public class AndGroup
    {
        public List<SingleCondition> Terms { get; }

        public AndGroup(List<SingleCondition> terms)
        {
            Terms = terms;
        }
    }

    public abstract class SingleCondition { }

    public class ComparisonCondition : SingleCondition
    {
        public string Left { get; }
        public string Operator { get; }
        public string RightRaw { get; }   // リテラル or 変数名
        public object? RightValue { get; } // パース済み値（数値/文字列/bool）

        public ComparisonCondition(string left, string op, string rightRaw, object? rightValue)
        {
            Left = left;
            Operator = op;
            RightRaw = rightRaw;
            RightValue = rightValue;
        }
    }

    public class NotCondition : SingleCondition
    {
        public SingleCondition Target { get; }

        public NotCondition(SingleCondition target)
        {
            Target = target;
        }
    }
}
