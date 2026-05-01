using GameEngine.Events;
using GameEngine.Events.RuntimeNode;
using GameEngine.System.Core;
using GameEngine.System.Input;
using GameEngine.System.State;
using GameEngine.UI;
using GameEngine.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Text;
using static GameEngine.System.GameConfig;

namespace GameEngine.UI.ADV
{
    public class ADVUIRenderer
    {
        private readonly UIManager _ui;
        private readonly EventManager _events;
        private readonly NodeExecutor _executor;
        private readonly GameSession _gameSession;

        private bool _isActive = false;
        private GameState _gameState = GameState.Idle;

        public bool IsActive => _isActive;

        public bool IsFinished { get; private set; } = false;

        private string _scenarioId;
        public string ScenarioId => _scenarioId;

        public ADVUIRenderer(
            UIManager ui,
            EventManager events,
            NodeExecutor executor,
            GameSession session)
        {
            _ui = ui;
            _events = events;
            _executor = executor;
            _gameSession = session;
        }

        // -------------------------
        // ADV の開始・終了
        // -------------------------
        public void Start(string scenarioId)
        {
            _scenarioId = scenarioId;
            _isActive = true;
            IsFinished = false;
            _gameState = GameState.Idle;
        }

        public void Stop()
        {
            _isActive = false;
            IsFinished = true;
        }

        // -------------------------
        // Update（ADV がアクティブな時だけ）
        // -------------------------
        public void Update(GameTime gameTime, InputManager input)
        {
            if (!_isActive)
                return;

            _ui.Update(gameTime);

            switch (_gameState)
            {
                case GameState.Idle:
                    RunNextNode();
                    break;

                case GameState.WaitingForNext:
                    HandleNext(input);
                    break;

                case GameState.WaitingForChoice:
                    HandleChoice(input);
                    break;
            }
        }

        // -------------------------
        // Draw（ADV がアクティブな時だけ）
        // -------------------------
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_isActive)
                return;

            spriteBatch.Begin();

            switch (_gameState)
            {
                case GameState.WaitingForChoice:
                    _ui.DrawChoices(
                        FontManager.GetFont(FontID.Main, 24)
                    );
                    break;

                default:
                    _ui.DrawDialogue(
                        FontManager.GetFont(FontID.Main, 24)
                    );
                    break;
            }

            spriteBatch.End();
        }

        // -------------------------
        // ノード実行（後で実装）
        // -------------------------
        private void RunNextNode()
        {
            if (_events.IsFinished)
            {
                Stop();
                return;
            }

            var node = _events.NextNode();
            if (node == null)
                return;

            // NodeExecutor に一元化
            _events.ExecuteNode(node, _gameSession, _executor);

            // UI 状態遷移だけ Game1 が担当
            switch (node)
            {
                case DialogueNode:
                    _ui.SetDialogue(_gameSession.Speaker, _gameSession.Text, FontManager.GetFont(FontID.Main, 24));
                    _gameState = GameState.WaitingForNext;
                    break;
                case ChoiceNode choiceNode:
                    _ui.SetChoices(_gameSession.Choices);
                    _gameState = GameState.WaitingForChoice;
                    break;
            }
        }

        private void HandleNext(InputManager input)
        {
            if (input.Keyboard.Pressed(Keys.Enter))
            {
                // ① まだタイプ途中なら全文表示
                if (!_ui.IsPageComplete)
                {
                    _ui.SkipToPageEnd();
                    return;
                }

                // ② ページが終わっていて、次ページがあるならページ送り
                if (_ui.NextPage())
                    return;

                // ③ 最終ページなら次のノードへ
                _gameState = GameState.Idle;
            }
        }

        private void HandleChoice(InputManager input)
        {
            // カーソル移動
            if (input.Keyboard.Pressed(Keys.Up))
            {
                _ui.MoveCursor(-1);
            }

            if (input.Keyboard.Pressed(Keys.Down))
            {
                _ui.MoveCursor(+1);
            }

            // Enter で決定
            if (input.Keyboard.Pressed(Keys.Enter))
            {
                _events.SelectChoice(_gameSession, _ui.CursorIndex);
                _gameState = GameState.Idle;
            }
        }
    }
}
