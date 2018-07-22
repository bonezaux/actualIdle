using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {

    /// <summary>
    /// Indicates a price, and everything is thusly applied negatively. Can be used for positives though, this just needs to use negative values.
    /// </summary>
    public class Resources {
        /// <summary>
        /// Key = name of thing
        /// Value = amount of thing
        /// </summary>
        public Dictionary<string, double> Table { get; private set; }

        /// <summary>
        /// <summary>
        /// Indicates a price, and everything is thusly applied negatively. Can be used for positives though, this just needs to use negative values.
        /// </summary>
        /// </summary>
        /// <param name="table">Which resources will be spent how much.</param>
        public Resources(Dictionary<string, double> table) {
            Table = table;
        }

        /// <summary>
        /// Returns whether the forest can afford this resource by checking whether it has all the necessary resources.
        /// availablePart is used for percentage purchases, it is how much of the forest's resources can be used.
        /// </summary>
        /// <param name="forest"></param>
        /// <param name="amount">How many times it is to be paid for.</param>
        /// <returns></returns>
        public virtual bool CanAfford(Forest forest, int amount, double availablePart = 1) {
            foreach (KeyValuePair<string, double> entry in Table) {
                if (forest.Entities[entry.Key].Amount * availablePart < amount * entry.Value)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Applies the resource change to the 
        /// </summary>
        /// <param name="forest"></param>
        /// <param name="amount"></param>
        public virtual void Apply(Forest forest, int amount) {
            foreach (KeyValuePair<string, double> entry in Table) {
                forest.Entities[entry.Key].Amount -= entry.Value * amount;
            }
        }

        /// <summary>
        /// Makes the Resource print itself out.
        /// </summary>
        /// <param name="forest"></param>
        /// <param name="amount"></param>
        public virtual void Print(Forest forest, int amount) {
            Console.WriteLine(Text(forest, amount));
        }

        public virtual string Text(Forest forest, int amount) {
            string res = "";
            foreach (KeyValuePair<string, double> entry in Table) {
                res += (Statics.GetDisplayNumber(entry.Value * amount) + " ( "+Math.Round((entry.Value * amount)*100/forest.Entities["Organic Material"].Amount, 3) +"%) " + entry.Key) + "\n";
            }
            return res.Substring(0, res.Length - 1);
        }

        /// <summary>
        /// Tells you how many buys it takes for this to cost the given percentage or less of the forest resources.
        /// </summary>
        /// <param name="amount"></param>
        public virtual int GetBuys(Forest forest, int percentage) {
            int i = 1;
            while (true) {
                if (!CanAfford(forest, i, percentage/100D))
                    return i - 1;
                i++;
            }
        }
    }
}
