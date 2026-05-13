using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using static GameEngine.System.GameConfig;

namespace GameEngine.Utils
{
    public static class GameAssets
    {
        public static readonly Dictionary<TextureID, string> TexturePaths = new()
        {
            { TextureID.MessageFrame, "images/message_frame" },
            { TextureID.MessageFrameNoName, "images/message_frame_no_name" },
            { TextureID.ChoiceWindow, "images/choice_frame" },
        };

        public static readonly Dictionary<ItemID, string> ItemTexturePaths = new()
        {
            { ItemID.Coin, "images/cupmen_ramen_shoyu_01_open" },
            { ItemID.Potion, "images/icecream_cone_triple_vanilla_strawberry_chocomint" },
        };

        public static readonly Dictionary<SoundID, string> SoundPaths = new();
        public static readonly Dictionary<FontID, string> FontPaths = new()
        {
            { FontID.Main, "Content/Fonts/NotoSansJP-Regular.ttf" }
        };

        public record ItemInfo(
            string Name,
            string Description,
            ItemCategory Category
        );

        public static class ItemDB
        {
            public static readonly Dictionary<ItemID, ItemInfo> Items = new()
            {
                { ItemID.Coin,   new("コイン", "誰が何と言おうと聖剣エクスカリバーです", ItemCategory.Weapon) },
                { ItemID.Potion, new("回復薬", "HPを20回復するポーション", ItemCategory.Consumable) },
                { ItemID.Key,    new("鍵", "どこかの扉を開ける鍵", ItemCategory.KeyItem) },
            };
        }
    }
}
