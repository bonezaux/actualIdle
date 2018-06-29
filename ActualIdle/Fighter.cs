using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {

    public class Fighter : IEntity {
        public double Hp { get; set; }
        public double MaxHp { get; set; }
        public double Attack { get; set; }
        public double Defense { get; set; }
        public string Name { get; set; }
        public Resources Reward { get; set; }
        public Dictionary<string, int> Xp { get; set; }
        public string Requirements { get; set; }
        public bool Unlocked { get; set; }
        public string Description { get; set; }

        public Fighter(double maxHp, double attack, double defense, string name, Resources reward, Dictionary<string, int> xp, string requirements, string description = null) {
            MaxHp = maxHp;
            Hp = MaxHp;
            Attack = attack;
            Defense = defense;
            Name = name;
            Reward = reward;
            Xp = xp;
            Requirements = requirements;
        }

        public void FightLoop(Fighter fighter) {
            fighter.Hp -= (this.Attack - fighter.Defense);
            if (fighter.Hp <= 0)
                fighter.Lose();
        }

        public virtual void Lose() {
            Console.WriteLine("You defeated " + Name + "!");
        }
    }
}
