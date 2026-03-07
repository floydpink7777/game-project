using FontStashSharp;
using GameEngine.Events.RuntimeNode;
using GameEngine.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System.Collections.Generic;
using static GameEngine.System.GameConfig;

namespace GameEngine.UI
{
    internal class UIManager
    {
        private SpriteBatch _spriteBatch;

        public UIManager(SpriteBatch spriteBatch)
        {
            _spriteBatch = spriteBatch;
        }

        public void DrawDialogue(string speaker, string text, SpriteFontBase font)
        {
            _spriteBatch.DrawString(
                font,//FontManager.GetFont(FontID.Main),
                speaker,
                new Vector2(50, 50),
                Color.White
            );

            _spriteBatch.DrawString(
                font,//FontManager.GetFont(FontID.Main),
                text,
                new Vector2(50, 100),
                Color.White
            );
        }

        // ★ dynamic を完全に排除
        public void DrawChoices(List<ChoiceOption> choices, SpriteFontBase font)
        {
            for (int i = 0; i < choices.Count; i++)
            {
                _spriteBatch.DrawString(
                    font,//FontManager.GetFont(FontID.Main),
                    $"{i + 1}. {choices[i].Text}",   // ← PascalCase に変更
                    new Vector2(50, 150 + i * 40),
                    Color.Green
                );
            }
        }
    }
}