using FontStashSharp;
using GameEngine.Dungeon;
using GameEngine.Events;
using GameEngine.Events.RuntimeNode;
using GameEngine.GameData.DataStore;
using GameEngine.GameData.Npc;
using GameEngine.GameData.Player;
using GameEngine.System.Core;
using GameEngine.System.Evaluator;
using GameEngine.System.Input;
using GameEngine.System.Logic;
using GameEngine.System.State;
using GameEngine.UI;
using GameEngine.UI.ADV;
using GameEngine.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Tiled;
using Newtonsoft.Json;
using SharpDX.Direct2D1;
using SharpDX.Direct3D9;
using SharpDX.MediaFoundation;
using System;
using System.IO;
using System.Security.Policy;
using static GameEngine.System.GameConfig;

namespace GameEngine
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private Microsoft.Xna.Framework.Graphics.SpriteBatch _spriteBatch;

        private VariableStore vars;

        private InputManager _input;
        private UIManager _ui;
        private EventManager _events;
        private NodeExecutor _executor;

        private GameSession _gameSession;

        private GameWorld _gameWorld;

        private GameLogic _logic;

        private TitleScreenRenderer _titleScreen;

        private ADVUIRenderer _advUI;

        // ダンジョン用クラス
        Texture2D _adventurerTex;
        Texture2D _enemyTex;

        private DungeonScene _dungeonScene;

        SlashEffect _slash;

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

            var player = new Player { Name = a.Name, Hp = 10, MaxHp = 10};
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

            var runner = new EventRunner(eventData, vars);

            _input = new InputManager();
            _events = new EventManager(runner);
            _executor = new NodeExecutor(vars, evaluator);
            MessageWindowRenderer.InitDebug(GraphicsDevice);

            _logic = new GameLogic(_events);

            _logic.GoToTitle();

            GameAssets.Init(_graphics.GraphicsDevice);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new Microsoft.Xna.Framework.Graphics.SpriteBatch(GraphicsDevice);
            GameAssets.Load(Content, GraphicsDevice);
            _ui = new UIManager(_spriteBatch);
            _titleScreen = new TitleScreenRenderer(_spriteBatch);

            _advUI = new ADVUIRenderer(_ui, _events, _executor, _gameSession);

            var tileset = Content.Load<Texture2D>("images/tileset");
            _adventurerTex = Content.Load<Texture2D>("images/adventurer1");

            _enemyTex = Content.Load<Texture2D>("images/enemy");

            Texture2D slashTex = Content.Load<Texture2D>("images/tktk_Sword_2");
            _slash = new SlashEffect(slashTex, 192, 192);

            _dungeonScene = new DungeonScene(
                _gameWorld,
                _adventurerTex,
                _enemyTex,
                tileset,
                _slash,
                _graphics.PreferredBackBufferWidth,
                _graphics.PreferredBackBufferHeight
            );
        }

        protected override void Update(GameTime gameTime)
        {
            _input.Update();
            _logic.Update();

            switch (_logic.Mode)
            {
                case GameMode.Title:
                    if (_titleScreen.Update(_logic, _input))
                    {
                        _dungeonScene.StartNewDungeon();

                        //_advUI.Start("start");
                    }
                    break;

                case GameMode.NewGame:
                    //_newGameScreen.Update(_logic);
                    break;

                case GameMode.MainGame:
                    //_mainGameScreen.Update(_logic);
                    break;
                case GameMode.Dungeon:

                    _dungeonScene.Update(gameTime);

                    if (_dungeonScene.PlayerDead)
                    {
                        _dungeonScene.StartNewDungeon();
                        _logic.GoToTitle();
                        return;
                    }

                    if (_dungeonScene.ReachedGoal)
                    {
                        _dungeonScene.NextFloor();
                    }

                    break;

            }

            _advUI.Update(gameTime, _input);

            if (_advUI.IsFinished)
            {
                _logic.OnScenarioFinished(_advUI.ScenarioId);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            switch (_logic.Mode)
            {
                case GameMode.Title:
                    _spriteBatch.Begin();
                    _titleScreen.Draw(FontManager.GetFont(FontID.Main, 35));
                    _spriteBatch.End();
                    break;

                case GameMode.NewGame:
                    //_newGameScreen.Draw(_fontLarge);
                    break;

                case GameMode.MainGame:
                    //_mainGameScreen.Draw(_fontLarge);
                    break;
                case GameMode.Dungeon:
                    _dungeonScene.Draw(_spriteBatch, gameTime);
                    break;
            }

            _advUI.Draw(_spriteBatch);

            base.Draw(gameTime);
        }
    }
}
