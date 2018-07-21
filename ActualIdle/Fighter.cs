using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {

    public class Fighter : IEntity {
        public double Hp { get; set; }
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
        public Dictionary<string, double> Stats;

        public Fighter(double maxHp, double attack, double defense, string name, Resources reward, Dictionary<string, int> xp, string requirements, string description = null, int addedGrowths = 1) {
            Stats = new Dictionary<string, double>();
            Stats[E.HEALTH] = maxHp;
            Stats[E.ATTACK] = attack;
            Stats[E.DEFENSE] = defense;
            Stats[E.STALL] = 0;
            Stats[E.SPEED] = 0;
            Hp = maxHp;
            Name = name;
            Reward = reward;
            Xp = xp;
            Requirements = requirements;
            Description = description;
            AddedGrowths = addedGrowths;
        }

        /// <summary>
        /// Returns damage taken
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="attacker"></param>
        /// <param name="armor"></param>
        /// <returns></returns>
        public virtual double takeDamage(double damage, Fighter attacker, bool armor = true) {
            if(armor)
                damage -= Stats[E.DEFENSE];
            Hp -= damage;
            return damage;
        }

        public void FightLoop(Fighter fighter) {
            fighter.takeDamage(Stats[E.ATTACK], this);
            if (fighter.Hp <= 0)
                fighter.Lose();
        }

        public virtual void Lose() {
        }

        public Fighter Clone() {
            Fighter result = new Fighter(Stats[E.HEALTH], Stats[E.ATTACK], Stats[E.DEFENSE], Name, Reward, Xp, Requirements, Description);
            result.Stats = Stats;
            return result;
        }
    }
}
