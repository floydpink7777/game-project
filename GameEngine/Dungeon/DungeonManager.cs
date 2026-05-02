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

        public bool ReachedGoal { get; private set; } = false;


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

            var tileX = (int)(_adventurer.Position.X / _map.TileSize);
            var tileY = (int)(_adventurer.Position.Y / _map.TileSize);

            if (tileX == _map.TileMapData.GoalPos.X &&
                tileY == _map.TileMapData.GoalPos.Y)
            {
                ReachedGoal = true;
            }

            UpdateFog();
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Begin(transformMatrix: _camera.GetMatrix());

            var cameraView = _camera.GetViewRectangle(_screenWidth, _screenHeight);
            _map.Draw(sb, cameraView);

            int fw = 32;
            int fh = 32;
            int row = _adventurer.Direction;

            var src = new Rectangle(_frame * fw, row * fh, fw, fh);
            sb.Draw(_playerTexture, _adventurer.Position, src, Color.White);

            sb.End();

            // ★ ミニマップはカメラ行列なしで描画
            sb.Begin();
            DrawMiniMap(sb);
            sb.End();
        }

        private void UpdateFog()
        {
            int radius = 7; // 視界半径（調整可）

            int px = (int)(_adventurer.Position.X / _map.TileSize);
            int py = (int)(_adventurer.Position.Y / _map.TileSize);

            var fog = _map.TileMapData.Fog;

            // Visible → Seen に落とす
            for (int y = 0; y < _map.TileMapData.Height; y++)
                for (int x = 0; x < _map.TileMapData.Width; x++)
                    if (fog[x, y] == Visibility.Visible)
                        fog[x, y] = Visibility.Seen;

            // 現在視界を Visible に
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    int tx = px + dx;
                    int ty = py + dy;

                    if (tx < 0 || ty < 0 || tx >= _map.TileMapData.Width || ty >= _map.TileMapData.Height)
                        continue;

                    if (dx * dx + dy * dy <= radius * radius)
                        fog[tx, ty] = Visibility.Visible;
                }
            }
        }

        private void DrawMiniMap(SpriteBatch sb)
        {
            int miniTile = 3;
            int offsetX = _screenWidth - (_map.TileMapData.Width * miniTile) - 20;
            int offsetY = 20;

            var fog = _map.TileMapData.Fog;
            var tiles = _map.TileMapData.Tiles;
            var data = _map.TileMapData;

            for (int y = 0; y < data.Height; y++)
            {
                for (int x = 0; x < data.Width; x++)
                {
                    // ★ 未探索は非表示
                    if (fog[x, y] == Visibility.Unseen)
                        continue;

                    Color c;

                    if (tiles[x, y] == 6) // 壁
                    {
                        c = new Color(30, 30, 30); // 壁（濃いグレー）
                    }
                    else
                    {
                        if (data.IsRoomTile(x, y))
                            c = new Color(100, 230, 150); // 部屋（緑）
                        else
                            c = new Color(140, 200, 255); // 通路（水色）
                    }

                    sb.Draw(
                        _playerTexture,
                        new Rectangle(offsetX + x * miniTile, offsetY + y * miniTile, miniTile, miniTile),
                        c
                    );
                }
            }

            // ★ プレイヤー位置（赤）
            int px = (int)(_adventurer.Position.X / _map.TileSize);
            int py = (int)(_adventurer.Position.Y / _map.TileSize);

            sb.Draw(
                _playerTexture,
                new Rectangle(offsetX + px * miniTile, offsetY + py * miniTile, miniTile, miniTile),
                Color.Red
            );

            // ★ ゴールは視界に入ったら表示
            var goal = data.GoalPos;
            var goalFog = fog[goal.X, goal.Y];

            if (goalFog == Visibility.Visible || goalFog == Visibility.Seen)
            {
                sb.Draw(
                    _playerTexture,
                    new Rectangle(offsetX + goal.X * miniTile, offsetY + goal.Y * miniTile, miniTile, miniTile),
                    Color.Yellow
                );
            }
        }

    }
}
