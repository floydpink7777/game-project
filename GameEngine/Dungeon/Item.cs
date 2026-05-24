using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static GameEngine.System.GameConfig;

namespace GameEngine.Dungeon
{
    public class Item
    {
        public Vector2 Position;
        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, 16, 16);

        public ItemInstance Instance { get; }

        public Item(Vector2 pos, ItemInstance instance)
        {
            Position = pos;
            Instance = instance;
        }
    }

    // アイテムのテンプレートを定義
    public record ItemTemplate(
        string BaseName,              // 基本名
        string BaseDescription,       // 基本説明文
        string IconPath,              // 見た目
        ItemForm Form,                // 形状（Potion, Scroll, Sword…）
        int BaseWeight,               // 基本重量
        int BasePrice,                // 基本価格
        List<UseBehavior> DefaultUse, // 基本の使い方
        List<Enchant>? BaseEnchants,  // 効果
        List<ItemTag>? BaseTags       // タグなど
    );

    // アイテムの効果を定義
    public class Enchant
    {

    }

    // アイテムのタグを定義
    public class ItemTag
    {

    }

    // アイテムテンプレート + 生成時に付与するエンチャント等 = アイテムの実体
    public class ItemInstance
    {
        public ItemTemplate Template { get; }

        // 表示名
        public string DisplayName { get; private set; }
        // 表示説明
        public string DisplayDescription { get; private set; }

        public List<ItemType> Type { get; set; }
        public MaterialType Material { get; set; }
        public List<Enchant> Enchants { get; }
        public List<ItemTag> Tags { get; }
        public ItemQuality Quality { get; set; }
        public List<UseBehavior> Behaviors { get; }

        public ItemInstance(ItemTemplate template)
        {
            Template = template;

            // BaseEnchants / BaseTags をコピー
            Enchants = new List<Enchant>(template.BaseEnchants ?? Enumerable.Empty<Enchant>());
            Tags = new List<ItemTag>(template.BaseTags ?? Enumerable.Empty<ItemTag>());

            Behaviors = new List<UseBehavior>(template.DefaultUse ?? Enumerable.Empty<UseBehavior>());

            DisplayName = template.BaseName;
            DisplayDescription = template.BaseDescription;

            // 生成時に追加するロジックを

            // ここに生成時の追加属性を加える
            // Enchants.Add(...)
            // Tags.Add(...)
        }

        public bool IsStackableWith(ItemInstance other)
        {
            // テンプレートが違うなら絶対にスタック不可
            if (Template != other.Template)
                return false;

            // エンチャント数が違うならスタック不可
            if (Enchants.Count != other.Enchants.Count)
                return false;

            // タグ数が違うならスタック不可
            if (Tags.Count != other.Tags.Count)
                return false;

            // ★ 中身の比較はまだ未定なのでスキップ
            // 将来ここに Enchant/Tag の比較ロジックを入れる

            return true;
        }
    }

    public static class ItemDB
    {
        public static Dictionary<string, ItemTemplate> Templates { get; private set; }

        public static void LoadFromJson(string path)
        {
            string json = File.ReadAllText(path);

            Templates = JsonConvert.DeserializeObject<Dictionary<string, ItemTemplate>>(json)
                ?? new Dictionary<string, ItemTemplate>();
        }

        public static ItemInstance CreateInstance(ItemTemplate template)
        {
            return new ItemInstance(template);
        }
    }

}
