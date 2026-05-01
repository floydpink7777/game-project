using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Dungeon
{
    public class TileMapRenderer
    {
        private Texture2D _tileset;
        private int _tileSize;
        private int _tilesPerRow;

        public TileMapRenderer(Texture2D tileset, int tileSize)
        {
            _tileset = tileset;
            _tileSize = tileSize;
            _tilesPerRow = tileset.Width / tileSize;
        }

        public void Draw(SpriteBatch sb, TileMapData map)
        {
            int drawSize = _tileSize * 2;

            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                {
                    int id = map.Tiles[x, y];
                    if (id < 0) continue;

                    var src = GetSourceRect(id);
                    var dst = new Rectangle(x * drawSize, y * drawSize, drawSize, drawSize);

                    sb.Draw(_tileset, dst, src, Color.White);
                }
        }

        private Rectangle GetSourceRect(int id)
        {
            int sx = (id % _tilesPerRow) * _tileSize;
            int sy = (id / _tilesPerRow) * _tileSize;
            return new Rectangle(sx, sy, _tileSize, _tileSize);
        }
    }

}
