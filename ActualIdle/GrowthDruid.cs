using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {

    /// <summary>
    /// An object that gives DruidCraft experience when bought. May become obsolete pretty quickly.
    /// </summary>
    public class DruidObject : Growth {
        public double Xp { get; private set; }

        public DruidObject(Forest forest, string[] addedThings, Formula[] addedFormulas, string name, Resources price, double xp) : base(forest, name, addedThings, addedFormulas, price) {
            Xp = xp;
        }

        public override bool Buy(int amount) {
            if (base.Buy(amount)) {
                forest.AddXp("Druidcraft", amount * Xp * (1 + forest.GetValue("wandlevel") * 0.01));
                return true;
            } else
                return false;
        }
    }
}
