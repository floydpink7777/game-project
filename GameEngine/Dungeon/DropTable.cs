using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Dungeon
{
    public  class DropTable
    {
        public (string id, double weight)[] Entries;

        public DropTable((string id, double weight)[] entries)
        {
            Entries = entries;
        }

        public string Roll()
        {
            double total = 0;
            foreach (var e in Entries) total += e.weight;

            double r = Random.Shared.NextDouble() * total;
            double sum = 0;

            foreach (var e in Entries)
            {
                sum += e.weight;
                if (r <= sum)
                    return e.id;
            }

            return Entries[^1].id;
        }
    }
}
