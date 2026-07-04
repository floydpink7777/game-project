using GameEngine.System.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using static GameEngine.System.GameConfig;

namespace GameEngine.Dungeon
{
    public class Enemy : ICollidable
    {
        public Point TilePos;
        public EnemyAnimator Animator;

        public float Speed = 40f;
        public float ChaseSpeed = 100f;


        public string EnemyID;
        public EnemyCategory Category;
        public int Attack;
        public int Defense;

        public EnemyState State = EnemyState.Idle;

        // Idle（徘徊）
        public Vector2 WanderDirection;
        public float WanderTimer;

        // Lost（捜索）
        public float LostTimer;
        public bool JustExitedLost = false;
        public Vector2 LastSeenDirection;
        public Vector2 LastSeenPosition;

        private static Random _rand = new Random();

        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }

        public Rectangle Bounds => new Rectangle(
            (int)Position.X,
            (int)Position.Y,
            32, 32
        );

        public Dictionary<string, DropTable> ItemDropTable { get; set; }

        public DropTable RarityTable { get; set; }

        public Enemy(Point tilePos, Texture2D tex)
        {
            TilePos = tilePos;
            Animator = new EnemyAnimator(tex, 32, 32);
            Position = new Vector2(tilePos.X * 32, tilePos.Y * 32);
        }

        public void Update(GameTime gameTime, Vector2 playerPos, TileMap map)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            switch (State)
            {
                case EnemyState.Idle:
                    UpdateIdle(dt, playerPos, map);
                    break;

                case EnemyState.Chase:
                    UpdateChase(dt, playerPos, map);
                    break;

                case EnemyState.Lost:
                    UpdateLost(dt);
                    break;
            }

            MoveAndCollide(map, dt);

            TilePos = new Point((int)(Position.X / 32), (int)(Position.Y / 32));

