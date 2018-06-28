using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {

    /// <summary>
    /// A Formula that is a limited exponential growth written as (limit*e^x)&(e^x+limit-start)
    /// </summary>
    public class FormulaLimitedExp : Formula {

        /// <summary>
        /// The limit of the logistic function
        /// </summary>
        public string Limit { get; private set; }
        public string Growth { get; private set; }

        public FormulaLimitedExp(string limit, string growth) {
            this.Limit = limit;
            this.Growth = growth;
        }

        public override double Calculate(double number, Forest forest) {
            double x = number;
            double lf = forest.GetValue(Limit);
            double a = forest.GetValue(Growth);
            return lf - Math.Pow(Math.E, -a * x) * lf;
        }
    }
}
