using ScriptParser.Parser;                     // AST
using GameEngine.Events.RuntimeNode;           // RuntimeNode
using System.Linq;

namespace BuildTool.Converters
{
    public static class ConditionConverter
    {
        public static GameEngine.Events.RuntimeNode.ConditionNode Convert(ScriptParser.Parser.ConditionNode ast)
        {
            return new GameEngine.Events.RuntimeNode.ConditionNode
            {
                OrGroups = ast.OrGroups.Select(ConvertOr).ToList()
            };
        }

        private static GameEngine.Events.RuntimeNode.OrGroup ConvertOr(ScriptParser.Parser.OrGroup ast)
        {
            return new GameEngine.Events.RuntimeNode.OrGroup
            {
                AndGroups = ast.AndGroups.Select(ConvertAnd).ToList()
            };
        }

        private static GameEngine.Events.RuntimeNode.AndGroup ConvertAnd(ScriptParser.Parser.AndGroup ast)
        {
            return new GameEngine.Events.RuntimeNode.AndGroup
            {
                Terms = ast.Terms.Select(ConvertTerm).ToList()
            };
        }

        private static GameEngine.Events.RuntimeNode.SingleCondition ConvertTerm(ScriptParser.Parser.SingleCondition ast)
        {
            return ast switch
            {
                ScriptParser.Parser.ComparisonCondition c => new GameEngine.Events.RuntimeNode.ComparisonCondition
                {
                    Left = c.Left,
                    Operator = c.Operator,
                    RightValue = c.RightValue
                },

                ScriptParser.Parser.NotCondition n => new GameEngine.Events.RuntimeNode.NotCondition
                {
                    Target = ConvertTerm(n.Target)
                },

                _ => throw new System.Exception($"Unknown condition type: {ast.GetType().Name}")
            };
        }
    }
}