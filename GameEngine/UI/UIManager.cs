using FontStashSharp;
using GameEngine.Events.RuntimeNode;
using GameEngine.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using static GameEngine.System.GameConfig;

namespace GameEngine.UI
{
    public class UIManager
    {
        private readonly SpriteBatch _spriteBatch;

        private readonly MessageWindowRenderer _dialogueWindow;
        private readonly MessageWindowRenderer _narrationWindow;
        private ChoiceWindowRenderer _choiceWindow;

        private List<List<string>> _pages = new();
        private int _currentPage = 0;

        public string Speaker { get; private set; }
        public string Text { get; private set; }

        private int _visibleCharacters = 0;
        private float _typeSpeed = 30f; // 1秒あたり60文字（好みで調整）
        private float _typeTimer = 0f;

        private float _iconTimer = 0f;
        private bool _iconVisible = true;

        public bool IsPageComplete =>
            _visibleCharacters >= GetCurrentPageTotalCharacters();

        private List<ChoiceOption> _currentChoices;

        private int _cursorIndex = 0;
        public int CursorIndex => _cursorIndex;

        public UIManager(SpriteBatch spriteBatch)
        {
            _spriteBatch = spriteBatch;
            _dialogueWindow = new MessageWindowRenderer(spriteBatch, false);
            _narrationWindow = new MessageWindowRenderer(spriteBatch, true);
        }

        // -----------------------------
        // 安全ガード付き：ページ内文字数
        // -----------------------------
        private int GetCurrentPageTotalCharacters()
        {
            if (_pages == null || _pages.Count == 0)
                return 0;

            if (_currentPage < 0 || _currentPage >= _pages.Count)
                return 0;

            int count = 0;
            foreach (var line in _pages[_currentPage])
                count += line.Length;

            return count;
        }

        public void Update(GameTime gameTime)
        {
            // ページ未セットなら何もしない
            if (_pages == null || _pages.Count == 0)
                return;

            // タイプライター更新
            if (!IsPageComplete)
            {
                _typeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                while (_typeTimer >= 1f / _typeSpeed)
                {
                    _typeTimer -= 1f / _typeSpeed;
                    _visibleCharacters++;

                    if (_visibleCharacters >= GetCurrentPageTotalCharacters())
                        break;
                }
            }

            // ▼アイコン点滅
            if (IsPageComplete)
            {
                _iconTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_iconTimer >= 0.5f) // 0.5秒ごとに切り替え
                {
                    _iconVisible = !_iconVisible;
                    _iconTimer = 0f;
                }
            }
            else
            {
                // タイプ途中は非表示
                _iconVisible = false;
                _iconTimer = 0f;
            }
        }

        // -----------------------------
        // 名前欄
        // -----------------------------
        private void DrawName(string speaker, SpriteFontBase font, Rectangle area)
        {
            if (string.IsNullOrEmpty(speaker))
                return;

            _spriteBatch.DrawString(
                font,
                speaker,
                new Vector2(area.X, area.Y),
                Color.White
            );
        }

        // -----------------------------
        // WrapText（折り返し処理）
        // -----------------------------
        private List<string> WrapText(SpriteFontBase font, string text, float maxWidth, float spacing = 1.5f)
        {
            var result = new List<string>();
            var lines = text.Split('\n');

            foreach (var line in lines)
            {
                string current = "";

                foreach (var c in line)
                {
                    string test = current + c;

                    // 文字列の実際の描画幅を計算
                    float width = 0;
                    foreach (char ch in test)
                        width += font.MeasureString(ch.ToString()).X + spacing;

                    if (width > maxWidth)
                    {
                        result.Add(current);
                        current = c.ToString();
                    }
                    else
                    {
                        current = test;
                    }
                }

                if (current.Length > 0)
                    result.Add(current);
            }

            return result;
        }

        // 3行ごとにページ分割する
        private List<List<string>> PaginateLines(List<string> lines, int maxLinesPerPage = 3)
        {
            var pages = new List<List<string>>();
            var currentPage = new List<string>();

            foreach (var line in lines)
            {
                currentPage.Add(line);

                if (currentPage.Count >= maxLinesPerPage)
                {
                    pages.Add(currentPage);
                    currentPage = new List<string>();
                }
            }

            // 最後のページを追加
            if (currentPage.Count > 0)
                pages.Add(currentPage);

            return pages;
        }

