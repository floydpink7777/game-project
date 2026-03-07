using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.System.Input
{
    public class InputManager
    {
        public KeyboardInput Keyboard { get; private set; } = new KeyboardInput();

        public void Update()
        {
            Keyboard.Update();
        }
    }
}
