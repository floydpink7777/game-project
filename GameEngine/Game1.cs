using FontStashSharp;
using GameEngine.Events;
using GameEngine.Events.RuntimeNode;
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
using System.CodeDom.Compiler;
using System.Collections.Generic;
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
            // TODO: Add your initialization logic here
            _gameSession = new GameSession();

            vars = new VariableStore();

            _gameWorld = new GameWorld();
            var evaluator = new ObjectEvaluator(_gameWorld, vars);
            var player = new Player { Name = "トニー", Hp = 1000 };
            _gameWorld.Player = player;

            // NPC を登録
            _gameWorld.NPCs["alice"] = new NPC { Name = "アリス", Hp = 20 };
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

            var eventData = JsonConvert.DeserializeObject<EventData>(jsonText, settings);

            runner = new EventRunner(eventData, vars);
            _input = new InputManager();
            _events = new EventManager(runner);
            _executor = new NodeExecutor(vars, evaluator);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _ui = new UIManager(_spriteBatch);

            GameAssets.Load(Content, GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            _input.Update();

            switch (_gameState)
            {
                case GameState.WaitingForNext:
                    if (_input.Keyboard.Pressed(Keys.Enter))
                        _gameState = GameState.Idle;
                    return;

                case GameState.WaitingForChoice:
                    if (_input.Keyboard.Pressed(Keys.D1)) SelectChoice(0);
                    if (_input.Keyboard.Pressed(Keys.D2)) SelectChoice(1);
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

                    switch (node)
                    {
                        case DialogueNode d:
                            _executor.ExecuteDialogue(d, _gameSession);
                            _gameState = GameState.WaitingForNext;
                            break;

                        case ChoiceNode c:
                            _executor.ExecuteChoice(c, _gameSession);
                            _gameState = GameState.WaitingForChoice;
                            break;

                        case CommandNode cmd:
                            _executor.ExecuteCommand(cmd);
                            runner.ExecuteCommand(cmd);
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
                    _ui.DrawChoices(_gameSession.Choices, FontManager.GetFont(FontID.Main,48));
                    break;

                default:
                    _ui.DrawDialogue(_gameSession.Speaker, _gameSession.Text, FontManager.GetFont(FontID.Main, 48));
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
