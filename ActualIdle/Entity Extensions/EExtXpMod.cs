using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle.Entity_Extensions {
    public class EExtXpMod : EExt {
        
        public double Xp { get; set; }
        public string Skill { get; set; }
        public override string Name => E.EEXT_XPMOD;

        /// <summary>
        /// xp is how much in % each one of this item increases the given
        /// </summary>
        /// <param name="xp"></param>
        /// <param name="skill"></param>
        public EExtXpMod(double xp, string skill=E.DRUIDCRAFT) {
            Xp = xp;
            Skill = skill;
        }

        public override void OnAdd(double amount) {
            //Console.WriteLine("Being called..." + (Skill + E.XP + E.GAIN) + "+" + Xp * amount * 0.01 + " f a " + amount);
            //Entity.Forest.Modifiers[E.XP + E.GAIN].ModifiersF[Skill + E.XP + E.GAIN] += Xp * amount * 0.01;
        }
    }
}
