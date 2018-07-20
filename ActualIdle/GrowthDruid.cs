using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {

    /// <summary>
    /// An object that gives DruidCraft experience when bought. May become obsolete pretty quickly.
    /// </summary>
    public class GrowthDruid : Growth {
        public double Xp { get; private set; }
        public string Skill { get; private set; }
        public bool IncreaseBoughtThings { get; private set; }

        public GrowthDruid(Forest forest, string[] addedThings, Formula[] addedFormulas, string name, Resources price, double xp, string skill="Druidcraft", bool increaseBoughtThings = false) : base(forest, name, addedThings, addedFormulas, price) {
            Xp = xp;
            Skill = skill;
            IncreaseBoughtThings = increaseBoughtThings;
        }

        public override bool Create(int amount, bool percentage=false) {
            if(percentage) {
                return base.Create(amount, percentage);
            }
            if (IncreaseBoughtThings) {
                if (forest.Values["boughtThings"] + amount > forest.GetValue("allowedGrowths")) {
                    Console.WriteLine("You cannot have more than " + forest.GetValue("allowedGrowths") + " growths!");
                    return false;
                }
            }
            if (base.Create(amount)) {
                forest.Modifiers["Xp Gain"].ModifiersF[Skill + "XpGain"] += Xp * amount * 0.01;
                if(IncreaseBoughtThings)
                    forest.Values["boughtThings"] += amount;
                return true;
            } else
                return false;
        }
    }
}
