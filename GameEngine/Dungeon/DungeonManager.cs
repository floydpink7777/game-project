using GameEngine.System.Core;
using GameEngine.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using System.Collections.Generic;
using static GameEngine.System.GameConfig;

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

        public bool PlayerDead { get; private set; } = false;

        public bool ReachedGoal { get; private set; } = false;

        List<Enemy> _enemies;

        private SlashEffect _slash;

        public Camera2D Camera => _camera;

        public Adventurer Adventurer => _adventurer;

        private List<DamagePopup> _popups = new();

        public DungeonManager(TileMap map, Adventurer adv, Texture2D advTex, Texture2D enemyTex, int w, int h, SlashEffect slash)
        {
            _map = map;
            _adventurer = adv;
            _playerTexture = advTex;
            _screenWidth = w;
            _screenHeight = h;
            _slash = slash;
            _camera = new Camera2D();

            _enemies = new List<Enemy>();
            foreach (var pos in map.TileMapData.Enemies)
                _enemies.Add(new Enemy(pos, enemyTex));
        }

        public void ResetPlayerPosition(Vector2 pos)
        {
            _adventurer.SetPosition(pos);
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

            // ★ ゴールとの当たり判定（矩形）
            Rectangle playerRect = new Rectangle(
                (int)_adventurer.Position.X,
                (int)_adventurer.Position.Y,
                32, 32
            );

            Rectangle goalRect = new Rectangle(
                _map.TileMapData.GoalPos.X * _map.TileSize,
                _map.TileMapData.GoalPos.Y * _map.TileSize,
                _map.TileSize,
                _map.TileSize
            );

            if (playerRect.Intersects(goalRect))
            {
                ReachedGoal = true;
            }

            UpdateFog();

            // 無敵時間の減少
            if (_adventurer.InvincibleTime > 0)
                _adventurer.InvincibleTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;


            foreach (var enemy in _enemies)
            {
                enemy.Update(gameTime, _adventurer.Position);

                Rectangle enemyRect = new Rectangle(
                    (int)enemy.Position.X,
                    (int)enemy.Position.Y,
                    32, 32
                );

                if (playerRect.Intersects(enemyRect))
                {
                    OnEnemyHit(enemy);
                    break;
                }
            }


            // ★ 斬撃が再生中なら当たり判定
            if (_slash.IsPlaying)
            {
                Rectangle slashBox = _slash.Hitbox;

                for (int i = _enemies.Count - 1; i >= 0; i--)
                {
                    var enemy = _enemies[i];

                    Rectangle enemyRect = new Rectangle(
                        enemy.TilePos.X * _map.TileSize,
                        enemy.TilePos.Y * _map.TileSize,
                        32, 32
                    );

                    if (slashBox.Intersects(enemyRect))
                    {
                        // ★ 敵ダメージポップ生成
                        Vector2 pos = enemy.Position + new Vector2(16, -10);
                        _popups.Add(new DamagePopup(pos, 1, Color.White));

                        _enemies.RemoveAt(i);   // ★ 敵を消す
                    }
                }
            }

            for (int i = _popups.Count - 1; i >= 0; i--)
            {
                if (_popups[i].Update(gameTime))
                    _popups.RemoveAt(i);
            }
        }

        private void OnEnemyHit(Enemy enemy)
        {
            if (_adventurer.InvincibleTime > 0)
                return; // ★ 無敵中はダメージなし

            _adventurer.Hp -= 1;
            _adventurer.InvincibleTime = 0.5f; // ★ 0.5秒無敵

            var pos = Adventurer.Position + new Vector2(16, 0);
            _popups.Add(new DamagePopup(pos, 1, Color.Red));

            if (_adventurer.Hp <= 0)
                OnPlayerDead();
        }

        private void OnPlayerDead()
        {
            // ★ Game1 に通知するためのフラグ
            PlayerDead = true;
        }

        public void Draw(SpriteBatch sb, GameTime gameTime)
        {
            sb.Begin(transformMatrix: _camera.GetMatrix());

            var cameraView = _camera.GetViewRectangle(_screenWidth, _screenHeight);
            _map.Draw(sb, cameraView);

            int fw = 32;
            int fh = 32;
            int row = _adventurer.Direction;

            var src = new Rectangle(_frame * fw, row * fh, fw, fh);

            // ★ 無敵中は点滅（透明度を変える）
            Color color = Color.White;

            if (_adventurer.InvincibleTime > 0)
            {
                // 点滅周期（0.1秒ごとにON/OFF）
                float blink = (float)(gameTime.TotalGameTime.TotalSeconds * 10);

                if (((int)blink % 2) == 0)
                    color = Color.White * 0.3f; // 薄く表示
                else
                    color = Color.White; // 通常表示
            }

            // ★ プレイヤー描画はこれ1回だけ！
            sb.Draw(_playerTexture, _adventurer.Position, src, color);

            // ★ 敵描画
            foreach (var e in _enemies)
            {
                int ex = e.TilePos.X;
                int ey = e.TilePos.Y;

                if (_map.TileMapData.Fog[ex, ey] == Visibility.Visible)
                    e.Draw(sb, _map.TileSize);
            }

            foreach (var p in _popups)
            {
                p.Draw(sb, FontManager.GetFont(FontID.Main, 20));
            }

            sb.End();

            // ★ ミニマップ
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
