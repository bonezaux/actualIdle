using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle  {
    /// <summary>
    /// An object in the forest, like oaks or bushes. Produce other things by a formula based on how many of them there are, and give stats.
    /// </summary>


    /// <summary>
    /// An object in the forest, like oaks or bushes. Produce other things by a formula based on how many of them there are, and give stats.
    /// </summary>
    public class Growth : IEntity {
        public string Name { get; set; }
        public Forest forest { get; set; }
        public string[] AddedGrowths { get; set; }
        public Formula[] AddedFormulas { get; set; }
        public Resources Price { get; set; }
        public bool Unlocked { get; set; }
        public double Amount { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Code injects:
        ///  loop
        ///  create (called when the Growth is created, args[0] = amount)
        /// </summary>
        public Dictionary<string, List<codeInject>> injects { get; private set; }

        public Growth(Forest forest, string name, string[] addedThings, Formula[] addedFormulas, Resources price) {
            Name = name;
            this.forest = forest;
            AddedGrowths = addedThings;
            AddedFormulas = addedFormulas;
            Price = price;
            Unlocked = false;
            injects = new Dictionary<string, List<codeInject>>();
            injects["loop"] = new List<codeInject>();
            injects["create"] = new List<codeInject>();
            foreach (string stat in Statics.statList) {
                if (!forest.Values.ContainsKey(name + stat)) {
                    forest.Values[name + stat] = 0;
                }
            }
            Amount = 0;
            Description = "";
        }

        public virtual void Loop() {
            foreach (codeInject gci in injects["loop"])
                gci(forest, this, null);
            for (int loop = 0; loop < AddedGrowths.Length; loop++) {
                if (AddedGrowths[loop] != null) {
                    forest.Growths[AddedGrowths[loop]].Amount += AddedFormulas[loop].Calculate(Amount, forest);
                }
            }
        }

        public virtual bool Create(int amount) {
            if (Price.CanAfford(forest, amount)) {
                foreach (codeInject gci in injects["create"])
                    gci(forest, this, new RuntimeValue[] { new RuntimeValue(2, amount) });
                Console.WriteLine("You bought " + amount + " " + Name + " for ");
                Price.Print(forest, amount);
                Price.Apply(forest, amount);
                Amount += amount;
                return true;
            } else {
                Console.WriteLine("You don't have enough to buy " + amount + " " + Name + "! You need");
                Price.Print(forest, amount);
            }
            return false;
        }

        public void Echo(bool writeDescription = false) {
            string result = Name + ": " + Math.Round(Amount, 2);
            for (int loop = 0; loop < AddedGrowths.Length; loop++) {
                result += ", " + Math.Round(AddedFormulas[loop].Calculate(Amount, forest), 2) + " " + AddedGrowths[loop] + "/t";
            }
            Console.WriteLine(result);
            if (writeDescription)
                Console.WriteLine(Description);
        }

        public void EchoPrice() {
            if (Price == null)
                return;
            string result = Name + ": ";
            result += Price.Text(forest, 1);
            Console.WriteLine(result);
        }

        public Dictionary<string, double> GetStats() {
            Dictionary<string, double> result = new Dictionary<string, double>();
            foreach (string stat in Statics.statList) {
                result[stat] = forest.GetValue(Name + stat)*Amount;
            }
            return result;
        }
    }
}