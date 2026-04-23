using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.System.Core
{

    public static class JsonLoader
    {
        public static T LoadJson<T>(string path)
        {
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }

    public static class JsonDirectoryLoader
    {
        public static Dictionary<string, T> LoadJsonDirectory<T>(string folderPath, Func<T, string> getId)
        {
            var dict = new Dictionary<string, T>();

            foreach (var file in Directory.GetFiles(folderPath, "*.json"))
            {
                try
                {
                    var data = JsonLoader.LoadJson<T>(file);
                    var id = getId(data);

                    if (string.IsNullOrEmpty(id))
                    {
                        Console.WriteLine($"[JsonDirectoryLoader] Invalid ID in file: {file}");
                        continue;
                    }

                    if (dict.ContainsKey(id))
                    {
                        Console.WriteLine($"[JsonDirectoryLoader] Duplicate ID detected: {id} in {file}");
                    }

                    dict[id] = data;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[JsonDirectoryLoader] Failed to load {file}: {ex.Message}");
                }
            }

            return dict;
        }
    }
}
