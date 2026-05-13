using FontStashSharp;
using GameEngine.System.Input;
using GameEngine.System.Logic;
using GameEngine.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameEngine.System.GameConfig;

namespace GameEngine.UI
{
    public class TitleScreenRenderer
    {
        private readonly SpriteBatch _spriteBatch;

        private Rectangle _startRect = new Rectangle(300, 260, 200, 60);
        private Rectangle _quitRect = new Rectangle(300, 340, 200, 60);

        public TitleScreenRenderer(SpriteBatch spriteBatch)
        {
            _spriteBatch = spriteBatch;
        }

        // -------------------------
        // 入力処理（Game1.Update から呼ぶ）
        // -------------------------
        public bool Update(GameLogic logic, InputManager input)
        {
            var mouse = Mouse.GetState();
            var pos = new Point(mouse.X, mouse.Y);

            // Start
            if (_startRect.Contains(pos) && mouse.LeftButton == ButtonState.Pressed)
            {
                logic.StartNewGame();
                return true;
            }

            // Quit
            if (_quitRect.Contains(pos) && mouse.LeftButton == ButtonState.Pressed)
            {
                Environment.Exit(0);
                //return;
            }

            // Enter キーで Start（任意）
            if (input.Keyboard.Pressed(Keys.Enter))
            {
                logic.StartNewGame();
                return true;
            }

            return false;
        }

        // -------------------------
        // 描画（Game1.Draw から呼ぶ）
        // -------------------------
        public void Draw(SpriteFontBase font)
        {
            // 背景
            _spriteBatch.Draw(
                TextureManager.Get(TextureID.WhitePixel),
                new Rectangle(0, 0, 800, 480),
                Color.Black
            );

            // タイトル文字
            string title = "Record Alter";
            var size = font.MeasureString(title);
            float tx = 400 - size.X / 2;
            float ty = 120;

            _spriteBatch.DrawString(font, title, new Vector2(tx, ty), Color.White);

            // ボタン
            DrawButton(font, "Start", _startRect);
            DrawButton(font, "Quit", _quitRect);
        }

        private void DrawButton(SpriteFontBase font, string text, Rectangle rect)
        {
            _spriteBatch.Draw(
                TextureManager.Get(TextureID.WhitePixel),
                rect,
                Color.DarkSlateGray
            );

            var size = font.MeasureString(text);
            float x = rect.Center.X - size.X / 2;
            float y = rect.Center.Y - size.Y / 2;

            _spriteBatch.DrawString(font, text, new Vector2(x, y), Color.White);
        }
    }
}
