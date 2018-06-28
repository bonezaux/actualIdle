using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {

    /// <summary>
    /// A Formula that is a logistic growth written as (limit*e^x)&(e^x+limit-start)
    /// </summary>
    public class FormulaLogistic : Formula {

        /// <summary>
        /// The limit of the logistic function
        /// </summary>
        public string Limit { get; private set; }
        public string Speed { get; private set; }
        /// <summary>
        /// The start value of the logistic function
        /// </summary>
        public string Start { get; private set; }

        public FormulaLogistic(string limit, string speed, string start) {
            this.Limit = limit;
            this.Speed = speed;
            this.Start = start;
        }

        public override double Calculate(double number, Forest forest) {
            double x = number;
            double lf = forest.GetValue(Limit);
            double spd = forest.GetValue(Speed);
            double start = forest.GetValue(Start);
            double etox = start * Math.Pow(Math.E, lf * spd * x);
            return (lf * etox) / (etox + lf - start);
        }
    }
}
