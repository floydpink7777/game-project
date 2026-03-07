using GameEngine.Events.RuntimeNode;
using GameEngine.System.Core;
using System;
using System.Collections.Generic;

namespace GameEngine.Events
{
    public class EventRunner
    {
        private readonly EventData _event;
        private readonly VariableStore _vars;

        private int _sceneIndex = 0;
        private int _nodeIndex = 0;

        public bool IsFinished =>
            _sceneIndex >= _event.Scenes.Count;

        public EventRunner(EventData data, VariableStore vars)
        {
            _event = data;
            _vars = vars;

            // Start シーンを探す
            _sceneIndex = _event.Scenes.FindIndex(s => s.Label == "Start");
            if (_sceneIndex < 0)
                throw new Exception("Start シーンが存在しません");

            _nodeIndex = 0;
        }

        // ★ 1ステップ進めて NodeBase を返す
        public NodeBase NextNode()
        {
            if (IsFinished)
                return null;

            var scene = _event.Scenes[_sceneIndex];

            if (_nodeIndex >= scene.Nodes.Count)
            {
                // シーン終了 → 次のシーンへ
                _sceneIndex++;
                _nodeIndex = 0;
                return NextNode();
            }

            return scene.Nodes[_nodeIndex++];
        }

        // ★ 選択肢でジャンプ
        public void Jump(string label)
        {
            int index = _event.Scenes.FindIndex(s => s.Label == label);
            if (index < 0)
                throw new Exception($"Scene '{label}' が存在しません");

            _sceneIndex = index;
            _nodeIndex = 0;
        }

        // ★ コマンド実行
        public void ExecuteCommand(CommandNode cmd)
        {
            switch (cmd.Name)
            {
                case "/end":
                    _sceneIndex = _event.Scenes.Count; // 終了扱い
                    break;

                case "/jump":
                    Jump(cmd.Args[0].ToString());
                    break;

                    // 必要なら追加
                    // case "/wait":
                    // case "/set":
                    // case "/voice":
            }
        }
    }
}