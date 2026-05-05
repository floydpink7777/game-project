using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Dungeon
{
    public class DungeonScene
    {
        private DungeonManager _dungeonManager;
        private SlashEffect _slash;
        //private Camera2D _camera;
        private Adventurer _player;

        private KeyboardState _prevKeyboard;

        public bool PlayerDead => _dungeonManager.PlayerDead;
        public bool ReachedGoal => _dungeonManager.ReachedGoal;

        public DungeonScene(
            TileMap map,
            Adventurer player,
            Texture2D playerTex,
            Texture2D enemyTex,
            SlashEffect slash,
            int screenW,
            int screenH
        )
        {
            _player = player;
            _slash = slash;

            _dungeonManager = new DungeonManager(
                map,
                player,
                playerTex,
                enemyTex,
                screenW,
                screenH,
                slash
            );
        }

        public void Update(GameTime gameTime)
        {
            _dungeonManager.Update(gameTime);

            // 攻撃処理
            var kb = Keyboard.GetState();
            bool pressedNow = kb.IsKeyDown(Keys.Space) && !_prevKeyboard.IsKeyDown(Keys.Space);

            if (pressedNow)
            {
                Vector2 center = _player.Position + new Vector2(16, 16);
                float rot = 0f;
                Vector2 pos = center;

                switch (_player.Direction)
                {
                    case 3: pos = center + new Vector2(0, -32); rot = 0f; break;
                    case 2: pos = center + new Vector2(32, 0); rot = MathF.PI / 2; break;
                    case 1: pos = center + new Vector2(-32, 0); rot = -MathF.PI / 2; break;
                    case 0: pos = center + new Vector2(0, 32); rot = MathF.PI; break;
                }

                _slash.Play(pos, rot);
            }

            _prevKeyboard = kb;
            _slash.Update(gameTime);

            //_camera.Follow(_player.Position, 1280, 720);
        }

        public void Draw(SpriteBatch sb, GameTime gameTime)
        {
            _dungeonManager.Draw(sb, gameTime);

            sb.Begin(
                SpriteSortMode.Deferred,
                BlendState.Additive,
                transformMatrix: _dungeonManager.Camera.GetMatrix()
            );
            _slash.Draw(sb);
            sb.End();
        }
    }
}
