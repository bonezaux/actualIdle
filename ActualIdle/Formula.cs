using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {
    /// <summary>
    /// A Formula calculates a value from itself, a given number (x), and the forest.
    ///  Everything in formulas is given in forest values
    /// </summary>
    public class Formula {
        public Formula() {

        }

        public virtual double Calculate(double number, Forest forest) => 0;
    }

    class FormulaLinear : Formula {

        public string BaseValue { get; private set; }
        public string Proportionality { get; private set; }

        public FormulaLinear(string baseValue, string proportionality) {
            this.BaseValue = baseValue;
            this.Proportionality = proportionality;
        }

        public override double Calculate(double number, Forest forest) {
            return forest.GetValue(BaseValue) + forest.GetValue(Proportionality) * number;
        }
    }
}
