using FontStashSharp;
using GameEngine.System.Core;
using GameEngine.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameEngine.System.GameConfig;
using static GameEngine.Utils.GameAssets;

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

        private Texture2D _debugPixel;

        private List<Item> _items = new();

        private bool _inventoryOpen = false;

        private bool _prevTab = false;

        private ItemInstance? _hoverItem = null;
        private Rectangle? _hoverSlotRect = null;

        private ItemInstance? _dragItem = null;
        private int _dragCount = 0;
        private MouseState prevMs;

        private int? _hoverSlotIndex = null;
        private int? _dragOriginIndex = null;

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
            {
                // ★ 本来はマップデータから EnemyID を読むべき
                string id = "slime";

                var e = new Enemy(pos, enemyTex);
                e.EnemyID = id;

                // ★ EnemyID → Category のマッピング（暫定）
                //   後で JSON 化しても良い
                e.Category = EnemyCategory.Slime;

                // ★ ステータスも後で JSON 化できる
                e.Attack = 105;
                e.Defense = 90;

                // ================================
                // ★ ① レア枠（カテゴリーごと）
                // ================================
                var rarity = ItemDB.RarityTable[e.Category.ToString()];
                e.RarityTable = new DropTable(
                    rarity.Select(kv => (kv.Key, kv.Value)).ToArray()
                );

                // ================================
                // ★ ② アイテムテーブル（EnemyID ごと）
                // ================================
                var drop = ItemDB.DropTable[e.EnemyID];
                e.ItemDropTable = new Dictionary<string, DropTable>();

                foreach (var rarityEntry in drop)
                {
                    e.ItemDropTable[rarityEntry.Key] =
                        new DropTable(rarityEntry.Value.Select(kv => (kv.Key, kv.Value)).ToArray());
                }

                _enemies.Add(e);
            }


            _debugPixel = new Texture2D(advTex.GraphicsDevice, 1, 1);
            _debugPixel.SetData(new[] { Color.White });
        }

        private void DrawCircle(SpriteBatch sb, Vector2 center, float radius, Color color, int segments = 32)
        {
            float increment = MathF.Tau / segments;
            float theta = 0f;

            Vector2 prev = center + new Vector2(MathF.Cos(0), MathF.Sin(0)) * radius;

            for (int i = 1; i <= segments; i++)
            {
                theta += increment;
                Vector2 next = center + new Vector2(MathF.Cos(theta), MathF.Sin(theta)) * radius;

                DrawLine(sb, prev, next, color);
                prev = next;
            }
        }

        private void DrawLine(SpriteBatch sb, Vector2 a, Vector2 b, Color color)
        {
            Vector2 diff = b - a;
            float length = diff.Length();
            float angle = MathF.Atan2(diff.Y, diff.X);

            sb.Draw(_debugPixel, a, null, color, angle, Vector2.Zero, new Vector2(length, 1f), SpriteEffects.None, 0f);
        }

        private void DrawCone(SpriteBatch sb, Vector2 center, Vector2 forward, float radius, float angleDeg, Color color)
        {
            int segments = 20;
            float angle = MathHelper.ToRadians(angleDeg);
            float half = angle / 2f;

            // forward の角度
            float baseAngle = MathF.Atan2(forward.Y, forward.X);

            Vector2 prev = center;

            for (int i = 0; i <= segments; i++)
            {
                float t = -half + (angle * i / segments);
                float theta = baseAngle + t;

                Vector2 dir = new Vector2(MathF.Cos(theta), MathF.Sin(theta));
                Vector2 point = center + dir * radius;

                DrawLine(sb, center, point, color);
            }
        }

        public void ResetPlayerPosition(Vector2 pos)
        {
            _adventurer.SetPosition(pos);
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.Tab) && !_prevTab)
            {
                _inventoryOpen = !_inventoryOpen;
            }

            _prevTab = ks.IsKeyDown(Keys.Tab);

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
                enemy.Update(gameTime, _adventurer.Position, _map);
                enemy.ResolveEnemyCollision(_enemies);

                Rectangle enemyRect = new Rectangle(
                    (int)enemy.Position.X,
                    (int)enemy.Position.Y,
                    32, 32
                );

                // 敵に接触時のプレイヤーへのダメージ
                if (playerRect.Intersects(enemyRect))
                {
                    OnEnemyHit(enemy);
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
                        int dmg = DamageCalculator.CalculateDamage(_adventurer, enemy);

                        // ★ 敵ダメージポップ生成
                        Vector2 pos = enemy.Position + new Vector2(16, -10);
                        _popups.Add(new DamagePopup(pos, dmg, Color.White));

                        _enemies.RemoveAt(i);   // ★ 敵を消す

                        // ★ アイテムドロップ判定
                        string rarity = enemy.RarityTable.Roll();

                        if (rarity != "None")
                        {
                            string itemId = enemy.ItemDropTable[rarity].Roll();

                            var template = ItemDB.Templates[itemId];
                            var instance = ItemDB.CreateInstance(template);
                            _items.Add(new Item(enemy.Position, instance));
                        }
                    }
                }

                prevMs = Mouse.GetState();
            }

            for (int i = _popups.Count - 1; i >= 0; i--)
            {
                if (_popups[i].Update(gameTime))
                    _popups.RemoveAt(i);
            }

            for (int i = _items.Count - 1; i >= 0; i--)
            {
                if (_adventurer.Bounds.Intersects(_items[i].Bounds))
                {
                    OnItemPickup(_items[i]);
                    _items.RemoveAt(i);
                }
            }
        }

        private void OnEnemyHit(Enemy enemy)
        {
            if (_adventurer.InvincibleTime > 0)
                return; // ★ 無敵中はダメージなし

            int dmg = DamageCalculator.CalculateDamage(enemy, _adventurer);

            _adventurer.Hp -= dmg;
            _adventurer.InvincibleTime = 0.5f; // ★ 0.5秒無敵

            var pos = Adventurer.Position + new Vector2(16, 0);
            _popups.Add(new DamagePopup(pos, dmg, Color.Red));

            if (_adventurer.Hp <= 0)
                OnPlayerDead();
        }

        private void OnItemPickup(Item item)
        {
            // ① ItemInstance を生成
            ItemInstance instance = ItemDB.CreateInstance(item.Instance.Template);

            // ② 既にあるスロットに追加
            for (int i = 0; i < _adventurer.Inventory.Length; i++)
            {
                var slot = _adventurer.Inventory[i];

                if (slot != null && slot.Value.item.IsStackableWith(instance))
                {
                    _adventurer.Inventory[i] = (slot.Value.item, slot.Value.count + 1);
                    return;
                }
            }

            // ③ 空スロットに入れる
            for (int i = 0; i < _adventurer.Inventory.Length; i++)
            {
                if (_adventurer.Inventory[i] == null)
                {
                    _adventurer.Inventory[i] = (instance, 1);
                    return;
                }
            }
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

            foreach (var e in _enemies)
            {
                int ex = e.TilePos.X;
                int ey = e.TilePos.Y;

                var fog = _map.TileMapData.Fog;

                if (ex < 0 || ey < 0 || ex >= fog.GetLength(0) || ey >= fog.GetLength(1))
                    continue;

                Vector2 forward;

                if (e.State == EnemyState.Lost)
                {
                    forward = e.LastSeenDirection;   // ★ 追跡中の向きを維持
                }
                else if (e.Velocity.LengthSquared() > 0)
                {
                    forward = Vector2.Normalize(e.Velocity);
                }
                else
                {
                    forward = e.WanderDirection;
                }

                // ★ 前方視界（60°・半径200px）を描画
                DrawCone(sb, e.Position + new Vector2(16, 16), forward, 150f, 60f, Color.Red * 0.3f);

                if (fog[ex, ey] == Visibility.Visible)
                    e.Draw(sb, _map.TileSize);
            }

            foreach (var item in _items)
            {
                Texture2D tex = TextureManager.GetByPath(item.Instance.Template.IconPath);

                sb.Draw(tex, new Rectangle((int)item.Position.X, (int)item.Position.Y, 16, 16), Color.White);
            }

            foreach (var p in _popups)
            {
                p.Draw(sb, FontManager.GetFont(FontID.Main, 20));
            }

            sb.End();

            // ★ ミニマップ
            sb.Begin();
            DrawMiniMap(sb);
            if (_inventoryOpen)
            {
                DrawInventoryWindow(sb);
            }

            sb.End();
        }

        private void DrawInventoryWindow(SpriteBatch sb)
        {
            var font = FontManager.GetFont(FontID.Main, 18);

            MouseState ms = Mouse.GetState();
            Point mousePos = new Point(ms.X, ms.Y);

            _hoverItem = null;
            _hoverSlotRect = null;
            _hoverSlotIndex = null;

            sb.Draw(_debugPixel, new Rectangle(50, 50, 350, 350), Color.Black * 0.7f);
            sb.DrawString(font, "所持品", new Vector2(70, 60), Color.White);

            int slotSize = 48;
            int padding = 8;
            int cols = 5;
            int startX = 70;
            int startY = 100;

            var slots = _adventurer.Inventory; // ★ 固定 20 スロット

            // ============================
            // ① スロット描画（ホバー → 掴む → 描画）
            // ============================
            for (int index = 0; index < 20; index++)
            {
                int col = index % cols;
                int row = index / cols;

                int x = startX + col * (slotSize + padding);
                int y = startY + row * (slotSize + padding);

                Rectangle slotRect = new Rectangle(x, y, slotSize, slotSize);

                // ホバー
                if (slotRect.Contains(mousePos))
                {
                    _hoverSlotRect = slotRect;
                    _hoverSlotIndex = index;

                    if (slots[index] != null)
                        _hoverItem = slots[index]!.Value.item;
                }

                // 掴む
                if (ms.LeftButton == ButtonState.Pressed && prevMs.LeftButton == ButtonState.Released)
                {
                    if (slotRect.Contains(mousePos) && slots[index] != null)
                    {
                        bool split = Keyboard.GetState().IsKeyDown(Keys.LeftShift) ||
                                     Keyboard.GetState().IsKeyDown(Keys.RightShift);

                        var slot = slots[index];
                        if (slot == null)
                            continue;

                        var item = slot.Value.item;
                        var count = slot.Value.count;

                        if (split && count > 1)
                        {
                            // ★ 半分だけ掴む
                            int half = count / 2;
                            _dragItem = item;
                            _dragCount = half;
                            _dragOriginIndex = index;

                            // 元スロットには残りを残す
                            slots[index] = (item, count - half);
                        }
                        else
                        {
                            // ★ 通常の掴む
                            _dragItem = item;
                            _dragCount = count;
                            _dragOriginIndex = index;

                            slots[index] = null;
                        }
                    }

                }

                // スロット枠
                sb.Draw(_debugPixel, new Rectangle(x, y, slotSize, slotSize), Color.White * 0.2f);

                // アイテム描画
                if (slots[index] != null)
                {
                    var slot = slots[index];
                    var item = slot.Value.item;
                    var count = slot.Value.count;

                    Texture2D tex = TextureManager.GetByPath(item.Template.IconPath);
                    sb.Draw(tex, new Rectangle(x + 8, y + 8, 32, 32), Color.White);

                    // ★ 個数表示（右下）
                    string countText = $"x{count}";
                    Vector2 size = font.MeasureString(countText);

                    int textX = x + slotSize - (int)size.X - 4;
                    int textY = y + slotSize - (int)size.Y - 2;

                    // 背景（黒半透明）
                    sb.Draw(_debugPixel,
                        new Rectangle(textX - 2, textY - 2, (int)size.X + 4, (int)size.Y + 4),
                        Color.Black * 0.6f);

                    sb.DrawString(font, countText, new Vector2(textX, textY), Color.White);
                }

                // 右クリックで使用
                if (ms.RightButton == ButtonState.Pressed &&
                    prevMs.RightButton == ButtonState.Released &&
                    slotRect.Contains(mousePos) &&
                    slots[index] != null)
                {
                    UseItem(index);
                }
            }

            // ============================
            // ② ドロップ処理（固定スロット方式 + swap対応）
            // ============================
            if (_dragItem != null &&
                ms.LeftButton == ButtonState.Released &&
                prevMs.LeftButton == ButtonState.Pressed)
            {
                int dropIndex = _hoverSlotIndex ?? _dragOriginIndex.Value;

                var target = slots[dropIndex]; // (ItemInstance item, int count)? or null

                // ★ ① 同じ TemplateID なら結合
                if (target != null &&
                    target.Value.item.IsStackableWith(_dragItem))
                {
                    slots[dropIndex] = (target.Value.item, target.Value.count + _dragCount);
                }
                else
                {
                    // ★ ② 別アイテムなら swap
                    if (target != null)
                    {
                        var temp = target.Value; // (ItemInstance item, int count)
                        slots[dropIndex] = (_dragItem, _dragCount);
                        slots[_dragOriginIndex.Value] = temp;
                    }
                    else
                    {
                        // ★ ③ 空スロットなら普通に置く
                        slots[dropIndex] = (_dragItem, _dragCount);
                    }
                }

                _dragItem = null;
                _dragOriginIndex = null;
            }

            // ============================
            // ③ ホバー枠
            // ============================
            if (_hoverSlotRect.HasValue)
            {
                var r = _hoverSlotRect.Value;
                sb.Draw(_debugPixel, new Rectangle(r.X - 2, r.Y - 2, r.Width + 4, 2), Color.Yellow);
                sb.Draw(_debugPixel, new Rectangle(r.X - 2, r.Y + r.Height, r.Width + 4, 2), Color.Yellow);
                sb.Draw(_debugPixel, new Rectangle(r.X - 2, r.Y - 2, 2, r.Height + 4), Color.Yellow);
                sb.Draw(_debugPixel, new Rectangle(r.X + r.Width, r.Y - 2, 2, r.Height + 4), Color.Yellow);
            }

            // ============================
            // ④ ドラッグ中アイコン
            // ============================
            if (_dragItem != null)
            {
                Texture2D tex = TextureManager.GetByPath(_dragItem.Template.IconPath);
                sb.Draw(tex, new Rectangle(mousePos.X - 16, mousePos.Y - 16, 32, 32), Color.White);

                sb.DrawString(font, $"x{_dragCount}", new Vector2(mousePos.X, mousePos.Y), Color.White);
            }


            // ============================
            // ⑤ Tooltip
            // ============================
            if (_hoverItem != null)
                DrawItemTooltip(sb, _hoverItem);

            prevMs = ms;
        }

        private void DrawItemTooltip(SpriteBatch sb, ItemInstance instance)
        {
            var font = FontManager.GetFont(FontID.Main, 18);

            string name = instance.DisplayName;
            string desc = instance.DisplayDescription;

            MouseState ms = Mouse.GetState();
            int x = ms.X + 20;
            int y = ms.Y + 20;

            // サイズ計算
            Vector2 nameSize = font.MeasureString(name);
            Vector2 descSize = font.MeasureString(desc);

            int width = (int)Math.Max(nameSize.X, descSize.X) + 20;
            int height = (int)(nameSize.Y + descSize.Y) + 20;

            // ★ 画面外に出ないように調整
            int screenW = _screenWidth;
            int screenH = _screenHeight;

            if (x + width > screenW)
                x = screenW - width - 10;

            if (y + height > screenH)
                y = screenH - height - 10;

            // 背景
            sb.Draw(_debugPixel, new Rectangle(x, y, width, height), Color.Black * 0.8f);

            // 枠線
            sb.Draw(_debugPixel, new Rectangle(x, y, width, 1), Color.White);
            sb.Draw(_debugPixel, new Rectangle(x, y + height - 1, width, 1), Color.White);
            sb.Draw(_debugPixel, new Rectangle(x, y, 1, height), Color.White);
            sb.Draw(_debugPixel, new Rectangle(x + width - 1, y, 1, height), Color.White);

            // テキスト
            sb.DrawString(font, name, new Vector2(x + 10, y + 5), Color.Yellow);
            sb.DrawString(font, desc, new Vector2(x + 10, y + 5 + nameSize.Y), Color.White);
        }

        private void UseItem(int index)
        {
            var slot = _adventurer.Inventory[index];
            if (slot == null)
                return;

            var instance = slot.Value.item;      // ItemInstance
            var template = instance.Template;    // ItemTemplate

            switch (template.Form)               // ← Form or Category で判定
            {
                case ItemForm.Potion:
                    UseConsumable(instance, index);
                    break;

                case ItemForm.Scroll:
                case ItemForm.Sword:
                    // 装備や魔法など後で追加
                    break;

                default:
                    // 使用不可
                    break;
            }
        }

        private void UseConsumable(ItemInstance instance, int index)
        {
            var template = instance.Template;

            // ★ 今は固定値 20 回復（後で Enchant に移せる）
            _adventurer.Hp = Math.Min(_adventurer.MaxHp, _adventurer.Hp + 20);

            // ★ スタックを減らす
            var slot = _adventurer.Inventory[index]!.Value;

            if (slot.count > 1)
                _adventurer.Inventory[index] = (instance, slot.count - 1);
            else
                _adventurer.Inventory[index] = null;

            // ★ ポップアップ
            _popups.Add(new DamagePopup(_adventurer.Position, +20, Color.Green));
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

        private void ResolvePlayerEnemyCollisions()
        {
            foreach (var enemy in _enemies)
            {
                if (_adventurer.Bounds.Intersects(enemy.Bounds))
                {
                    Vector2 push = _adventurer.Position - enemy.Position;

                    if (push.LengthSquared() < 0.01f)
                        push = new Vector2(1, 0);

                    push.Normalize();

                    // ★ 壁に押し込まれないように弱めの押し返し
                    _adventurer.Position += push * 0.5f;
                }
            }
        }
    }
}
