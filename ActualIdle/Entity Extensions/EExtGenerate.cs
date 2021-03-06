﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle.Entity_Extensions {
    public class EExtGenerate : EExt {
        /// <summary>
        /// What growths this growth increases every tick by just existing
        /// </summary>
        public (string, Formula)[] AddedGrowths { get; set; }
        public override string Name => E.EEXT_GENERATE;
        public override string ShortDescription { get {
                string result = "";
                for (int loop = 0; loop < AddedGrowths.Length; loop++) {
                    double add = AddedGrowths[loop].Item2.Calculate(Entity.Amount, Entity.Forest);
                    add = Modifier.Modify(Entity.Forest.Modifiers.Values, E.GAIN, add);
                    if (add > 0)
                        result += ", " +Statics.GetDisplayNumber(add) + " " + AddedGrowths[loop].Item1 + "/t" + " (" + Math.Round(add * 100 / Entity.Forest.Income, 3) + "%)";
                }
                return result;
            } }


        public EExtGenerate((string, Formula)[] addedGrowths) => AddedGrowths = addedGrowths;

        public override void Loop()
        {
            if (!(Entity.Unlocked && Entity.Amount > 0))
                return;
            Forest forest = Entity.Forest;
            for (int loop = 0; loop < AddedGrowths.Length; loop++)
            {
                forest.Entities[AddedGrowths[loop].Item1].Amount += Modifier.Modify(forest.Modifiers.Values, E.GAIN, AddedGrowths[loop].Item2.Calculate(Entity.Amount, forest));
            }
        }

    }
}
