using GameEngine.Events.RuntimeNode;
using GameEngine.System.Core;
using GameEngine.System.Evaluator;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace GameEngine.Events
{
    public class NodeExecutor
    {
        private readonly VariableStore _vars;

        private readonly Dictionary<string, Action<List<object>>> _commands;

        private readonly ObjectEvaluator _evaluator;

        public NodeExecutor(VariableStore vars, ObjectEvaluator evaluator)
        {
            this._vars = vars;
            this._evaluator = evaluator;

            _commands = new()
            {
                ["/set"] = ExecuteSet,
                ["/add"] = ExecuteAdd,
                ["/flag"] = ExecuteFlag,
                ["/end"] = args => { }, // 空振りさせる
                ["/jump"] = args => { } // 空振りさせる
            };
        }

        private void EnsureVariableExists(string key)
        {
            // 公式 → OK
            if (_vars.IsOfficial(key))
                return;

            // 非公式として既に存在 → OK
            if (_vars.HasValue(key))
                return;

            // 非公式変数として自動生成はしない
            throw new Exception($"Variable '{key}' does not exist. Use /set to create it.");
        }

        private string Expand(string text)
        {
            return Regex.Replace(text, @"@([a-zA-Z0-9_\.]+)", m =>
            {
                string key = m.Groups[1].Value;

                try
                {
                    var value = _evaluator.EvaluatePath(key);
                    return value?.ToString() ?? "";
                }
                catch
                {
                    // 未定義 → そのまま残す
                    return $"@{key}";
                }
            });
        }

        // DialogueNode の処理
        public void ExecuteDialogue(DialogueNode node, GameSession session)
        {
            session.Speaker = Expand(node.Speaker);
            session.Text = Expand(node.Text);
        }

        // ChoiceNode の処理
        public void ExecuteChoice(ChoiceNode node, GameSession session)
        {
            foreach (var opt in node.Options)
                opt.Text = Expand(opt.Text);

            session.Choices = node.Options;
        }

        // CommandNode の処理
        public void ExecuteCommand(CommandNode node)
        {
            if (_commands.TryGetValue(node.Name, out var action))
            {
                action(node.Args);
                return;
            }

            throw new Exception($"Unknown command '{node.Name}'. Args: {string.Join(", ", node.Args)}");
        }

        private void ExecuteSet(List<object> args)
        {
            RequireArgs(args, 2, "/set");

            string key = (string)args[0];
            object value = args[1];

            // 公式 → setter
            // 非公式 → 存在しなければ作る
            _vars.SetValue(key, value);
        }

        private void ExecuteAdd(List<object> args)
        {
            RequireArgs(args, 2, "/add");
            string key = (string)args[0];
            EnsureVariableExists(key);

            int amount = Convert.ToInt32(args[1]);
            int current = _vars.GetValue<int>(key);
            _vars.SetValue(key, current + amount);
        }

        private void ExecuteFlag(List<object> args)
        {
            RequireArgs(args, 2, "/flag");
            string key = (string)args[0];
            EnsureVariableExists(key);

            bool flag = Convert.ToBoolean(args[1]);
            _vars.SetValue(key, flag);
        }

        private void RequireArgs(List<object> args, int count, string command)
        {
            if (args.Count < count)
                throw new Exception($"{command} requires {count} arguments, but got {args.Count}");
        }

        public void ExecuteNode(NodeBase node, GameSession session, EventRunner runner)
        {
            switch (node)
            {
                case DialogueNode d:
                    ExecuteDialogue(d, session);
                    break;

                case ChoiceNode c:
                    ExecuteChoice(c, session);
                    break;

                case CommandNode cmd:
                    ExecuteCommand(cmd);
                    runner.ExecuteCommand(cmd); // ★ /end, /jump など
                    break;

                case IfNode i:
                    ExecuteIf(i, session, runner);
                    break;

                default:
                    throw new Exception($"Unknown node type: {node.GetType().Name}");
            }
        }

        public void ExecuteIf(IfNode node, GameSession session, EventRunner runner)
        {
            bool result = ConditionEvaluator.Evaluate(
                node.Condition,
                _vars,
                _evaluator
            );

            var list = result ? node.ThenBody : node.ElseBody;

            if (list == null || list.Count == 0)
                return;

            // ★ If の中身は「次のフレームで実行される」べきなので
            //    EventRunner に積むだけでよい
            runner.PushNodes(list);
        }
    }
}