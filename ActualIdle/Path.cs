using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {
    /// <summary>
    /// One Path of bosses to follow. Will end of in a branch where you can pick new paths.
    /// There'll also be some kind of optional bosses.
    /// Has one inject, unlocked, for determining whether it is unlocked.
    /// </summary>
    public class Path : IEntity{
        public List<Fighter> Bosses { get; private set; }
        public string Name { get; private set; }
        public string DescText { get; private set; }

        public Branch EndBranch { get; set; }
        public Forest forest { get; private set; }
        public Dictionary<string, List<codeInject>> Injects { get; private set; }
        public string Requirements { get; set; }
        /// <summary>
        /// Returns whether or not the Upgrade is Unlocked. Cannot be set.
        /// </summary>
        public bool Unlocked {
            get {
                bool result = true;
                result = forest.TextRequirements(Requirements);
                foreach (codeInject c in Injects["unlocked"]) {
                    if (!c(forest, this, null).GetBool())
                        result = false;
                }
                return result;
            }

            set {
                throw new NotImplementedException();
            }
        }
        

        public Path(Forest forest, string name, string descText, string requirements) {
            this.forest = forest;
            Bosses = new List<Fighter>();
            Name = name;
            DescText = descText;
            EndBranch = null;
            Injects = new Dictionary<string, List<codeInject>>();
            Requirements = requirements;
            Injects["unlocked"] = new List<codeInject>();
        }

        public void AddBoss(Fighter boss) {
            Bosses.Add(boss);
        }

        public void Echo() {
            Console.WriteLine(Name + ": " + DescText);
        }

        public int Length() {
            return Bosses.Count;
        }
    }
}
