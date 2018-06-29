using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {
    /// <summary>
    /// An upgrade.
    /// When it is bought, the value Upgrade[Name]Bought
    /// Has three injects, unlocked, bought and ownedLoop.
    ///  - unlocked returns a bool and all must be true for it to be unlocked.
    ///  - bought is run when the Upgrade is bought.
    ///  - ownedLoop is run every loop when the Upgrade is owned.
    /// </summary>
    public class Upgrade : IEntity {

        /// <summary>
        /// Text shown before you buy an upgrade.
        /// </summary>
        public string PreDescriptionText { get; private set; }
        /// <summary>
        /// Description of an upgrade after you buy it.
        /// </summary>
        public string PostDescriptionText { get; private set; }
        /// <summary>
        /// Name of upgrade.
        /// </summary>
        public string Name { get; private set; }
        public Forest forest { get; private set; }
        /// <summary>
        /// Price to buy the Upgrade.
        /// </summary>
        public Resources Price { get; private set; }
        /// <summary>
        /// The code injects for the upgrade.
        /// Has three injects, unlocked, bought and ownedLoop.
        ///  - unlocked returns a bool and all must be true for it to be unlocked.
        ///  - bought is run when the Upgrade is bought.
        ///  - ownedLoop is run every loop when the Upgrade is owned.
        /// </summary>
        public Dictionary<string, List<codeInject>> Injects { get; private set; }

        /// <summary>
        /// Returns whether or not the Upgrade is Unlocked. Cannot be set.
        /// </summary>
        public bool Unlocked {
            get {
                bool result = true;
                foreach(codeInject c in Injects["unlocked"]) {
                    if (!c(forest, this, null).GetBool())
                        result = false;
                }
                return result;
            }

            set {
                throw new NotImplementedException();
            }
        }
        public bool Owned { get; private set; }

        public Upgrade(Forest forest, string name, string preDescriptionText, string postDescriptionText, Resources price) {
            this.forest = forest;
            Name = name;
            PreDescriptionText = preDescriptionText;
            PostDescriptionText = postDescriptionText;
            Price = price;
            Injects = new Dictionary<string, List<codeInject>>();
            Injects["unlocked"] = new List<codeInject>();
            Injects["bought"] = new List<codeInject>();
            Injects["ownedLoop"] = new List<codeInject>();
        }
 

        public void Loop() {
            if(Owned) { // Runs ownedLoop injects if the Upghrade is owned.
                foreach(codeInject c in Injects["ownedLoop"]) {
                    c(forest, this, null);
                }
            }
        }

        public void Buy() {
            if(Price.CanAfford(forest, 1)) {
                Console.WriteLine("You bought the upgrade " + Name + " for");
                Price.Print(forest, 1);
                Price.Apply(forest, 1);
                foreach(codeInject c in Injects["bought"]) {
                    c(forest, this, null);
                }
                Owned = true;
            }
        }

        public void Echo() {
            Console.WriteLine(" --- " + Name + " --- [" + (Owned ? "Owned" : "Available") + "]");
            if (Owned)
                Console.WriteLine(PostDescriptionText);
            else {
                Console.WriteLine(PreDescriptionText);
                Price.Print(forest, 1);
            }
        }
    }
}
