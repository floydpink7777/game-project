using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.System.Core
{
    public enum GameMode
    {
        Title,      // タイトル画面
        NewGame,    // 生まれ選択などの初期設定
        MainGame,   // 本編（ステータス画面やマップなど）
        Dungeon,
    }
}
