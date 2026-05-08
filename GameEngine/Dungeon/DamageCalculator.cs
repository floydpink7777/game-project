using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Dungeon
{
    public static class DamageCalculator
    {
        public static int CalculateDamage(int attack, int defense)
        {
            int dmg = attack - defense;

            // 最低1ダメージ
            if (dmg < 1)
                dmg = 1;

            return dmg;
        }

        public static int CalculateDamage(Adventurer attacker, Enemy defender)
        {
            return CalculateDamage(attacker.Attack, defender.Defense);
        }

        public static int CalculateDamage(Enemy attacker, Adventurer defender)
        {
            return CalculateDamage(attacker.Attack, defender.Defense);
        }
    }

}
