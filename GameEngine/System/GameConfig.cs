using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.System
{
    public static class GameConfig
    {
        public enum TextureID
        {
            // キャラ画像
            Garen0,

            //敵画像
            Enemy1,

            //スキル画像
            GarenE1,
            GarenR,
            // エフェクト用
            Slash
        }

        public enum SoundID
        {
            Hit,
            Ult,
            E,
            Voice1,
            Voice2
        }

        public enum FontID
        {
            Main
        }

        //public static class Player
        //{
        //    public const float TargetWidth = 100f;
        //    public const float RotationSpeed = 10f;
        //    public const float HitRadius = 400f;

        //    public const float WiperSpeed = 2f;
        //    public const float WiperAngle = 0.5f;

        //    public const float SpinDuration = 3.0f;
        //    public const float SpinRotationSpeed = 20f;
        //    public const float SpinRadiusMultiplier = 2.0f;
        //}

        public static class Enemy
        {
            public const float TargetWidth = 75f;
            public const float RespawnTime = 1f;
            public const float SpawnOffset = 50f;
            public const int MinSpeed = 50;
            public const int MaxSpeed = 100;
            public const int MoneyPerKill = 5;
            public const int MaxEnemyCount = 150;
        }

        public static class System
        {
            public const double MoneyInterval = 5.0;
            public const int MoneyPerTick = 2;
        }

        public static class UI
        {
            // 画面下からの基準高さ
            public const float BottomMargin = 80f;

            // 左右の余白
            public const float SideMargin = 20f;

            // スキルアイコン
            public const float SkillIconSize = 32f;
            public const float SkillIconSpacing = 60f;

            // スキルバーの横幅（アイコン数に応じて中央揃えに使う）
            public const float SkillBarWidth = 200f;

            // 色設定
            public static readonly Color MoneyTextColor = Color.White;
            public static readonly Color CooldownMaskColor = new Color(0, 0, 0, 180);
        }

        public static class Debug
        {
            public const int CircleSegments = 256;
            public const float CircleRadius = 10f;
            public const float CircleLineThickness = 2f;
        }
    }
}
