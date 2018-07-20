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
        public static List<Path> paths = new List<Path>();

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
                if (!Show)
                    return false;
                result = forest.TestRequirements(Requirements);
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

        
        public string ShowRequirements { get; set; }
        /// <summary>
        /// Whether the Path is even shown. If it isn't shown, it also counts as not unlocked. You don't have to double things.
        /// </summary>
        public bool Show {
            get {
                if (!forest.TestRequirements(ShowRequirements))
                    return false;
                bool result = true;
                foreach (codeInject c in Injects["shown"]) {
                    if (!c(forest, this, null).GetBool())
                        result = false;
                }
                return result;
            }

            set {
                throw new NotImplementedException();
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="forest"></param>
        /// <param name="name"></param>
        /// <param name="descText">Description given of the Path when chosen. Should contain any requirements that are not necessary for it even being shown</param>
        /// <param name="requirements">Requirements for picking the Path. the "unlocked" injects do the same.</param>
        /// <param name="showRequirements">Requirements for having the Path even shown in picking. The "shown" injects do the same.</param>
        public Path(Forest forest, string name, string descText, string requirements = "", string showRequirements = "") {
            paths.Add(this);
            this.forest = forest;
            Bosses = new List<Fighter>();
            Name = name;
            DescText = descText;
            EndBranch = null;
            Injects = new Dictionary<string, List<codeInject>>();
            Requirements = requirements;
            ShowRequirements = showRequirements;
            Injects["unlocked"] = new List<codeInject>();
            Injects["shown"] = new List<codeInject>();
        }

        public void AddBoss(Fighter boss) {
            Bosses.Add(boss);
        }

        public void Echo() {
            Console.WriteLine(Name + ": " + DescText + (Unlocked ? "" : " [Locked]"));
        }

        public int Length() {
            return Bosses.Count;
        }
    }
}
