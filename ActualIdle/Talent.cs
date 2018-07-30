using ActualIdle.Entity_Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {
    public class Talent : Entity {

        public string Skill { get; private set; }

        /// <summary>
        /// Add requirements with AddRequirements
        /// </summary>
        /// <param name="forest"></param>
        /// <param name="name"></param>
        public Talent(Forest forest, string name, string description, string skill, Modifier modifier=null) :base(forest, name, E.GRP_TALENTS) {
            Description = description;
            if(modifier != null) {
                Add(new EExtModifier(modifier));
            }
            Skill = skill;
            Debug.Assert(Statics.skills.Contains(Skill));
        }

        public override void OnAdd(double amount) {
            base.OnAdd(amount);
            Forest.TalentPoints[Skill] -= 1;
        }

        public override bool Create(int amount, bool percentage = false) {
            if (Forest.TalentPoints[Skill] < 1) {
                Console.WriteLine("You don't have any talent points!");
                return false;
            }
            Amount += 1;
            OnAdd(1);
            Console.WriteLine("You have taken the talent " + Name + "!");
            return true;
        }

        public override void Echo(bool writeDescription = true) {
            Console.WriteLine(Name + "["+Skill+"]");
            Console.WriteLine(Description);
        }

    }
}
