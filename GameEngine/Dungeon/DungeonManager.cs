using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Dungeon
{
    public class DungeonManager
    {
        private TileMap _map;
        private Adventurer _adventurer;
        private Camera2D _camera;
        private Texture2D _playerTexture;
        private int _frame = 0;
        private int _screenWidth;
        private int _screenHeight;

        public DungeonManager(TileMap map, Adventurer adventurer, Texture2D playerTexture,
                              int screenWidth, int screenHeight)
        {
            _map = map;
            _adventurer = adventurer;
            _playerTexture = playerTexture;

            _screenWidth = screenWidth;
            _screenHeight = screenHeight;

            _camera = new Camera2D();
        }

        public void Update(GameTime gameTime)
        {
            _adventurer.Update();

            CollisionResolver.Resolve(_adventurer, _map);
            CollisionResolver.ClampToMap(_adventurer, _map);

            _camera.Follow(_adventurer.Position, _screenWidth, _screenHeight);

            // アニメーション
            bool moving = _adventurer.Velocity.LengthSquared() > 0;
            if (moving)
                _frame = (int)((gameTime.TotalGameTime.TotalMilliseconds / 150) % 3);
            else
                _frame = 1;
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Begin(transformMatrix: _camera.GetMatrix());

            _map.Draw(sb);

            int fw = 32;
            int fh = 32;
            int row = _adventurer.Direction;

            var src = new Rectangle(_frame * fw, row * fh, fw, fh);

            sb.Draw(_playerTexture, _adventurer.Position, src, Color.White);

            sb.End();
        }
    }
}
