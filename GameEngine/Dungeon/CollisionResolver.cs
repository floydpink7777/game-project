using GameEngine.GameData.Player;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Dungeon
{
    public static class CollisionResolver
    {
        public static void Resolve(Adventurer adventurer, TileMap map)
        {
            Vector2 pos = adventurer.Position;

            int scale = 2;

            // --- X方向 ---
            pos.X += adventurer.Velocity.X;
            Rectangle boundsX = new Rectangle(
                (int)pos.X, (int)adventurer.Position.Y,
                adventurer.Bounds.Width, adventurer.Bounds.Height
            );

            // --- X方向 ---
            if (IsColliding(boundsX, map, out int tileX, out int tileY))
            {
                if (adventurer.Velocity.X > 0)
                {
                    pos.X = tileX * (map.TileSize * scale) - adventurer.Bounds.Width;
                }
                else if (adventurer.Velocity.X < 0)
                {
                    pos.X = (tileX + 1) * (map.TileSize * scale);
                }
            }

            // --- Y方向 ---
            pos.Y += adventurer.Velocity.Y;
            Rectangle boundsY = new Rectangle(
                (int)pos.X, (int)pos.Y,
                adventurer.Bounds.Width, adventurer.Bounds.Height
            );

            // --- Y方向 ---
            if (IsColliding(boundsY, map, out tileX, out tileY))
            {
                if (adventurer.Velocity.Y > 0)
                {
                    pos.Y = tileY * (map.TileSize * scale) - adventurer.Bounds.Height;
                }
                else if (adventurer.Velocity.Y < 0)
                {
                    pos.Y = (tileY + 1) * (map.TileSize * scale);
                }
            }

            adventurer.Position = pos;
        }

        private static bool IsColliding(Rectangle bounds, TileMap map, out int tileX, out int tileY)
        {
            int scale = 2; // TileMap.Draw の拡大率

            int left = bounds.Left / (map.TileSize * scale);
            int right = (bounds.Right - 1) / (map.TileSize * scale);
            int top = bounds.Top / (map.TileSize * scale);
            int bottom = (bounds.Bottom - 1) / (map.TileSize * scale);

            // 左上
            if (map.IsSolid(left, top)) { tileX = left; tileY = top; return true; }
            // 右上
            if (map.IsSolid(right, top)) { tileX = right; tileY = top; return true; }
            // 左下
            if (map.IsSolid(left, bottom)) { tileX = left; tileY = bottom; return true; }
            // 右下
            if (map.IsSolid(right, bottom)) { tileX = right; tileY = bottom; return true; }

            tileX = tileY = 0;
            return false;
        }

        public static void ClampToMap(Adventurer adventurer, TileMap map)
        {
            int scale = 2; // Draw と Collision に合わせる

            int mapWidthPx = map.TileMapData.Width * map.TileSize * scale;
            int mapHeightPx = map.TileMapData.Height * map.TileSize * scale;

            // 左端
            if (adventurer.Bounds.Left < 0)
                adventurer.Position.X = 0;

            // 上端
            if (adventurer.Bounds.Top < 0)
                adventurer.Position.Y = 0;

            // 右端
            if (adventurer.Bounds.Right > mapWidthPx)
                adventurer.Position.X = mapWidthPx - adventurer.Bounds.Width;

            // 下端
            if (adventurer.Bounds.Bottom > mapHeightPx)
                adventurer.Position.Y = mapHeightPx - adventurer.Bounds.Height;
        }

    }
}
