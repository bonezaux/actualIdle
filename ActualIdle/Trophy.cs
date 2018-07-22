using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ActualIdle {

    /// <summary>
    /// Will become a trophy somehow
    /// </summary>
    public class Trophy : IEntity {
        public string Name { get; private set; }
        public string Requirements { get; private set; }
        public string Text { get; set; }
        public bool Unlocked { get; set; }
        public Forest forest { get; private set; }
        /// <summary>
        /// Injects:
        /// - Acquire (When the trophy is acquired, and when it is reapplied on resets - DO NOT ADD SOMETHING THAT WRITES TEXT)
        /// - Requirements (If any of these return false, the trophy is not acquired)
        /// </summary>
        public Dictionary<string, List<codeInject>> Injects { get; private set; }

        public Trophy(Forest forest, string name, string requirements, string text) {
            Name = name;
            Requirements = requirements;
            Text = text;
            Unlocked = false;
            this.forest = forest;
            Injects = new Dictionary<string, List<codeInject>>() {
                [E.INJ_ACQUIRE] = new List<codeInject>(),
                [E.INJ_REQUIREMENTS] = new List<codeInject>()
            };
        }

        public void Loop() {
            if (!Unlocked) {
                bool result = true;
                if(!forest.TestRequirements(Requirements))
                    result = false;
                foreach (codeInject req in Injects[E.INJ_REQUIREMENTS])
                    if (!(bool)req(forest, this, null))
                        result = false;
                if (result) {
                    Apply();
                }
            }
        }

        public void Apply() {
            Console.WriteLine(Text);
            Unlocked = true;
            forest.Values["svTrophy" + Name] = 1;
            Reapply(false);
        }

        /// <summary>
        /// If reset is true, this is induced by a reset. Otherwise, it is induced by acquiring the trophy.
        /// This is passed as a param to the ACQUIRE injects.
        /// </summary>
        /// <param name="reset"></param>
        public void Reapply(bool reset = true) {
            foreach(codeInject c in Injects[E.INJ_ACQUIRE]) {
                c(forest, this, new RuntimeValue(3, reset));
            }
        }

        public void Save(XElement trophyElement) {
            XMLUtils.CreateElement(trophyElement, "Unlocked", Unlocked);
        }

        public void Load(XElement trophyElement) {
            Unlocked = XMLUtils.GetBool(trophyElement, "Unlocked");
        }
    }
}
