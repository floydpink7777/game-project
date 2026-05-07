using GameEngine.Events;
using GameEngine.System.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.System.Logic
{
    public class GameLogic
    {
        private readonly EventManager _events;

        public GameMode Mode { get; private set; } = GameMode.Title;

        public GameLogic(EventManager events)
        {
            _events = events;
        }

        public void GoToTitle()
        {
            Mode = GameMode.Title;
        }

        public void StartNewGame()
        {
            Mode = GameMode.Dungeon;
        }

        public void GoToMainGame()
        {
            Mode = GameMode.MainGame;
        }

        public void GoToDungeon()
        {
            Mode = GameMode.Dungeon;
        }

        public void OnScenarioFinished(string scenarioId)
        {
            switch (scenarioId)
            {
                case "start":
                    // 最初のシナリオが終わった → MainGame へ
                    Mode = GameMode.Title;
                    break;

                case "ending":
                    // エンディング → タイトルへ戻す
                    Mode = GameMode.Title;
                    break;

                default:
                    // デフォルトは MainGame に戻る
                    Mode = GameMode.MainGame;
                    break;
            }
        }

        // -------------------------
        // 毎フレーム更新（最小）
        // -------------------------
        public void Update()
        {
            // 今は何もしない
            // 後で：イベント発火、World更新、モード遷移などを追加
        }
    }
}
