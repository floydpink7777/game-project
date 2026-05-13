using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.System.Core
{
    public interface ICollidable
    {
        Vector2 Position { get; set; }
        Vector2 Velocity { get; }
        Rectangle Bounds { get; }
    }
}

