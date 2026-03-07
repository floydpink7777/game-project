using FontStashSharp;
using GameEngine.Utils;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Content;
using MonoGame.Extended.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static GameEngine.System.GameConfig;
using static System.Net.Mime.MediaTypeNames;

namespace GameEngine.Utils
{
    public static class FontManager
    {
        private static ContentManager _content;
        private static readonly Dictionary<FontID, FontSystem> _cache = new();

        public static void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _content = content;

            foreach (var (id, path) in GameAssets.FontPaths)
            {
                FontSystem s = new FontSystem();
                s.AddFont(File.ReadAllBytes(path));

                _cache[id] = s;
            }
        }

        public static FontSystem GetFontSystem(FontID id)
        {
            return _cache[id];
        }

        public static SpriteFontBase GetFont(FontID id, int size)
        {
            SpriteFontBase font = GetFontSystem(id).GetFont(size);

            return font;
        }
    }
}
