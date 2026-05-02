using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Dungeon
{
    public class TileMap
    {
        public Texture2D Tileset;
        public TileMapData TileMapData;
        public int TileSize;

        public TileMap(Texture2D tileset, TileMapData data, int tileSize)
        {
            Tileset = tileset;
            TileMapData = data;
            TileSize = tileSize;
        }

        private Rectangle GetSourceRect(int tileId)
        {
            var tilesPerRow = Tileset.Width / TileSize;
            var tilesPerColumn = Tileset.Height / TileSize;

            int sx = (tileId % tilesPerRow) * TileSize;
            int sy = (tileId / tilesPerRow) * TileSize;

            return new Rectangle(sx, sy, TileSize, TileSize);
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle cameraView)
        {
            int drawSize = TileSize;

            int startX = Math.Max(cameraView.Left / drawSize, 0);
            int startY = Math.Max(cameraView.Top / drawSize, 0);
            int endX = Math.Min(cameraView.Right / drawSize + 1, TileMapData.Width);
            int endY = Math.Min(cameraView.Bottom / drawSize + 1, TileMapData.Height);

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    int id = TileMapData.Tiles[x, y];
                    if (id < 0) continue;

                    var src = GetSourceRect(id);

                    // ★ cameraView を引かない（行列がやってくれる）
                    var dst = new Rectangle(
                        x * drawSize,
                        y * drawSize,
                        drawSize,
                        drawSize
                    );

                    spriteBatch.Draw(Tileset, dst, src, Color.White);
                }
            }
        }



        public bool IsSolid(int tileX, int tileY)
        {
            // マップ外は「壁扱いしない」
            if (tileX < 0 || tileY < 0 ||
                tileX >= TileMapData.Width ||
                tileY >= TileMapData.Height)
                return false;

            int id = TileMapData.Tiles[tileX, tileY];
            return TileMapData.SolidTiles.Contains(id);
        }
    }

    public class TileMapData
    {
        public int[,] Tiles;
        public List<Rectangle> Rooms = new();
        public HashSet<int> SolidTiles = new();

        public int MinRoomWidth = 6;
        public int MinRoomHeight = 6;

        public Point StartPos;
        public Point GoalPos;

        public int Width => Tiles.GetLength(0);
        public int Height => Tiles.GetLength(1);

        public TileMapData(int[,] tiles)
        {
            Tiles = tiles;
        }
    }
}
