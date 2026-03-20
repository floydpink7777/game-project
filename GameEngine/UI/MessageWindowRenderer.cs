using GameEngine.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static GameEngine.System.GameConfig;

namespace GameEngine.UI
{
    public class MessageWindowRenderer
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly Texture2D _windowTexture;
        private readonly float _scale;

        public Rectangle WindowRect { get; private set; }
        public Rectangle TextArea { get; private set; }
        public Rectangle NameArea { get; private set; }

        private static Texture2D _debugPixel;

        public MessageWindowRenderer(SpriteBatch spriteBatch, bool isNarration = false)
        {
            _spriteBatch = spriteBatch;
            _scale = 0.58f;

            WindowRect = new Rectangle(0, 720, 1280, 300);
            TextArea = new Rectangle(40, 760, 1200, 220);

            if (isNarration)
            {
                NameArea = Rectangle.Empty;
                _windowTexture = TextureManager.GetTexture(TextureID.MessageFrameNoName);
            }
            else
            {
                // セリフ用レイアウト
                NameArea = new Rectangle(40, 730, 300, 40);
                _windowTexture = TextureManager.GetTexture(TextureID.MessageFrame);
            }

            CalculateLayout();
        }

        private void CalculateLayout()
        {
            int drawWidth = (int)(_windowTexture.Width * _scale);
            int drawHeight = (int)(_windowTexture.Height * _scale);

            int x = (800 - drawWidth) / 2;
            int y = 480 - drawHeight - 20;

            WindowRect = new Rectangle(x, y, drawWidth, drawHeight);

            if (NameArea == Rectangle.Empty)
            {
                // ナレーション用：名前欄がないぶん上に広げる
                TextArea = new Rectangle(
                    WindowRect.X + 30,
                    WindowRect.Y + 10,          // ← 30 → 10 に変更（上に寄せる）
                    WindowRect.Width - 60,
                    WindowRect.Height - 40      // ← 高さを増やす
                );
            }
            else
            {
                // セリフ用
                TextArea = new Rectangle(
                    WindowRect.X + 30,
                    WindowRect.Y + 30,
                    WindowRect.Width - 60,
                    WindowRect.Height - 60
                );
            }

            // ★ ナレーション時は NameArea を再計算しない
            if (NameArea != Rectangle.Empty)
            {
                NameArea = new Rectangle(
                    WindowRect.X + 50,
                    WindowRect.Y,
                    250,
                    30
                );
            }
        }

        public void DrawWindow()
        {
            _spriteBatch.Draw(_windowTexture, WindowRect, Color.White);

            //// --- デバッグ描画 ---
            //DrawDebugRect(WindowRect, Color.Red);
            //DrawDebugRect(TextArea, Color.Green);
            //if (NameArea != Rectangle.Empty)
            //    DrawDebugRect(NameArea, Color.Blue);
        }

        public static void InitDebug(GraphicsDevice device)
        {
            _debugPixel = new Texture2D(device, 1, 1);
            _debugPixel.SetData(new[] { Color.White });
        }
        private void DrawDebugRect(Rectangle rect, Color color)
        {
            // 上
            _spriteBatch.Draw(_debugPixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), color);
            // 下
            _spriteBatch.Draw(_debugPixel, new Rectangle(rect.X, rect.Bottom, rect.Width, 1), color);
            // 左
            _spriteBatch.Draw(_debugPixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), color);
            // 右
            _spriteBatch.Draw(_debugPixel, new Rectangle(rect.Right, rect.Y, 1, rect.Height), color);
        }
    }
}
