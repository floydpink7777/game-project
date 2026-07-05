using FontStashSharp;
using GameEngine.System.Input;
using GameEngine.System.Logic;
using GameEngine.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using static GameEngine.System.GameConfig;

public class TitleScreenRenderer
{
    private readonly SpriteBatch _spriteBatch;

    private Rectangle _startRect;
    private Rectangle _quitRect;

    public enum Anchor
    {
        TopLeft,
        TopCenter,
        TopRight,
        CenterLeft,
        Center,
        CenterRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    public TitleScreenRenderer(SpriteBatch spriteBatch)
    {
        _spriteBatch = spriteBatch;
    }

    public void Resize(int screenW, int screenH)
    {
        // Start ボタン：画面中央に配置
        _startRect = UITransform.AnchorRect(
            Anchor.Center,
            200, 60,
            screenW, screenH
        );

        // Quit ボタン：Start の下に配置
        _quitRect = new Rectangle(
            _startRect.X,
            _startRect.Y + 100,
            200,
            60
        );
    }

    public bool Update(GameLogic logic, InputManager input)
    {
        var mouse = Mouse.GetState();
        var pos = new Point(mouse.X, mouse.Y);

        if (_startRect.Contains(pos) && mouse.LeftButton == ButtonState.Pressed)
        {
            logic.StartNewGame();
            return true;
        }

        if (_quitRect.Contains(pos) && mouse.LeftButton == ButtonState.Pressed)
        {
            Environment.Exit(0);
        }

        if (input.Keyboard.Pressed(Keys.Enter))
        {
            logic.StartNewGame();
            return true;
        }

        return false;
    }

    public void Draw(SpriteFontBase font, int screenW, int screenH)
    {
        // 背景
        _spriteBatch.Draw(
            TextureManager.Get(TextureID.WhitePixel),
            new Rectangle(0, 0, screenW, screenH),
            Color.Black
        );

        // タイトル文字（上中央）
        string title = "Record Alter";
        var size = font.MeasureString(title);

        float tx = screenW / 2f - size.X / 2f;
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

    public static class UITransform
    {
        public static Rectangle AnchorRect(Anchor anchor, int width, int height, int screenW, int screenH)
        {
            int x = 0, y = 0;

            switch (anchor)
            {
                case Anchor.Center:
                    x = (screenW - width) / 2;
                    y = (screenH - height) / 2;
                    break;

                case Anchor.BottomCenter:
                    x = (screenW - width) / 2;
                    y = screenH - height - 40; // 下から40px余白
                    break;

                case Anchor.TopCenter:
                    x = (screenW - width) / 2;
                    y = 40;
                    break;

                    // 必要に応じて他のアンカーも追加
            }

            return new Rectangle(x, y, width, height);
        }
    }
}
