using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.System.State
{
    public enum GameState
    {
        Idle,            // 次のノードを進める状態
        WaitingForNext,  // Enter 待ち
        WaitingForChoice, // 選択肢待ち
        AutoNext
    }
}
