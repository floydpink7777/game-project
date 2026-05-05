using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Dungeon
{
    public class SlashEffect
    {
        private Texture2D texture;
        public Vector2 position;
        private float rotation;
        private int frameWidth;
        private int frameHeight;
        private int currentFrame;
        private float timer;
        private float frameTime = 0.05f; // 50ms
        public bool IsPlaying { get; private set; }

        public Rectangle Hitbox
        {
            get
            {
                // 斬撃の描画サイズ（scale 0.2）
                int w = (int)(frameWidth * 0.2f);
                int h = (int)(frameHeight * 0.2f);

                // position は中心なので左上に変換
                return new Rectangle(
                    (int)(position.X - w / 2),
                    (int)(position.Y - h / 2),
                    w,
                    h
                );
            }
        }

        public SlashEffect(Texture2D texture, int frameWidth, int frameHeight)
        {
            this.texture = texture;
            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;
            this.currentFrame = 0;
            this.IsPlaying = false;
        }

        public void Play(Vector2 pos, float rotation = 0f)
        {
            position = pos;
            this.rotation = rotation;
            currentFrame = 0;
            timer = 0f;
            IsPlaying = true;
        }

        public void Update(GameTime gameTime)
        {
            if (!IsPlaying) return;

            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (timer >= frameTime)
            {
                timer -= frameTime;
                currentFrame++;

                if (currentFrame >= 3) // 3フレームで終了
                {
                    IsPlaying = false;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsPlaying) return;

            Rectangle src = new Rectangle(
                frameWidth * currentFrame,
                0,
                frameWidth,
                frameHeight
            );

            float scale = 0.2f;

            // ★ 完全な中心に戻す（scale を origin に掛けない）
            Vector2 origin = new Vector2(
                frameWidth / 2f,
                frameHeight / 2f
            );

            spriteBatch.Draw(
                texture,
                position,
                src,
                Color.White,
                rotation,
                origin,
                scale,
                SpriteEffects.None,
                0f
            );
        }

    }
}
