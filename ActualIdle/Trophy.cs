using ActualIdle.Entity_Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ActualIdle {

    /// <summary>
    /// Will become a trophy somehow
    /// - Acquire (When the trophy is acquired, and when it is reapplied on resets - DO NOT ADD SOMETHING THAT WRITES TEXT)
    /// - Requirements (If any of these return false, the trophy is not acquired)
    /// </summary>
    public class Trophy : Entity {
        public string Text { get; set; }

        public Trophy(Forest forest, string name, string requirements, string text, Modifier modifier=null)
            : base(forest, name, E.GRP_TROPHIES, 3) {
            Name = name;
            Text = text;
            Unlocked = false;
            this.Forest = forest;
            Injects[E.INJ_ACQUIRE] = new List<codeInject>();
            if (modifier != null)
                Add(new EExtModifier(modifier));
            Add(new EExtRequirements(requirements));
        }

        public override void Loop() {
            base.Loop();
        }

        public void Apply() {
            Console.WriteLine(Text);
            Unlocked = true;
            Forest.Values["svTrophy" + Name] = 1;
            Amount = 1;
            Reapply(false);
        }

        /// <summary>
        /// If reset is true, this is induced by a reset. Otherwise, it is induced by acquiring the trophy.
        /// This is passed as a param to the ACQUIRE injects.
        /// </summary>
        /// <param name="reset"></param>
        public void Reapply(bool reset = true) {
            foreach(codeInject c in Injects[E.INJ_ACQUIRE]) {
                c(Forest, this, new RuntimeValue(3, reset));
            }
            if (HasExtension(E.EEXT_MODIFIER))
                Extensions[E.EEXT_MODIFIER].OnEnable();
        }
    }
}
