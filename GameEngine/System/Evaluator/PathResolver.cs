using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.System.Evaluator
{
    public static class PathResolver
    {
        public static string[] Split(string path)
            => path.Split('.');
    }
}