        // -----------------------------
        // Dialogue
        // -----------------------------
        public void DrawDialogue(SpriteFontBase font)
        {
            if (string.IsNullOrEmpty(Speaker))
            {
                _narrationWindow.DrawWindow();
                DrawPage(_pages[_currentPage], font, _narrationWindow.TextArea);
                DrawPageIcon(font, _narrationWindow);
            }
            else
            {
                _dialogueWindow.DrawWindow();
                DrawName(Speaker, font, _dialogueWindow.NameArea);
                DrawPage(_pages[_currentPage], font, _dialogueWindow.TextArea);
                DrawPageIcon(font, _dialogueWindow);
            }
        }

        private void DrawPageIcon(SpriteFontBase font, MessageWindowRenderer window)
        {
            if (!_iconVisible || !IsPageComplete)
                return;

            string icon = "▼";

            float x = window.WindowRect.Right - font.MeasureString(icon).X - 6;
            float y = window.WindowRect.Bottom - font.LineHeight - 4;

            _spriteBatch.DrawString(font, icon, new Vector2(x, y), Color.White);
        }

        private void DrawPage(List<string> lines, SpriteFontBase font, Rectangle area)
        {
            float x = area.X;
            float y = area.Y;

            int remaining = _visibleCharacters;
            float spacing = 1.5f;

            foreach (var line in lines)
            {
                int len = line.Length;

                string toDraw =
                    remaining >= len ? line : line.Substring(0, remaining);

                DrawStringWithSpacing(font, toDraw, new Vector2(x, y), Color.White, spacing);

                remaining -= len;
                if (remaining <= 0)
                    break;

                y += font.LineHeight;
            }
        }

        public void DrawChoices(SpriteFontBase font)
        {
            if (_currentChoices == null)
                return;

            // 背景暗転
            _spriteBatch.Draw(
                TextureManager.GetTexture(TextureID.WhitePixel),
                new Rectangle(0, 0, 800, 480),
                Color.Black * 0.5f
            );

            int itemWidth = 500;
            int itemHeight = 80;
            int itemSpacing = 20;

            int startX = (800 - itemWidth) / 2;

            int totalHeight = _currentChoices.Count * itemHeight
                            + (_currentChoices.Count - 1) * itemSpacing;

            int startY = (480 - totalHeight) / 2;

            for (int i = 0; i < _currentChoices.Count; i++)
            {
                int y = startY + i * (itemHeight + itemSpacing);
                var rect = new Rectangle(startX, y, itemWidth, itemHeight);

                Color tint = (i == _cursorIndex) ? Color.Gold : Color.White;
                _choiceWindow.Draw(rect, tint);

                var text = $"{i + 1}. {_currentChoices[i].Text}";
                var size = font.MeasureString(text);

                float tx = rect.Center.X - size.X / 2;
                float ty = rect.Center.Y - size.Y / 2;

                _spriteBatch.DrawString(font, text, new Vector2(tx, ty), Color.White);
            }
        }

        public void SetDialogue(string speaker, string text, SpriteFontBase font)
        {
            _currentPage = 0;
            _visibleCharacters = 0;
            _typeTimer = 0f;

            Speaker = speaker;
            Text = text;

            // どのウィンドウを使うか決める
            var window = string.IsNullOrEmpty(speaker)
                ? _narrationWindow
                : _dialogueWindow;

            var wrapped = WrapText(font, text, window.TextArea.Width);
            _pages = PaginateLines(wrapped, 3);
        }

        public bool NextPage()
        {
            if (_currentPage < _pages.Count - 1)
            {
                _currentPage++;
                _visibleCharacters = 0;
                _typeTimer = 0f;
                return true;
            }
            return false;
        }

        private void DrawStringWithSpacing(SpriteFontBase font, string text, Vector2 position, Color color, float spacing)
        {
            float x = position.X;

            foreach (char c in text)
            {
                // 1文字描画
                _spriteBatch.DrawString(font, c.ToString(), new Vector2(x, position.Y), color);

                // 文字幅 + 追加スペース
                x += font.MeasureString(c.ToString()).X + spacing;
            }
        }

        public void SkipToPageEnd()
        {
            _visibleCharacters = GetCurrentPageTotalCharacters();
        }

        public void SetChoices(List<ChoiceOption> choices)
        {
            _currentChoices = choices;
            _choiceWindow = new ChoiceWindowRenderer(_spriteBatch);
            _cursorIndex = 0; // ← カーソル初期化
        }

        public void MoveCursor(int delta)
        {
            if (_currentChoices == null || _currentChoices.Count == 0)
                return;

            _cursorIndex += delta;

            if (_cursorIndex < 0)
                _cursorIndex = _currentChoices.Count - 1;

            if (_cursorIndex >= _currentChoices.Count)
                _cursorIndex = 0;
        }
    }
}