using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Dungeon
{
    public class Camera2D
    {
        public Vector2 Position;
        public float Zoom = 1f; // ← 追加（1.0 = 等倍）

        public Matrix GetMatrix()
        {
            return
                Matrix.CreateTranslation(-Position.X, -Position.Y, 0f) *
                Matrix.CreateScale(Zoom, Zoom, 1f);
        }

        public void Follow(Vector2 target, int screenWidth, int screenHeight)
        {
            Position = target - new Vector2(screenWidth / (2f * Zoom), screenHeight / (2f * Zoom));
        }
    }

}
