using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {
    /// <summary>
    /// Modifies things
    /// </summary>
    public class Modifier {
        public string Name { get; private set; }
        /// <summary>
        /// Multipliers to a value
        /// </summary>
        public Dictionary<string, double> ModifiersF { get; private set; }
        /// <summary>
        /// Absolute increments to the resulting value, added before multipliers.
        /// </summary>
        public Dictionary<string, double> ModifiersA { get; private set; }
        /// <summary>
        /// Absolute increments to the resulting value, added after multipliers
        /// </summary>
        public Dictionary<string, double> ModifiersAAfter { get; private set; }

        public Modifier(string name, Dictionary<string, double> modifiersF, Dictionary<string, double> modifiersA = null, Dictionary<string, double> modifiersAAfter = null) {
            this.Name = name;
            this.ModifiersF = modifiersF;
            this.ModifiersA = modifiersA;
            this.ModifiersAAfter = modifiersAAfter;
        }

        public double GetModF(string modName) {
            if (ModifiersF == null)
                return 1;
            if (ModifiersF.ContainsKey(modName))
                return ModifiersF[modName];
            else
                return 1;
        }

        public double GetModA(string modName) {
            if (ModifiersA == null)
                return 0;
            if (ModifiersA.ContainsKey(modName))
                return ModifiersA[modName];
            else
                return 0;
        }
        public double GetModAAfter(string modName) {
            if (ModifiersAAfter == null)
                return 0;
            if (ModifiersAAfter.ContainsKey(modName))
                return ModifiersAAfter[modName];
            else
                return 0;
        }

        /// <summary>
        /// Modifies a value by all modifiers in the supplied list.
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static double Modify(IEnumerable<Modifier> modifiers, string modName, double value) {
            double val2 = value;
            foreach (Modifier mod in modifiers) {
                val2 += mod.GetModA(modName);
            }

            double product = 1;
            foreach (Modifier mod in modifiers) {
                product *= mod.GetModF(modName);
            }

            foreach (Modifier mod in modifiers) {
                val2 += mod.GetModAAfter(modName);
            }

            val2 *= product;
            return val2;
        }
        /// <summary>
        /// For getting what a list of modifiers modifies a valuename by
        /// Returns an array, [mod added, mod multiply, mod added last]
        /// </summary>
        /// <param name="modifiers"></param>
        /// <param name="modName"></param>
        /// <returns></returns>
        public static double[] GetModification(Modifier[] modifiers, string modName) {

            double sums = 0;
            foreach (Modifier mod in modifiers) {
                sums += mod.GetModA(modName);
            }

            double product = 1;
            foreach (Modifier mod in modifiers) {
                product *= mod.GetModF(modName);
            }

            double sums2 = 0;
            foreach (Modifier mod in modifiers) {
                sums += mod.GetModAAfter(modName);
            }

            return new double[] { sums, product, sums2 };
        }
    }
}
