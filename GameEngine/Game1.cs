using FontStashSharp;
using GameEngine.Events;
using GameEngine.Events.RuntimeNode;
using GameEngine.GameData.DataStore;
using GameEngine.GameData.Npc;
using GameEngine.GameData.Player;
using GameEngine.System.Core;
using GameEngine.System.Evaluator;
using GameEngine.System.Input;
using GameEngine.System.State;
using GameEngine.UI;
using GameEngine.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System.IO;
using static GameEngine.System.GameConfig;

namespace GameEngine
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private EventRunner runner;
        private VariableStore vars;

        private InputManager _input;
        private UIManager _ui;
        private EventManager _events;
        private NodeExecutor _executor;

        private GameState _gameState = GameState.Idle;
        private GameSession _gameSession;

        private GameWorld _gameWorld;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _gameSession = new GameSession();

            vars = new VariableStore();

            _gameWorld = new GameWorld();
            var evaluator = new ObjectEvaluator(_gameWorld, vars);

            // 初期データの読み込み
            DataManager.Load();
            var a = PlayerInitValStore.Items["Orphan"];

            var player = new Player { Name = a.Name, Hp = 1000 };
            _gameWorld.Player = player;

            // NPC を登録
            _gameWorld.NPCs["alice"] = new NPC { Name = "ヒロイン", Hp = 20 };
            _gameWorld.NPCs["bob"] = new NPC { Name = "ボブ", Hp = 50 };

            // 公式変数（ゲームロジックと同期する）
            AutoBinder.Bind(_gameWorld.Player, "player", vars);
            foreach (var (id, npc) in _gameWorld.NPCs)
            {
                AutoBinder.Bind(npc, $"npc.{id}", vars);
            }

            var jsonText = File.ReadAllText("Content/Events/start.json");

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new NodeBaseConverter());
            settings.Converters.Add(new SingleConditionConverter());

            var eventData = JsonConvert.DeserializeObject<EventData>(jsonText, settings);

            runner = new EventRunner(eventData, vars);
            _input = new InputManager();
            _events = new EventManager(runner);
            _executor = new NodeExecutor(vars, evaluator);
            MessageWindowRenderer.InitDebug(GraphicsDevice);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            GameAssets.Load(Content, GraphicsDevice);
            _ui = new UIManager(_spriteBatch);
        }

        protected override void Update(GameTime gameTime)
        {
            _input.Update();
            _ui.Update(gameTime);

            switch (_gameState)
            {
                case GameState.WaitingForNext:
                    if (_input.Keyboard.Pressed(Keys.Enter))
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
                    return;
                case GameState.WaitingForChoice:

                    // カーソル移動
                    if (_input.Keyboard.Pressed(Keys.Up))
                    {
                        _ui.MoveCursor(-1);
                    }

                    if (_input.Keyboard.Pressed(Keys.Down))
                    {
                        _ui.MoveCursor(+1);
                    }

                    // Enter で決定
                    if (_input.Keyboard.Pressed(Keys.Enter))
                    {
                        SelectChoice(_ui.CursorIndex);
                    }

                    return;

                case GameState.Idle:
                    if (_events.IsFinished)
                    {
                        Exit();
                        return;
                    }

                    var node = _events.NextNode();
                    if (node == null)
                        return;

                    // NodeExecutor に一元化
                    _executor.ExecuteNode(node, _gameSession, runner);

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
                    break;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

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

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void SelectChoice(int index)
        {
            _events.SelectChoice(_gameSession, index);
            _gameState = GameState.Idle;
        }
    }
}
