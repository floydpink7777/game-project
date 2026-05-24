using GameEngine.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameEngine.System.GameConfig;

namespace GameEngine.Utils
{
    public static class TextureManager
    {
        private static ContentManager _content;
        private static readonly Dictionary<object, Texture2D> _cache = new();

        public static void Load(ContentManager content, GraphicsDevice gd)
        {
            _content = content;

            // WhitePixel
            Texture2D white = new Texture2D(gd, 1, 1);
            white.SetData(new[] { Color.White });
            _cache[TextureID.WhitePixel] = white;

            // ★ ここで TextureID を全部 Preload する
            foreach (var id in GameAssets.TexturePaths.Keys)
                Preload(id);

            // ★ ItemID も必要なら Preload
            foreach (var id in GameAssets.ItemTexturePaths.Keys)
                Preload(id);
        }

        public static void Preload<T>(T id) where T : Enum
        {
            if (_cache.ContainsKey(id)) return;

            string path = id switch
            {
                TextureID tex => tex.Path(),
                ItemID item => item.Path(),
                _ => throw new ArgumentException("Unsupported asset type")
            };

            _cache[id] = _content.Load<Texture2D>(path);
        }

        public static Texture2D Get<T>(T id) where T : Enum
        {
            return (Texture2D)_cache[id];
        }

        public static Texture2D GetByPath(string path)
        {
            if (!_cache.TryGetValue(path, out var tex))
            {
                tex = _content.Load<Texture2D>(path);
                _cache[path] = tex;
            }
            return tex;
        }
    }
}
