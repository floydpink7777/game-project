using FontStashSharp;
using GameEngine.System.Core;
using GameEngine.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Collisions.Layers;
using MonoGame.Extended.Tiled;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameEngine.System.GameConfig;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace GameEngine.Dungeon
{
    public class DungeonScene
    {
        private GameWorld _world;

        private Texture2D _playerTex;
        private Texture2D _enemyTex;
        private Texture2D _tileset;
        private SlashEffect _slash;

        private int _screenW;
        private int _screenH;

        private DungeonManager _manager;
        private KeyboardState _prevKeyboard;

        public bool PlayerDead => _manager.PlayerDead;
        public bool ReachedGoal => _manager.ReachedGoal;

        public DungeonScene(
            GameWorld world,
            Texture2D playerTex,
            Texture2D enemyTex,
            Texture2D tileset,
            SlashEffect slash,
            int screenW,
            int screenH
        )
        {
            _world = world;
            _playerTex = playerTex;
            _enemyTex = enemyTex;
            _tileset = tileset;
            _slash = slash;
            _screenW = screenW;
            _screenH = screenH;
        }

        // ★ ダンジョン開始（ゲーム開始 or ゲームオーバー）
        public void StartNewDungeon()
        {
            BuildDungeon(resetPlayer: true);
        }

        // ★ 次の階層へ
        public void NextFloor()
        {
            BuildDungeon(resetPlayer: false);
        }

        // ★ DungeonScene が持つべき ResetDungeon()
        private void BuildDungeon(bool resetPlayer)
        {
            // マップ生成
            var generator = new DungeonGenerator();
            var data = generator.CreateMap();
            var map = new TileMap(_tileset, data, 32);

            Adventurer player;

            if (resetPlayer)
            {
                player = new Adventurer()
                {
                    MaxHp = _world.Player.MaxHp,
                    Hp = _world.Player.Hp
                };
            }
            else
            {
                player = _manager.Adventurer;
            }

            player.SetPosition(data.StartPos.ToVector2() * map.TileSize);

            // DungeonManager を作り直す
            _manager = new DungeonManager(
                map,
                player,
                _playerTex,
                _enemyTex,
                _screenW,
                _screenH,
                _slash
            );
        }

        public void Update(GameTime gameTime)
        {
            _manager.Update(gameTime);

            // 攻撃処理
            var kb = Keyboard.GetState();
            bool pressedNow = kb.IsKeyDown(Keys.Space) && !_prevKeyboard.IsKeyDown(Keys.Space);

            var p = _manager.Adventurer;
            if (pressedNow)
            {
                
                Vector2 center = p.Position + new Vector2(16, 16);
                float rot = 0f;
                Vector2 pos = center;

                switch (p.Direction)
                {
                    case 3: pos = center + new Vector2(0, -32); rot = 0f; break;
                    case 2: pos = center + new Vector2(32, 0); rot = MathF.PI / 2; break;
                    case 1: pos = center + new Vector2(-32, 0); rot = -MathF.PI / 2; break;
                    case 0: pos = center + new Vector2(0, 32); rot = MathF.PI; break;
                }

                _slash.Play(pos, rot);
            }

            float speed = 10f; // 追従速度（大きいほど速い）
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            p.DisplayHp = MathHelper.Lerp(p.DisplayHp, p.Hp, speed * dt);

            _prevKeyboard = kb;
            _slash.Update(gameTime);
        }

        public void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, GameTime gameTime)
        {
            // ① ダンジョン描画
            _manager.Draw(sb, gameTime);

            // ② 斬撃エフェクト
            sb.Begin(
                SpriteSortMode.Deferred,
                BlendState.Additive,
                transformMatrix: _manager.Camera.GetMatrix()
            );
            _slash.Draw(sb);
            sb.End();

            // ③ HP 表示
            //sb.Begin();
            //var adv = _manager.Adventurer;

            //sb.DrawString(
            //    FontManager.GetFont(FontID.Main, 24),
            //    $"HP: {adv.Hp} / {adv.MaxHp}",
            //    new Vector2(20, 20),
            //    Microsoft.Xna.Framework.Color.White
            //);
            //sb.End();
            sb.Begin();

            var adv = _manager.Adventurer;

            // ① 数値表示（現在 / 最大）
            sb.DrawString(
                FontManager.GetFont(FontID.Main, 24),
                $"HP: {adv.Hp} / {adv.MaxHp}",
                new Vector2(20, 20),
                Color.White
            );

            // ② HPバー
            Vector2 barPos = new Vector2(20, 50);
            int barWidth = 120;
            int barHeight = 12;

            // HP割合（0～1）
            float ratio = adv.MaxHp > 0 ? adv.DisplayHp / adv.MaxHp : 0f;

            ratio = MathF.Round(ratio, 3);


            // 背景バー（黒）
            sb.Draw(
                GameAssets.WhiteTex,
                new Rectangle((int)barPos.X, (int)barPos.Y, barWidth, barHeight),
                Color.Black
            );

            // ★ 色判定（100%白 → 50%以下黄色 → 30%以下赤）
            Color barColor = Color.White;
            if (ratio <= 0.30f)
                barColor = Color.Red;
            else if (ratio <= 0.50f)
                barColor = Color.Yellow;

            // 背景バー（黒）
            sb.Draw(GameAssets.WhiteTex,
                new Rectangle((int)barPos.X, (int)barPos.Y, barWidth, barHeight),
                Color.Black);

            // 現在HPバー（割合 × 幅）
            sb.Draw(GameAssets.WhiteTex,
                new Rectangle((int)barPos.X, (int)barPos.Y, (int)(barWidth * ratio), barHeight),
                barColor);

            // 枠線（白）
            sb.Draw(GameAssets.WhiteTex, new Rectangle((int)barPos.X - 1, (int)barPos.Y - 1, barWidth + 2, 1), Color.White);
            sb.Draw(GameAssets.WhiteTex, new Rectangle((int)barPos.X - 1, (int)barPos.Y + barHeight, barWidth + 2, 1), Color.White);
            sb.Draw(GameAssets.WhiteTex, new Rectangle((int)barPos.X - 1, (int)barPos.Y - 1, 1, barHeight + 2), Color.White);
            sb.Draw(GameAssets.WhiteTex, new Rectangle((int)barPos.X + barWidth, (int)barPos.Y - 1, 1, barHeight + 2), Color.White);

            sb.End();
        }
    }

}
