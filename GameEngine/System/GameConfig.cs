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
            // 画像
            MessageFrame,
            MessageFrameNoName,
            ChoiceWindow,
            WhitePixel
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

        public enum ItemID
        {
            Coin,
            Potion,
            Key
        }

        public enum EnemyID
        {
            Slime,
            Goblin
        }

        public enum ItemCategory
        {
            Consumable, // 消費アイテム
            Material,   // 素材
            Weapon,     // 武器
            KeyItem     // 重要アイテム
        }

        // アイテムの使い方を定義
        public enum UseBehavior
        {
            Drink,
            Eat,
            Read,
            Throw,
            Equip
        }

        // アイテムの見た目や形状を定義
        public enum ItemForm
        {
            Potion,
            Scroll,
            Sword,
            Shield,
            Armor,
            Herb,
            Ore,
            Ring,
            Staff
        }

        // アイテムのカテゴリ
        public enum ItemType
        {
            Consumable, 
            Equipment
        }

        // 素材
        public enum MaterialType
        {

        }
        // 品質
        public enum ItemQuality
        {

        }

        public enum EnemyCategory
        {
            Slime,
            Goblin,
            Undead,
            Dragon,
            Beast
        }
    }
}
