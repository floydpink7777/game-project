using GameEngine.Events.RuntimeNode;
using GameEngine.System.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Events
{
    public class EventRunner
    {
        private readonly EventData _event;
        private readonly VariableStore _vars;

        private int _sceneIndex = 0;
        private int _nodeIndex = 0;

        // ★ IfNode や Choice の後に挿入されるノードを保持するスタック
        private readonly Stack<NodeBase> _pending = new();

        public bool IsFinished =>
            _sceneIndex >= _event.Scenes.Count;

        public EventRunner(EventData data, VariableStore vars)
        {
            _event = data;
            _vars = vars;

            _sceneIndex = _event.Scenes.FindIndex(s => s.Label == "Start");
            if (_sceneIndex < 0)
                throw new Exception("Start シーンが存在しません");

            _nodeIndex = 0;
        }

        // ★ IfNode などからノードを積む
        public void PushNodes(IEnumerable<NodeBase> nodes)
        {
            // 逆順で積むことで、最初のノードが先に実行される
            foreach (var n in nodes.Reverse())
                _pending.Push(n);
        }

        // ★ 1ステップ進めて NodeBase を返す
        public NodeBase NextNode()
        {
            // まず pending を優先
            if (_pending.Count > 0)
                return _pending.Pop();

            if (IsFinished)
                return null;

            var scene = _event.Scenes[_sceneIndex];

            if (_nodeIndex >= scene.Nodes.Count)
            {
                _sceneIndex++;
                _nodeIndex = 0;
                return NextNode();
            }

            return scene.Nodes[_nodeIndex++];
        }

        public void Jump(string label)
        {
            int index = _event.Scenes.FindIndex(s => s.Label == label);
            if (index < 0)
                throw new Exception($"Scene '{label}' が存在しません");

            _sceneIndex = index;
            _nodeIndex = 0;

            // ★ ジャンプ時は pending をクリアする
            _pending.Clear();
        }

        public void ExecuteCommand(CommandNode cmd)
        {
            switch (cmd.Name)
            {
                case "/end":
                    _sceneIndex = _event.Scenes.Count;
                    break;

                case "/jump":
                    Jump(cmd.Args[0].ToString());
                    break;
            }
        }
    }
}