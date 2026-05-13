using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameEngine.System.GameConfig;

namespace GameEngine.Utils
{
    public static class AssetIDExtensions
    {
        public static string Path(this TextureID id)
        {
            return GameAssets.TexturePaths[id];
        }

        public static string Path(this ItemID id)
        {
            return GameAssets.ItemTexturePaths[id];
        }

        public static string Path(this SoundID id)
        {
            return GameAssets.SoundPaths[id];
        }

        public static string Path(this FontID id)
        {
            return GameAssets.FontPaths[id];
        }
    }
}
