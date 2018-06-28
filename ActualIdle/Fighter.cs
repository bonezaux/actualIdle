using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {

    public class Fighter {
        public double Hp { get; protected set; }
        public double MaxHp { get; protected set; }
        public double Attack { get; protected set; }
        public double Defense { get; protected set; }
        public string Name { get; protected set; }
        public Resources Reward { get; protected set; }
        public Dictionary<string, int> Xp { get; protected set; }
        public string Requirements { get; set; }

        public Fighter(double maxHp, double attack, double defense, string name, Resources reward, Dictionary<string, int> xp, string requirements) {
            MaxHp = maxHp;
            Hp = MaxHp;
            Attack = attack;
            Defense = defense;
            Name = name;
            Reward = reward;
            Xp = xp;
            Requirements = requirements;
        }
    }
}
