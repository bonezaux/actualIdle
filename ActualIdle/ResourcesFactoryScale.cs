using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {

    /// <summary>
    /// A factory that creates a Resources object based on a scalar and a Resources table. These are multiplied together to create the final Resources table.
    /// </summary>
    public class ResourcesFactoryScale {

        public Dictionary<string, Formula> Table { get; private set; }

        public ResourcesFactoryScale(Dictionary<string, Formula> table) {
            Table = table;
        }

        public Resources CreateResources(double scale, Forest forest) {
            Dictionary<string, double> resTable = new Dictionary<string, double>();
            foreach (KeyValuePair<string, Formula> entry in Table) {
                resTable[entry.Key] = entry.Value.Calculate(scale, forest);
            }

            return new Resources(resTable);
        }
    }
}
