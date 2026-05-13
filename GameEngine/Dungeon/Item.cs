using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameEngine.System.GameConfig;

namespace GameEngine.Dungeon
{
    public class Item
    {
        public Vector2 Position;
        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, 16, 16);

        public ItemID Type;

        public Item(Vector2 pos, ItemID id)
        {
            Position = pos;
            Type = id;
        }

        public static readonly Dictionary<ItemID, string> ItemDescriptions = new()
        {
            { ItemID.Coin, "どこにでもある普通のコイン" },
            { ItemID.Potion, "HPを20回復するポーション" },
            { ItemID.Key, "どこかの扉を開ける鍵" }
        };
    }
}
