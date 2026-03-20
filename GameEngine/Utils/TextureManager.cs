using GameEngine.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Utils
{
    public static class TextureManager
    {
        private static ContentManager _content;

        private static readonly Dictionary<GameConfig.TextureID, Texture2D> _cache = new();

        public static void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _content = content;

            Preload(
                GameConfig.TextureID.MessageFrame,
                GameConfig.TextureID.MessageFrameNoName,
                GameConfig.TextureID.ChoiceWindow
            );

            // ★ WhitePixel を生成してキャッシュに登録
            Texture2D white = new Texture2D(graphicsDevice, 1, 1);
            white.SetData(new[] { Color.White });
            _cache[GameConfig.TextureID.WhitePixel] = white;
        }

        public static void Preload(GameConfig.TextureID id)
        {
            // 既にロード済みなら処理終了
            if (_cache.ContainsKey(id)) return;

            var path = GameAssets.GetTexturePath(id);
            var tex = _content.Load<Texture2D>(path);
            _cache[id] = tex;
        }

        public static void Preload(params GameConfig.TextureID[] ids)
        {
            foreach (var id in ids)
                Preload(id);
        }

        public static Texture2D GetTexture(GameConfig.TextureID id)
        {
            if (_content == null)
                throw new InvalidOperationException("TextureManager.Load() が呼ばれていません。");

            if (_cache.TryGetValue(id, out var tex))
                return tex;

            throw new KeyNotFoundException($"TextureID '{id}' はまだ Preload されていません。");
        }
    }
}
