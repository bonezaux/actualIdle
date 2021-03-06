﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ActualIdle {

    /// <summary>
    /// Something you can do at some point. Something like creating an oak or picking an ally or something like that.
    /// </summary>
    public class Doable : IPerformer {
        public Forest Forest { get; private set; }
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
        public Dictionary<string, List<CodeInject>> Injects { get; set; }

        public Doable(Forest forest, string name, Resources resourceChange, string reqs, string text, string failText, bool remainUnlocked, string manaCost) {
            this.Forest = forest;
            Name = name;
            ResourceChange = resourceChange;
            Requirements = reqs;
            Text = text;
            FailText = failText;
            RemainUnlocked = remainUnlocked;
            
            Unlocked = false;
            Injects = new Dictionary<string, List<CodeInject>> {
                ["loop"] = new List<CodeInject>(),
                ["perform"] = new List<CodeInject>(),
                ["tooltip"] = new List<CodeInject>()
            };
            ManaCost = manaCost;
        }

        /// <summary>
        /// Tests whether the requirements are met for this doable.
        /// </summary>
        /// <returns></returns>
        public bool TestRequirements() {
            return Forest.TestRequirements(Requirements);
        }

        public bool Perform() {
            if(Forest.Mana < Forest.GetValue(ManaCost)) {
                Console.WriteLine("Not enough mana! " + Name + " costs " + Forest.GetValue(ManaCost));
                return false;
            }
            if ((ResourceChange == null || ResourceChange.CanAfford(Forest, 1)) &&
                TestRequirements()) {
                Forest.SpendMana(Forest.GetValue(ManaCost));
                if (ResourceChange != null) {
                    ResourceChange.Print(Forest, 1);
                    ResourceChange.Apply(Forest, 1);
                }
                foreach (CodeInject c in Injects["perform"]) {
                    c(Forest, this, null);
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
            foreach(CodeInject c in Injects["loop"]) {
                c(Forest, this, null);
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
            if(ManaCost != null && Forest.GetValue(ManaCost) > 0) {
                result += " (" + Forest.GetValue(ManaCost) + "mp)";
            }
            if(Injects["tooltip"].Count > 0) {
                result += " " + (string)Injects["tooltip"][0](Forest, this, null);
            }
            return result;
        }

        public void Trigger(string trigger, params RuntimeValue[] arguments) {
            if (Injects.ContainsKey(trigger)) {
                foreach (CodeInject inj in Injects[trigger]) {
                    inj(Forest, this, arguments);
                }
            }
        }
    }
}
