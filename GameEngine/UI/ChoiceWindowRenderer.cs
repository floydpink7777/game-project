using GameEngine.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameEngine.System.GameConfig;

namespace GameEngine.UI
{
    public class ChoiceWindowRenderer
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly Texture2D _windowTexture;

        public ChoiceWindowRenderer(SpriteBatch spriteBatch)
        {
            _spriteBatch = spriteBatch;
            _windowTexture = TextureManager.GetTexture(TextureID.ChoiceWindow);
        }

        public void Draw(Rectangle rect)
        {
            _spriteBatch.Draw(_windowTexture, rect, Color.White);
        }

        // ★ 色付き描画（ハイライト用）
        public void Draw(Rectangle rect, Color color)
        {
            _spriteBatch.Draw(_windowTexture, rect, color);
        }
    }
}
