//using ScriptParser.Parser;
using GameEngine.Events.RuntimeNode;
using System;

namespace GameEngine.System.Evaluator
{
    public static class ConditionEvaluator
    {
        public static bool Evaluate(
            ConditionNode condition,
            VariableStore vars,
            ObjectEvaluator objEval)
        {
            foreach (var orGroup in condition.OrGroups)
            {
                if (EvaluateOrGroup(orGroup, vars, objEval))
                    return true;
            }
            return false;
        }

        private static bool EvaluateOrGroup(
            OrGroup orGroup,
            VariableStore vars,
            ObjectEvaluator objEval)
        {
            foreach (var andGroup in orGroup.AndGroups)
            {
                if (!EvaluateAndGroup(andGroup, vars, objEval))
                    return false;
            }
            return true;
        }

        private static bool EvaluateAndGroup(
            AndGroup andGroup,
            VariableStore vars,
            ObjectEvaluator objEval)
        {
            foreach (var cond in andGroup.Terms)
            {
                if (!EvaluateCondition(cond, vars, objEval))
                    return false;
            }
            return true;
        }

        private static bool EvaluateCondition(
            SingleCondition cond,
            VariableStore vars,
            ObjectEvaluator objEval)
        {
            return cond switch
            {
                ComparisonCondition c => EvaluateComparison(c, vars, objEval),
                NotCondition n => !EvaluateCondition(n.Target, vars, objEval),
                _ => throw new Exception("未知の条件要素です")
            };
        }

        private static bool EvaluateComparison(
            ComparisonCondition c,
            VariableStore vars,
            ObjectEvaluator objEval)
        {
            var left = ResolveValue(c.Left, vars, objEval);
            var right = c.RightValue;

            return c.Operator switch
            {
                "==" => Equals(left, right),
                "!=" => !Equals(left, right),
                ">" => Convert.ToInt32(left) > Convert.ToInt32(right),
                "<" => Convert.ToInt32(left) < Convert.ToInt32(right),
                ">=" => Convert.ToInt32(left) >= Convert.ToInt32(right),
                "<=" => Convert.ToInt32(left) <= Convert.ToInt32(right),
                _ => throw new Exception($"未知の比較演算子: {c.Operator}")
            };
        }

        private static object ResolveValue(
            string name,
            VariableStore vars,
            ObjectEvaluator objEval)
        {
            return objEval.EvaluatePath(name);
        }
    }
}