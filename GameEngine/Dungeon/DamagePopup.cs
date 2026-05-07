using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Dungeon
{
    public class DamagePopup
    {
        public Vector2 Position;
        public float Lifetime = 0.6f;   // 表示時間
        public float Time = 0f;
        public int Amount;

        public Color Color = Color.White;


        public DamagePopup(Vector2 pos, int amount, Color color)
        {
            Position = pos;
            Amount = amount;
            Color = color;
        }

        public bool Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Time += dt;

            // 上にふわっと移動
            Position.Y -= 30f * dt;

            // true を返したら削除
            return Time >= Lifetime;
        }

        public void Draw(SpriteBatch sb, SpriteFontBase font)
        {
            float alpha = 1f - (Time / Lifetime);

            sb.DrawString(
                font,
                Amount.ToString(),
                Position,
                Color * alpha
            );
        }
    }
}
