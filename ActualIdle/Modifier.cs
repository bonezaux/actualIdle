using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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

        /// <summary>
        /// If any of the modifier lists are to be empty, they should be null, not an empty dictionary!
        /// </summary>
        /// <param name="name"></param>
        /// <param name="modifiersF"></param>
        /// <param name="modifiersA"></param>
        /// <param name="modifiersAAfter"></param>
        public Modifier(string name, Dictionary<string, double> modifiersF = null, Dictionary<string, double> modifiersA = null, Dictionary<string, double> modifiersAAfter = null) {
            this.Name = name;
            this.ModifiersF = modifiersF;
            Debug.Assert(!(modifiersF != null && modifiersF.Count == 0)); //Can't be an empty dictionary.
            this.ModifiersA = modifiersA;
            Debug.Assert(!(modifiersA != null && modifiersA.Count == 0)); //Can't be an empty dictionary.
            this.ModifiersAAfter = modifiersAAfter;
            Debug.Assert(!(modifiersAAfter != null && modifiersAAfter.Count == 0)); //Can't be an empty dictionary.
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

        public void Echo() {
            string res = "";
            res += Name + ": \n";
            if(ModifiersA != null) {
                res += "Base additions: ";
                foreach(KeyValuePair<string, double> modifiedValue in ModifiersA) {
                    res += modifiedValue.Key + " " + (modifiedValue.Value > 0 ? "+": "") + modifiedValue.Value + ", ";
                }
                res = res.Substring(0, res.Length - 2);
                res += "\n";
            }
            if (ModifiersF!= null) {
                res += "Relative modifiers: ";
                foreach (KeyValuePair<string, double> modifiedValue in ModifiersF) {
                    res += modifiedValue.Key + " " + (modifiedValue.Value > 0 ? "+" : "") + (modifiedValue.Value*100) + "%, ";
                }
                res = res.Substring(0, res.Length - 2);
                res += "\n";
            }
            if (ModifiersAAfter != null) {
                res += "After-everything additions: ";
                foreach (KeyValuePair<string, double> modifiedValue in ModifiersAAfter) {
                    res += modifiedValue.Key + " " + (modifiedValue.Value > 0 ? "+" : "") + (modifiedValue.Value) + ", ";
                }
                res = res.Substring(0, res.Length - 2);
                res += "\n";
            }
            Console.WriteLine(res);
        }

        public void Save(XElement modifierElement) {
            if (ModifiersF != null) {
                XElement fElement = XMLUtils.CreateElement(modifierElement, "ModifiersF");
                foreach (KeyValuePair<string, double> sMod in ModifiersF) {
                    XMLUtils.CreateElement(fElement, sMod.Key, sMod.Value);
                }
            }
            
            if(ModifiersA != null) {
                XElement aElement = XMLUtils.CreateElement(modifierElement, "ModifiersA");
                foreach (KeyValuePair<string, double> sMod in ModifiersA) {
                    XMLUtils.CreateElement(aElement, sMod.Key, sMod.Value);
                }
            }

            if (ModifiersAAfter != null) {
                XElement aAfterElement = XMLUtils.CreateElement(modifierElement, "ModifiersAAfter");
                foreach (KeyValuePair<string, double> sMod in ModifiersAAfter) {
                    XMLUtils.CreateElement(aAfterElement, sMod.Key, sMod.Value);
                }
            }
        }

        public void Load(XElement modifierElement) {
            XElement fElement = XMLUtils.GetElement(modifierElement, "ModifiersF");
            if(fElement != null) {
                ModifiersF = new Dictionary<string, double>();
                foreach(XElement childElement in fElement.Elements()) {
                    ModifiersF.Add(XMLUtils.GetName(childElement), double.Parse(childElement.Value));
                }
            }

            XElement aElement = XMLUtils.GetElement(modifierElement, "ModifiersA");
            if (aElement != null) {
                ModifiersA = new Dictionary<string, double>();
                foreach (XElement childElement in aElement.Elements()) {
                    ModifiersA.Add(XMLUtils.GetName(childElement), double.Parse(childElement.Value));
                }
            }

            XElement aAfterElement = XMLUtils.GetElement(modifierElement, "ModifiersAAfter");
            if (aAfterElement != null) {
                ModifiersAAfter = new Dictionary<string, double>();
                foreach (XElement childElement in aElement.Elements()) {
                    ModifiersAAfter.Add(XMLUtils.GetName(childElement), double.Parse(childElement.Value));
                }
            }
        }
    }
}
