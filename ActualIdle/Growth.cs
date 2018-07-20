using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
        /// <summary>
        /// What growths this growth increases every tick by just existing
        /// </summary>
        public string[] AddedGrowths { get; set; }
        /// <summary>
        /// How much the growths specified by AddedGrowths are increased by.
        /// </summary>
        public Formula[] AddedFormulas { get; set; }
        public Resources Price { get; set; }
        public string Description { get; set; }

        // THESE ARE RUNTIME VARIABLES
        /// <summary>
        /// Whether the growth is unlocked currently.
        /// </summary>
        public bool Unlocked { get; set; }
        /// <summary>
        /// How many of the growth are currently owned.
        /// </summary>
        public double Amount { get; set; }

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
                    forest.Growths[AddedGrowths[loop]].Amount += Modifier.Modify(forest.Modifiers.Values, "Gain", AddedFormulas[loop].Calculate(Amount, forest));
                }
            }
        }

        /// <summary>
        /// If percentage is used, amount is the percentage of total resources to use.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="percentage"></param>
        /// <returns></returns>
        public virtual bool Create(int amount, bool percentage=false) {
            if (percentage) {
                int resA = Price.GetBuys(forest, amount);
                Console.WriteLine("Buys : " + resA);
                if(resA > 0) {
                    Create(resA);
                }
                return true;
            }
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
            string result = Name + ": " + Statics.GetDisplayNumber(Amount);
            for (int loop = 0; loop < AddedGrowths.Length; loop++) {
                double add = AddedFormulas[loop].Calculate(Amount, forest);
                add = Modifier.Modify(forest.Modifiers.Values, "Gain", add);
                result += ", " + Statics.GetDisplayNumber(add) + " ("+Math.Round(add*100 / forest.Income, 3) +"%)" + AddedGrowths[loop] + "/t";
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

        public void Save(XElement growthElement) {
            XMLUtils.CreateElement(growthElement, "Amount", Math.Round(Amount, 3));
            XMLUtils.CreateElement(growthElement, "Unlocked", Unlocked);
        }

        public void Load(XElement growthElement) {
            Amount = XMLUtils.GetDouble(growthElement, "Amount");
            Unlocked = XMLUtils.GetBool(growthElement, "Unlocked");
        }
    }
}