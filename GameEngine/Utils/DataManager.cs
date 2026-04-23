using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameEngine.GameData.DataStore;

namespace GameEngine.Utils
{
    public static  class DataManager
    {
        public static void Load()
        {
            PlayerInitValStore.Load();
        }
    }
}
