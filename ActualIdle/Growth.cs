﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {
    /// <summary>
    /// An object in the forest, like oaks or bushes. Produce other things by a formula based on how many of them there are, and give stats.
    /// </summary>


    /// <summary>
    /// An object in the forest, like oaks or bushes. Produce other things by a formula based on how many of them there are, and give stats.
    /// </summary>
    public class Growth {
        public string Name { get; set; }
        public Forest forest { get; set; }
        public string[] AddedGrowths { get; set; }
        public Formula[] AddedFormulas { get; set; }
        public Resources Price { get; set; }
        public bool Unlocked { get; set; }
        public double Amount { get; set; }

        public Growth(Forest forest, string name, string[] addedThings, Formula[] addedFormulas, Resources price) {
            Name = name;
            this.forest = forest;
            AddedGrowths = addedThings;
            AddedFormulas = addedFormulas;
            Price = price;
            Unlocked = false;

            foreach (string stat in Statics.statList) {
                if (!forest.Values.ContainsKey(name + stat)) {
                    forest.Values[name + stat] = 0;
                }
            }
            Amount = 0;
        }

        public virtual void Loop() {
            for (int loop = 0; loop < AddedGrowths.Length; loop++) {
                if (AddedGrowths[loop] != null) {
                    forest.Growths[AddedGrowths[loop]].Amount += AddedFormulas[loop].Calculate(Amount, forest);
                }
            }
        }

        public virtual bool Buy(int amount) {
            if (Price.CanAfford(forest, amount)) {
                Console.WriteLine("You bought " + amount + " " + Name + " for ");
                Price.Print(forest, amount);
                Price.Apply(forest, amount);
                Amount += amount;
                forest.Values["boughtThings"] += amount;
                return true;
            } else {
                Console.WriteLine("You don't have enough to buy " + amount + " " + Name + "! You need");
                Price.Print(forest, amount);
            }
            return false;
        }

        public void Echo() {
            string result = Name + ": " + Math.Round(Amount, 2);
            for (int loop = 0; loop < AddedGrowths.Length; loop++) {
                result += ", " + Math.Round(AddedFormulas[loop].Calculate(Amount, forest), 2) + " " + AddedGrowths[loop] + "/t";
            }
            Console.WriteLine(result);
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