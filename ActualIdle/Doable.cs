using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {

    /// <summary>
    /// Something you can do at some point. Something like creating an oak or picking an ally or something like that.
    /// </summary>
    public class Doable {
        public Forest forest { get; private set; }
        public string Name { get; private set; }
        public Resources ResourceChange { get; private set; }
        public string[] Requirements { get; private set; }
        public string Text { get; private set; }
        public string FailText { get; private set; }
        public string[] UnlockedThings { get; private set; }
        public List<string> UnlockedDoables { get; private set; }
        public List<string> UnlockedUnlocks { get; private set; }
        public bool Unlocked { get; private set; }
        public bool RemainUnlocked { get; private set; }

        public Doable(Forest forest, string name, Resources resourceChange, string[] reqs, string text, string failText, string[] unlockedThings, bool remainUnlocked) {
            this.forest = forest;
            Name = name;
            ResourceChange = resourceChange;
            Requirements = reqs;
            Text = text;
            FailText = failText;
            UnlockedThings = unlockedThings;
            UnlockedThings = unlockedThings;
            RemainUnlocked = remainUnlocked;

            UnlockedDoables = new List<string>();
            UnlockedUnlocks = new List<string>();
            Unlocked = false;
        }

        /// <summary>
        /// Adds doables to add when this doable is performed. Likely deprecated.
        /// </summary>
        public void AddUnlockedDoables(string[] doables) {
            foreach (string doable in doables) {
                UnlockedDoables.Add(doable);
            }
        }

        /// <summary>
        /// Adds unlocks to add when this doable is performed. Likely deprecated.
        /// </summary>
        /// <param name="unlocks"></param>
        public void AddUnlockedUnlocks(string[] unlocks) {
            foreach (string unlock in unlocks) {
                UnlockedUnlocks.Add(unlock);
            }
        }

        /// <summary>
        /// Tests whether the requirements are met for this doable.
        /// </summary>
        /// <returns></returns>
        public bool TestRequirements() {
            bool result = true;
            foreach (string requirement in Requirements) {
                if (!forest.TestRequirement(requirement))
                    result = false;
            }
            return result;
        }

        public bool Perform() {
            if ((ResourceChange == null || ResourceChange.CanAfford(forest, 1)) &&
                TestRequirements()) {
                if (ResourceChange != null) {
                    ResourceChange.Print(forest, 1);
                    ResourceChange.Apply(forest, 1);
                }
                Console.WriteLine(Text);
                Unlocked = RemainUnlocked;

                foreach (string unlockedThing in UnlockedThings)
                    forest.Growths[unlockedThing].Unlocked = true;
                return true;
            } else {
                Console.WriteLine(FailText);
                return false;
            }

        }

        internal void Loop() {
            throw new NotImplementedException();
        }
    }
}
