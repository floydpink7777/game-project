using GameEngine.GameData.Player;
using GameEngine.System.Core;
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

            int scale = 1;

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

        public static void Resolve(ICollidable obj, TileMap map)
        {
            Vector2 pos = obj.Position;

            // X方向
            pos.X += obj.Velocity.X;
            Rectangle boundsX = new Rectangle(
                (int)pos.X, (int)obj.Position.Y,
                obj.Bounds.Width, obj.Bounds.Height
            );

            if (IsColliding(boundsX, map, out int tileX, out int tileY))
            {
                if (obj.Velocity.X > 0)
                    pos.X = tileX * map.TileSize - obj.Bounds.Width;
                else if (obj.Velocity.X < 0)
                    pos.X = (tileX + 1) * map.TileSize;
            }

            // Y方向
            pos.Y += obj.Velocity.Y;
            Rectangle boundsY = new Rectangle(
                (int)pos.X, (int)pos.Y,
                obj.Bounds.Width, obj.Bounds.Height
            );

            if (IsColliding(boundsY, map, out tileX, out tileY))
            {
                if (obj.Velocity.Y > 0)
                    pos.Y = tileY * map.TileSize - obj.Bounds.Height;
                else if (obj.Velocity.Y < 0)
                    pos.Y = (tileY + 1) * map.TileSize;
            }

            obj.Position = pos;
        }

        public static bool IsColliding(Rectangle bounds, TileMap map, out int tileX, out int tileY)
        {
            int scale = 1; // TileMap.Draw の拡大率

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
            var pos = adventurer.Position;
            Clamp(ref pos, adventurer.Bounds, map);
            adventurer.Position = pos;
        }

        public static void ClampToMap(Enemy enemy, TileMap map)
        {
            var pos = enemy.Position;
            Clamp(ref pos, enemy.Bounds, map);
            enemy.Position = pos;
        }

        private static void Clamp(ref Vector2 position, Rectangle bounds, TileMap map)
        {
            int mapWidthPx = map.TileMapData.Width * map.TileSize;
            int mapHeightPx = map.TileMapData.Height * map.TileSize;

            if (bounds.Left < 0)
                position.X = 0;

            if (bounds.Top < 0)
                position.Y = 0;

            if (bounds.Right > mapWidthPx)
                position.X = mapWidthPx - bounds.Width;

            if (bounds.Bottom > mapHeightPx)
                position.Y = mapHeightPx - bounds.Height;
        }
    }
}
