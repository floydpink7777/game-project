using GameEngine.System.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.GameData.DataStore
{
    public static class PlayerInitValStore
    {
        public static Dictionary<string, PlayerInitVal> Items { get; private set; }

        public static void Load()
        {
            Items = new Dictionary<string, PlayerInitVal>();

            Console.WriteLine(Directory.GetCurrentDirectory());

            // 1. 単一ファイル（配列）方式
            var arrayPath = "GameData/StoreJson/player_inti_val.json";
            if (File.Exists(arrayPath))
            {
                var list = JsonLoader.LoadJson<List<PlayerInitVal>>(arrayPath);
                foreach (var item in list)
                    Items[item.Id] = item;
            }

            // 2. 複数ファイル方式
            var dirPath = "GameData/StoreJson/InitVals";
            if (Directory.Exists(dirPath))
            {
                var dict = JsonDirectoryLoader.LoadJsonDirectory<PlayerInitVal>(
                    dirPath,
                    p => p.Id
                );

                // 配列方式よりこちらを優先（後勝ち）
                foreach (var kv in dict)
                    Items[kv.Key] = kv.Value;
            }
        }

        public static PlayerInitVal Get(string id) => Items[id];
    }

    public class PlayerInitVal
    {
        public string Id { get; set; }

        public string Name { get; set; }
        
        public string Gender { get; set; }

        public int Age { get; set; }
    }
}
