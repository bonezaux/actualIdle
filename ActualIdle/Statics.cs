using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {
    /// <summary>
    /// Contains static values and functions.
    /// </summary>
    public class Statics {
        public static string[] statList = new string[] { "Health", "Attack", "HealthRegen", "Defense", "Soothing", "Speed", "Stall", E.MAXMANA, E.MANAREGEN, E.MANA };
        public static string[] skills = new string[] { "Druidcraft", "Animal Handling", "Soothing", "Alchemy", "Transformation", "Restoration" };
        public const double DCxpLogBase = 1000000; // This determines what base on the virtual total the xp will be calculated from for Druidcraft
        public const double AHxpLogBase = 1000; // This determines what base on the virtual total the xp will be calculated from for Animal Handling
        public const double SxpLogBase = 1000; // This determines what base on the virtual total the xp will be calculated from for Soothing

        /// <summary>
        /// Returns a number equal to leading*10^exponent
        /// </summary>
        /// <param name="leading"></param>
        /// <param name="exponent"></param>
        public static double GetNumber(double leading, int exponent) {
            return leading * Math.Pow(10, exponent);
        }

        /// <summary>
        /// Adds the values of the two dictionaries together.
        /// Reduction reduces the result by the given value for all results.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="addition"></param>
        /// <returns></returns>
        public static Dictionary<string, double> AddDictionaries(Dictionary<string, double> source, Dictionary<string, double> addition, double reduction=0) {
            System.Diagnostics.Debug.Assert(source != null);
            System.Diagnostics.Debug.Assert(addition != null);
            return (from t in source.Concat(addition)
                      group t by t.Key into g
                      select new { Name = g.Key, Count = g.Sum(kvp => kvp.Value) }).ToDictionary(item => item.Name, item => item.Count-reduction);
        }

        /// <summary>
        /// Multiplies the values of the two dictionaries together.
        /// Reduction reduces the result by the given value for all results.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="multiplication"></param>
        /// <returns></returns>
        public static Dictionary<string, double> MultiplyDictionaries(Dictionary<string, double> source, Dictionary<string, double> multiplication) {
            Dictionary<string, double> result = new Dictionary<string, double>();
            System.Diagnostics.Debug.Assert(source != null);
            System.Diagnostics.Debug.Assert(multiplication != null);
            foreach (KeyValuePair<string, double> kvp in source) {
                result[kvp.Key] = kvp.Value;
            }
            foreach (KeyValuePair<string, double> kvp in multiplication) {
                if (result.ContainsKey(kvp.Key)) {
                    result[kvp.Key] = result[kvp.Key] * kvp.Value;
                }
                else {
                    result[kvp.Key] = kvp.Value;
                }
            }
            return result;
        }

        public static string GetDisplayNumber(double number) {
            if (number == 0)
                return "0";
            if (number < 1000)
                return ""+number;
            int log = (int)Math.Log10(number);
            double leading = number / Math.Pow(10, log);
            return Math.Round(leading, 3) + "E" + log;
        }

        /// <summary>
        /// Get the corresponding virtual total amount of organic material to an amount of xp
        /// </summary>
        /// <param name="xp"></param>
        /// <returns></returns>
        public static double GetTotal(double xp, string skill = E.DRUIDCRAFT) {
            if(skill == E.DRUIDCRAFT)
                return Math.Pow(DCxpLogBase, Math.Log(xp / 100, 10)) -1;
            else if(skill == E.ANIMAL_HANDLING)
                return Math.Pow(AHxpLogBase, Math.Log(xp / 100, 10)) - 1;
            else if (skill == E.SOOTHING)
                return Math.Pow(SxpLogBase, Math.Log(xp / 100, 10)) - 1;
            return 0;
        }

        /// <summary>
        /// Gets the gained xp from a given difference in xp given the change in xp.
        /// </summary>
        /// <param name="preXp"></param>
        /// <param name="change"></param>
        /// <returns></returns>
        public static double XpGain(double preXp, double change, string skill = E.DRUIDCRAFT) {
            double preTotal = GetTotal(preXp, skill);
            double logBase = DCxpLogBase;
            if (skill == E.ANIMAL_HANDLING)
                logBase = AHxpLogBase;
            else if (skill == E.ANIMAL_HANDLING)
                logBase = SxpLogBase;
            double res = Math.Log(preTotal+change+1, logBase) - Math.Log(preTotal+1, logBase);
            res = Math.Pow(10, res) * 100 - 100;
            if (res < 0) {
                Console.WriteLine("Reduced XP!" + res +" Pre : "+ (preTotal+1) + "Post:" + (preTotal+change+1) + ", log diff: " + res);
                return 0;
            }
            return res;
        }
    }
}
