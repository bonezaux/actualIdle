﻿using System;
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
        public double Stall { get; set; }
        public double Speed { get; set; }
        public string Name { get; set; }
        public Resources Reward { get; set; }
        public Dictionary<string, int> Xp { get; set; }
        public string Requirements { get; set; }
        public bool Unlocked { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// How many extra growths can be created after this fighter is defeated.
        /// </summary>
        public int AddedGrowths { get; set; }
        public int Hesitation { get; set; }

        public Fighter(double maxHp, double attack, double defense, string name, Resources reward, Dictionary<string, int> xp, string requirements, string description = null, int addedGrowths = 1) {
            MaxHp = maxHp;
            Hp = MaxHp;
            Attack = attack;
            Defense = defense;
            Name = name;
            Reward = reward;
            Xp = xp;
            Requirements = requirements;
            Description = description;
            AddedGrowths = addedGrowths;
        }

        public virtual void takeDamage(double damage, Fighter attacker) {
            Hp -= (damage - Defense);
        }

        public void FightLoop(Fighter fighter) {
            fighter.takeDamage(this.Attack, this);
            if (fighter.Hp <= 0)
                fighter.Lose();
        }

        public virtual void Lose() {
        }

        public Fighter Clone() {
            return new Fighter(MaxHp, Attack, Defense, Name, Reward, Xp, Requirements, Description);
        }
    }
}
