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

        private Trophy(Forest forest, string name, string text)
            : base(forest, name, E.GRP_TROPHIES, 3) {
            Name = name;
            Text = text;
            Unlocked = false;
            this.Forest = forest;
            Injects[E.INJ_ACQUIRE] = new List<CodeInject>();
        }

        /// <summary>
        /// Requirements is the requirements string, triggers which triggers trigger it to be reevaluated.
        /// There are three overloads, because there can be a modifier and not be a modifier, and requirements can be either a string or a func.
        /// </summary>
        /// <param name="forest"></param>
        /// <param name="name"></param>
        /// <param name="text"></param>
        /// <param name="requirements"></param>
        /// <param name="triggers"></param>
        public Trophy(Forest forest, string name, string text, string requirements, params string[] triggers)
            : this(forest, name, text) {
            CodeInject reqInject = Initializer.CreateRequirementInject(requirements);
            foreach(string trigger in triggers) {
                AddTrigger(trigger, reqInject);
            }
        }
        /// <summary>
        /// Overload for using func requirements
        /// </summary>
        /// <param name="forest"></param>
        /// <param name="name"></param>
        /// <param name="text"></param>
        /// <param name="requirements"></param>
        /// <param name="triggers"></param>
        public Trophy(Forest forest, string name, string text, Func<bool> requirements, params string[] triggers)
            : this(forest, name, text) {
            CodeInject reqInject = Initializer.CreateRequirementInject(requirements);
            foreach (string trigger in triggers) {
                AddTrigger(trigger, reqInject);
            }
        }
        /// <summary>
        /// Overload for adding a modifier with string reqs
        /// </summary>
        /// <param name="forest"></param>
        /// <param name="name"></param>
        /// <param name="text"></param>
        /// <param name="requirements"></param>
        /// <param name="modifier"></param>
        /// <param name="triggers"></param>
        public Trophy(Forest forest, string name, string text, Modifier modifier, string requirements, params string[] triggers)
            : this(forest, name, text, requirements, triggers) {
            Add(new EExtModifier(modifier));
        }
        /// <summary>
        /// Overload for adding a modifier with bool reqs
        /// </summary>
        /// <param name="forest"></param>
        /// <param name="name"></param>
        /// <param name="text"></param>
        /// <param name="modifier"></param>
        /// <param name="requirements"></param>
        /// <param name="triggers"></param>
        public Trophy(Forest forest, string name, string text, Modifier modifier, Func<bool> requirements, params string[] triggers)
            : this(forest, name, text, requirements, triggers) {
            Add(new EExtModifier(modifier));
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
            foreach(CodeInject c in Injects[E.INJ_ACQUIRE]) {
                c(Forest, this, new RuntimeValue(3, reset));
            }
            if (HasExtension(E.EEXT_MODIFIER))
                Extensions[E.EEXT_MODIFIER].OnEnable();
        }
    }
}
