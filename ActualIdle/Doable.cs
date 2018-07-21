using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ActualIdle {

    /// <summary>
    /// Something you can do at some point. Something like creating an oak or picking an ally or something like that.
    /// </summary>
    public class Doable : IEntity {
        public Forest forest { get; private set; }
        public string Name { get; private set; }
        public Resources ResourceChange { get; private set; }
        public string Requirements { get; set; }
        public string Text { get; private set; }
        public string FailText { get; private set; }
        public bool Unlocked { get; set; }
        public bool RemainUnlocked { get; private set; }
        public string ManaCost { get; set; }
        /// <summary>
        /// Doables have two injects:
        ///  - loop: called on every loop
        ///  - perform: Called when successfully performed.
        ///  - tooltip: Only one allowed, will be written next to the Doable when tooltip is shown.
        /// </summary>
        public Dictionary<string, List<codeInject>> Injects { get; set; }

        public Doable(Forest forest, string name, Resources resourceChange, string reqs, string text, string failText, bool remainUnlocked, string manaCost) {
            this.forest = forest;
            Name = name;
            ResourceChange = resourceChange;
            Requirements = reqs;
            Text = text;
            FailText = failText;
            RemainUnlocked = remainUnlocked;
            
            Unlocked = false;
            Injects = new Dictionary<string, List<codeInject>>();
            Injects["loop"] = new List<codeInject>();
            Injects["perform"] = new List<codeInject>();
            Injects["tooltip"] = new List<codeInject>();
            ManaCost = manaCost;
        }

        /// <summary>
        /// Tests whether the requirements are met for this doable.
        /// </summary>
        /// <returns></returns>
        public bool TestRequirements() {
            return forest.TestRequirements(Requirements);
        }

        public bool Perform() {
            if(forest.Mana < forest.GetValue(ManaCost)) {
                Console.WriteLine("Not enough mana! " + Name + " costs " + forest.GetValue(ManaCost));
                return false;
            }
            if ((ResourceChange == null || ResourceChange.CanAfford(forest, 1)) &&
                TestRequirements()) {
                forest.SpendMana(forest.GetValue(ManaCost));
                if (ResourceChange != null) {
                    ResourceChange.Print(forest, 1);
                    ResourceChange.Apply(forest, 1);
                }
                foreach (codeInject c in Injects["perform"]) {
                    c(forest, this, null);
                }
                Console.WriteLine(Text);
                Unlocked = RemainUnlocked;
                
                return true;
            } else {
                Console.WriteLine(FailText);
                return false;
            }

        }

        public void Loop() {
            foreach(codeInject c in Injects["loop"]) {
                c(forest, this, null);
            }
        }

        public void Save(XElement doableElement) {
            XMLUtils.CreateElement(doableElement, "Unlocked", Unlocked);
        }

        public void Load(XElement doableElement) {
            Unlocked = XMLUtils.GetBool(doableElement, "Unlocked");
        }

        public string GetTooltip() {
            string result = Name;
            if(Injects["tooltip"].Count > 0) {
                result += " " + Injects["tooltip"][0](forest, this, null).GetString();
            }
            return result;
        }
    }
}
