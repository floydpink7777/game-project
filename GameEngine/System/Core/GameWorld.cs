using GameEngine.GameData.Npc;
using GameEngine.GameData.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace GameEngine.System.Core
{
    public class GameWorld
    {
        public Dictionary<string, NPC> NPCs { get; set; } = new();
        public Player Player { get; set; }
        //public Dictionary<string, Background> Backgrounds { get; set; } = new();
        //public Dictionary<string, Item> Items { get; set; } = new();
    }
}
