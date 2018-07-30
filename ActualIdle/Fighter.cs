using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {

    public class Fighter : IPerformer {
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
        public string LootTable;

        public Fighter(double maxHp, double attack, double defense, string name, Resources reward, Dictionary<string, int> xp, string requirements, string description = null, int addedGrowths = 1) {
            Stats = new Dictionary<string, double> {
                [E.HEALTH] = maxHp,
                [E.ATTACK] = attack,
                [E.DEFENSE] = defense,
                [E.STALL] = 0,
                [E.SPEED] = 0
            };
            Hp = maxHp;
            Name = name;
            Reward = reward;
            Xp = xp;
            Requirements = requirements;
            Description = description;
            AddedGrowths = addedGrowths;
        }

        public Fighter AddLootTable(string lootTable) {
            Debug.Assert(Initializer.lootTables.ContainsKey(lootTable));
            LootTable = lootTable;
            return this;
        }

        public void Trigger(string trigger, params RuntimeValue[] arguments) {
            return; //TODO: ADD TRIGGER SYSTEM
        }

        /// <summary>
        /// Returns damage taken
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="attacker"></param>
        /// <param name="armor"></param>
        /// <returns></returns>
        public virtual double TakeDamage(double damage, Fighter attacker, bool armor = true) {
            if(armor)
                damage -= Stats[E.DEFENSE];
            Hp -= damage;
            return damage;
        }

        public void FightLoop(Fighter fighter) {
            fighter.TakeDamage(Stats[E.ATTACK], this);
            if (fighter.Hp <= 0 && Hp > 0)
                fighter.Lose(this);
        }

        public virtual void Lose(Fighter fighter) {
            if(LootTable != null) {
                string loot = Initializer.GetLoot(LootTable);
                if (loot != null)
                    ((Forest)fighter).AddItem(loot, 1);
            }
        }

        public Fighter Clone() {
            Fighter result = new Fighter(Stats[E.HEALTH], Stats[E.ATTACK], Stats[E.DEFENSE], Name, Reward, Xp, Requirements, Description) {
                Stats = Stats
            };
            return result;
        }
    }
}
