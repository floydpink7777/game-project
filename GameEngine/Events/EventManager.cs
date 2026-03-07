using GameEngine.Events.RuntimeNode;
using GameEngine.System.Core;
using System.Collections.Generic;

namespace GameEngine.Events
{
    public class EventManager
    {
        private readonly EventRunner _runner;

        public EventManager(EventRunner runner)
        {
            _runner = runner;
        }

        public bool IsFinished => _runner.IsFinished;

        // ★ NodeBase を返す
        public NodeBase NextNode() => _runner.NextNode();

        // ★ DialogueNode の処理
        public string GetSpeaker(DialogueNode node)
            => node.Speaker;

        public string GetText(DialogueNode node)
            => node.Text;

        // ★ ChoiceNode の処理
        public List<ChoiceOption> GetChoices(ChoiceNode node)
            => node.Options;

        // ★ 選択肢のジャンプ
        public void SelectChoice(GameSession session, int index)
        {
            var option = session.Choices[index];
            _runner.Jump(option.Jump);

            session.ClearChoices();
            session.ClearDialogue();
        }
    }
}