            UpdateDirection();
            Animator.Update(gameTime);
        }

        // ============================
        // Idle
        // ============================
        private void UpdateIdle(float dt, Vector2 playerPos, TileMap map)
        {
            if (JustExitedLost)
            {
                JustExitedLost = false;
                WanderTimer = 1.0f;
                Velocity = LastSeenDirection * Speed;
            }
            else
            {
                WanderTimer -= dt;

                if (WanderTimer <= 0)
                {
                    WanderDirection = RandomDirection();
                    WanderTimer = 1.5f;
                }

                Velocity = WanderDirection * Speed;
            }

            if (CanSeePlayer(playerPos, map))
                State = EnemyState.Chase;
        }

        // ============================
        // Chase
        // ============================
        private void UpdateChase(float dt, Vector2 playerPos, TileMap map)
        {
            Vector2 dir = playerPos - Position;

            if (dir.LengthSquared() > 1f)
            {
                dir.Normalize();
                Velocity = dir * ChaseSpeed;

                LastSeenDirection = dir;
                LastSeenPosition = playerPos;
            }

            if (!CanSeePlayer(playerPos, map))
            {
                State = EnemyState.Lost;
                LostTimer = 3.0f;
            }
        }

        // ============================
        // Lost（捜索）
        // ============================
        private void UpdateLost(float dt)
        {
            Vector2 toLast = LastSeenPosition - Position;

            if (toLast.LengthSquared() > 4f)
            {
                toLast.Normalize();
                Velocity = toLast * Speed;
            }
            else
            {
                Velocity = Vector2.Zero;
            }

            LostTimer -= dt;

            if (LostTimer <= 0)
            {
                State = EnemyState.Idle;
                JustExitedLost = true;
            }
        }

        // ============================
        // Collision
        // ============================
        private void MoveAndCollide(TileMap map, float dt)
        {
            Vector2 pos = Position;

            pos.X += Velocity.X * dt;
            Rectangle boundsX = new Rectangle((int)pos.X, (int)Position.Y, Bounds.Width, Bounds.Height);
            if (CollisionResolver.IsColliding(boundsX, map, out int tileX, out int tileY))
            {
                pos.X = (Velocity.X > 0)
                    ? tileX * map.TileSize - Bounds.Width
                    : (tileX + 1) * map.TileSize;
            }

            pos.Y += Velocity.Y * dt;
            Rectangle boundsY = new Rectangle((int)pos.X, (int)pos.Y, Bounds.Width, Bounds.Height);
            if (CollisionResolver.IsColliding(boundsY, map, out tileX, out tileY))
            {
                pos.Y = (Velocity.Y > 0)
                    ? tileY * map.TileSize - Bounds.Height
                    : (tileY + 1) * map.TileSize;
            }

            Position = pos;
        }

        // ============================
        // Vision
        // ============================
        private bool CanSeePlayer(Vector2 playerPos, TileMap map)
        {
            float dist = Vector2.Distance(Position, playerPos);
            if (dist > 150f) return false;

            Vector2 forward = (State == EnemyState.Lost)
                ? LastSeenDirection
                : (Velocity.LengthSquared() > 0 ? Vector2.Normalize(Velocity) : WanderDirection);

            Vector2 toPlayer = Vector2.Normalize(playerPos - Position);
            float dot = Vector2.Dot(forward, toPlayer);

            if (dot <= 0.5f) return false;

            return HasLineOfSight(playerPos, map);
        }

        private bool HasLineOfSight(Vector2 target, TileMap map)
        {
            Vector2 start = Position + new Vector2(16, 16);
            Vector2 end = target + new Vector2(16, 16);

            Vector2 dir = end - start;
            float dist = dir.Length();
            dir.Normalize();

            float step = 8f;

            for (float d = 0; d < dist; d += step)
            {
                Vector2 p = start + dir * d;

                int tx = (int)(p.X / map.TileSize);
                int ty = (int)(p.Y / map.TileSize);

                if (tx < 0 || ty < 0 || tx >= map.TileMapData.Width || ty >= map.TileMapData.Height)
                    return false;

                if (map.TileMapData.Tiles[tx, ty] == 6)
                    return false;
            }

            return true;
        }

        // ============================
        // Direction
        // ============================
        private void UpdateDirection()
        {
            Vector2 dir;

            if (State == EnemyState.Lost)
                dir = LastSeenDirection;
            else if (Velocity.LengthSquared() > 0)
                dir = Velocity;
            else
                return;

            float angle = MathF.Atan2(dir.Y, dir.X);

            if (angle > -MathF.PI / 4 && angle <= MathF.PI / 4)
                Animator.Direction = 2;
            else if (angle > MathF.PI / 4 && angle <= 3 * MathF.PI / 4)
                Animator.Direction = 0;
            else if (angle <= -MathF.PI / 4 && angle > -3 * MathF.PI / 4)
                Animator.Direction = 3;
            else
                Animator.Direction = 1;
        }

        public void Draw(SpriteBatch spriteBatch, int tileSize)
        {
            Animator.Draw(spriteBatch, Position);
        }

        private Vector2 RandomDirection()
        {
            // -1〜1 のランダム方向
            float x = (float)(_rand.NextDouble() * 2 - 1);
            float y = (float)(_rand.NextDouble() * 2 - 1);

            Vector2 dir = new Vector2(x, y);

            // 方向がゼロに近い場合は再生成
            if (dir.LengthSquared() < 0.1f)
                return RandomDirection();

            dir.Normalize();
            return dir;
        }

        public void ResolveEnemyCollision(List<Enemy> enemies)
        {
            foreach (var other in enemies)
            {
                if (other == this) continue;

                Rectangle a = this.Bounds;
                Rectangle b = other.Bounds;

                if (a.Intersects(b))
                {
                    Vector2 push = (this.Position - other.Position);
                    if (push.LengthSquared() < 1f)
                        push = new Vector2(1, 0);

                    push.Normalize();
                    this.Position += push * 1.0f;   // 押し返す
                }
            }
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

    public enum EnemyState
    {
        Idle,
        Chase,
        Lost
    }
}
