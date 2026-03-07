using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.System.Input
{
    public class KeyboardInput
    {
        private KeyboardState prev;
        private KeyboardState current;

        public void Update()
        {
            prev = current;
            current = Keyboard.GetState();
        }

        public bool Pressed(Keys key)
        {
            return current.IsKeyDown(key) && prev.IsKeyUp(key);
        }

        public bool Down(Keys key)
        {
            return current.IsKeyDown(key);
        }
    }
}
