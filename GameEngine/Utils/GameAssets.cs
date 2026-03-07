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
            //{ TextureID.Garen0, "images/Garen_0" },
            //{ TextureID.Enemy1, "images/enemy_1" },
            //{ TextureID.GarenE1, "images/Garen_E1" },
            //{ TextureID.GarenR, "images/Garen_R" },
            //{ TextureID.Slash, "Effects/Slash" }
        };

        public static readonly Dictionary<SoundID, string> SoundPaths = new()
        {
            //{ SoundID.Hit, "Sounds/SE/maou_se_battle14" },
            //{ SoundID.Ult, "Sounds/SE/urt" },
            //{ SoundID.E, "Sounds/SE/e" },
            //{ SoundID.Voice1, "Sounds/SE/Voice1" },
            //{ SoundID.Voice2, "Sounds/SE/Voice2" },
        };

        public static readonly Dictionary<FontID, string> FontPaths = new()
        {
              { FontID.Main, "Content/Fonts/NotoSansJP-Regular.ttf" }
        };

        public static string GetTexturePath(TextureID id)
        {
            return TexturePaths[id];
        }

        public static string GetSoundPath(SoundID id)
        {
            return SoundPaths[id];
        }

        public static void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            //TextureManager.Load(content, graphicsDevice);
            //SoundManager.Load(content);
            FontManager.Load(content, graphicsDevice);
        }
    }
}
