using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace GameEngine.Dungeon
{
    public class Adventurer
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float WalkSpeed = 4.0f;
        public float DashSpeed = 8.0f;

        // 0～3 の方向（スプライトシートの行番号）
        public int Direction;

        public bool IsDashing => Keyboard.GetState().IsKeyDown(Keys.LeftShift);

        public Rectangle Bounds =>
            new Rectangle((int)Position.X, (int)Position.Y, 28, 28);

        public void Update()
        {
            Velocity = Vector2.Zero;

            float speed = IsDashing ? DashSpeed : WalkSpeed;
            var ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.W)) Velocity.Y = -1;
            if (ks.IsKeyDown(Keys.S)) Velocity.Y = 1;
            if (ks.IsKeyDown(Keys.A)) Velocity.X = -1;
            if (ks.IsKeyDown(Keys.D)) Velocity.X = 1;

            // 移動しているときだけ方向を更新
            if (Velocity.LengthSquared() > 0)
            {
                Velocity.Normalize();
                Velocity *= speed;

                // 角度から4方向に変換
                float angle = MathF.Atan2(Velocity.Y, Velocity.X);

                // 右
                if (angle > -MathF.PI / 4 && angle <= MathF.PI / 4)
                {
                    Direction = 2;
                }
                // 下
                else if (angle > MathF.PI / 4 && angle <= 3 * MathF.PI / 4)
                {
                    Direction = 0;
                }
                // 上
                else if (angle <= -MathF.PI / 4 && angle > -3 * MathF.PI / 4)
                {
                    Direction = 3;
                }
                // 左
                else
                {
                    Direction = 1;
                }
            }
        }
    }
}
