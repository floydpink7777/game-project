using GameEngine.Events.RuntimeNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.System.Core
{
    public class GameSession
    {
        public string Speaker { get; set; }
        public string Text { get; set; }
        public List<ChoiceOption> Choices { get; set; }

        public void ClearDialogue()
        {
            Speaker = "";
            Text = "";
        }

        public void ClearChoices()
        {
            Choices = null;
        }
    }
}
