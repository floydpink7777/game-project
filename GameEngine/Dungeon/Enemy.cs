using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Dungeon
{
    public class Enemy
    {
        public Point TilePos;
        public EnemyAnimator Animator;

        public Vector2 Position;   // ★ 追加：実際の座標
        public float Speed = 40f;  // ★ 追加：移動速度(px/秒)

        public int Attack;
        public int Defense;

        public Enemy(Point tilePos, Texture2D tex)
        {
            TilePos = tilePos;
            Animator = new EnemyAnimator(tex, 32, 32);

            // ★ タイル座標 → ピクセル座標へ変換
            Position = new Vector2(tilePos.X * 32, tilePos.Y * 32);
        }

        public void Update(GameTime gameTime, Vector2 playerPos)
        {
            Animator.Update(gameTime);

            // ★ プレイヤーへの方向ベクトル
            Vector2 dir = playerPos - Position;

            if (dir.LengthSquared() > 1f)
            {
                dir.Normalize();
                Position += dir * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            // ★ TilePos を更新（描画やFog用）
            TilePos = new Point(
                (int)(Position.X / 32),
                (int)(Position.Y / 32)
            );
        }

        public void Draw(SpriteBatch spriteBatch, int tileSize)
        {
            // ★ Position を使って描画
            Animator.Draw(spriteBatch, Position);
        }
    }


    public class EnemyAnimator
    {
        private Texture2D texture;
        private int frameWidth;
        private int frameHeight;

        private int currentFrame = 0;
        private float timer = 0f;
        private float frameTime = 0.2f; // 1フレームの時間

        public int Direction = 0; // 0=Down, 1=Left, 2=Right, 3=Up

        public EnemyAnimator(Texture2D tex, int frameW, int frameH)
        {
            texture = tex;
            frameWidth = frameW;
            frameHeight = frameH;
        }

        public void Update(GameTime gameTime)
        {
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (timer >= frameTime)
            {
                timer -= frameTime;
                currentFrame = (currentFrame + 1) % 3; // 3フレーム
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            var src = new Rectangle(
                currentFrame * frameWidth,
                Direction * frameHeight,
                frameWidth,
                frameHeight
            );

            spriteBatch.Draw(texture, position, src, Color.White);
        }
    }

}
