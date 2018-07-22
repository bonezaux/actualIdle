using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {

    /// <summary>
    /// Indicates a price that increases by the function x^a each time it is bought.
    /// </summary>
    public class ResourcesIncrement : Resources {
        /// <summary>
        /// How much the price is times'd by when it increases. (a in x^a)
        /// </summary>
        public string Inc { get; private set; }
        /// <summary>
        /// Defines what x is in x^a. This is presumed to increase by one every time this is bought, otherwise things will go wrong.
        /// </summary>
        public string XValue { get; private set; }

        public ResourcesIncrement(Dictionary<string, double> table, string inc, string xValue) : base(table) {
            Inc = inc;
            XValue = xValue;
        }

        /// <summary>
        /// Returns what the price of a given purchase will be modified by. 
        /// The modification is equal to Inc^(lift+base)
        /// 
        /// Lift indicates how many more purchases this modifier counts for than the current base.
        /// This is used for getting the price of paying this Resources several times at the same time.
        /// </summary>
        /// <param name="lift"></param>
        /// <param name="forest"></param>
        /// <returns></returns>
        public double Modifier(int lift, Forest forest) {
            return Math.Pow(forest.GetValue(Inc), forest.GetValue(XValue) + lift);
        }

        /// <summary>
        /// Gets how much of a single resource is to be paid when this price is to be paid a specific amount of times..
        /// </summary>
        /// <param name="forest"></param>
        /// <param name="thing"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public double GetThingPrice(Forest forest, string thing, int amount) {
            double thingPrice = Table[thing];
            double result = 0;
            for (int loop = 0; loop < amount; loop++) {
                result += thingPrice * Modifier(loop, forest);
            }
            return result;
        }

        public override bool CanAfford(Forest forest, int amount, double availablePart=1) {
            foreach (KeyValuePair<string, double> entry in Table) {
                if (forest.Entities[entry.Key].Amount*availablePart < GetThingPrice(forest, entry.Key, amount)) {
                    return false;
                }
            }
            return true;
        }

        public override void Apply(Forest forest, int amount) {
            foreach (KeyValuePair<string, double> entry in Table) {
                forest.Entities[entry.Key].Amount -= GetThingPrice(forest, entry.Key, amount);
            }
        }

        public override string Text(Forest forest, int amount) {
            string result = "";
            foreach (KeyValuePair<string, double> entry in Table) {
                result += Statics.GetDisplayNumber(GetThingPrice(forest, entry.Key, amount)) + " ("+Math.Round(GetThingPrice(forest, entry.Key, amount)*100 / forest.Entities["Organic Material"].Amount, 3) +"%) " + entry.Key + "\n";
            }
            return result.Substring(0, result.Length - 1);
        }
    }
}
