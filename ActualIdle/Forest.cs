using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ActualIdle {

    public class Forest : Fighter {
        
        /// <summary>
        /// The Growths available to the druid.
        /// </summary>
        public Dictionary<string, Growth> Growths { get; private set; }
        public Dictionary<string, Doable> Doables { get; private set; }
        public Dictionary<string, Trophy> Trophies { get; private set; }
        public Dictionary<string, double> Values { get; private set; }
        /// <summary>
        /// The Druids xp in all skills specified in the Statics statlist. Levels are at specific intervals, which will probably be changed at some point.
        /// </summary>
        public Dictionary<string, double> Xp { get; private set; }
        public List<Item> Items { get; private set; }
        public List<Modifier> Modifiers { get; private set; }
        public Fighter Boss { get; private set; }
        public bool running { get; private set; }

        public Forest() : base(0, 0, 0, "Druid", null, null, "!Btrue") {
            Growths = new Dictionary<string, Growth>();
            Doables = new Dictionary<string, Doable>();
            Trophies = new Dictionary<string, Trophy>();
            Values = new Dictionary<string, double>();
            Xp = new Dictionary<string, double>();
            Items = new List<Item>();
            Modifiers = new List<Modifier>();
            foreach (string skill in Statics.skills) {
                Xp[skill] = 1;
            }

            running = true;
        }

        /// <summary>
        /// Returns a dictionary of the Druids stats in all the stats specified in Statics.statList
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, double> GetStats() {
            Dictionary<string, double> result = new Dictionary<string, double>();
            foreach (string stat in Statics.statList) {
                result[stat] = 0;
            }

            foreach (KeyValuePair<string, Growth> entry in Growths) {
                Dictionary<string, double> thingStats = Growths[entry.Key].GetStats();
                result = (from t in result.Concat(thingStats)
                          group t by t.Key into g
                          select new { Name = g.Key, Count = g.Sum(kvp => kvp.Value) }).ToDictionary(item => item.Name, item => item.Count);
            }
            result = (from e in result
                      select new { Name = e.Key, Count = Modifier.Modify(Modifiers, e.Key, e.Value) }).ToDictionary(item => item.Name, item => item.Count);
            return result;
        }
        public void loop() {

            foreach (KeyValuePair<string, Growth> entry in Growths) {
                entry.Value.Loop();
            }
            foreach (KeyValuePair<string, Trophy> entry in Trophies) {
                entry.Value.Loop();
            }
            foreach (Item item in Items) {
                item.Loop(this);
            }
            foreach (KeyValuePair<string, Doable> entry in Doables) {
                entry.Value.Loop();
            }
            MaxHp = GetStats()["Health"];
            Hp += GetStats()["HealthRegen"] / 5.0;
            if (Hp > MaxHp) {
                Hp = MaxHp;
            }
        }

        public void AddItem(Item item) {
            Items.Add(item);
            item.Get(this);
        }

        public void AddObject(Growth obj) {
            Growths.Add(obj.Name, obj);
        }

        public void AddDoable(Doable doable) {
            Doables.Add(doable.Name, doable);
        }

        public void addUnlockable(Trophy unlockable) {
            Trophies.Add(unlockable.Name, unlockable);
        }

        public void AddXp(string skillName, double xp) {
            if (Statics.skills.Contains(skillName)) {
                Xp[skillName] += xp;
            }
        }

        /// <summary>
        /// Buys a number of ForestObjects. 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="amount"></param>
        public void BuyObject(string obj, int amount) {
            Growths[obj].Create(amount);
        }

        /// <summary>
        /// Echoes the stats of the next boss. Maybe this should also echo like boss options or something like that.
        /// </summary>
        public void EchoBoss() {
            Console.WriteLine("Boss:");
            if(Boss == null) {
                Console.WriteLine("There is currently no boss.");
                return;
            }
            Console.WriteLine(Boss.Name);
            Console.WriteLine(Boss.Hp);
        }

        public void ListGrowths() {
            int i = 0;
            Console.WriteLine();
            foreach (KeyValuePair<string, Growth> entry in Growths) {
                if (entry.Value.Unlocked) {
                    entry.Value.Echo();
                    i++;
                    if (i == 4) {
                        i = 0;
                        Console.WriteLine();
                    }
                }
            }
        }

        public void ListPrices() {
            foreach (KeyValuePair<string, Growth> entry in Growths) {
                if (Growths[entry.Key].Unlocked) {
                    Growths[entry.Key].EchoPrice();
                }
            }
        }

        public void ListItems() {
            foreach (Item item in Items) {
                item.Echo(this);
            }
        }

        public void ListDoables() {
            Console.WriteLine("You can: ");
            foreach (KeyValuePair<string, Doable> entry in Doables) {
                if (entry.Value.Unlocked)
                    Console.WriteLine("\t" + entry.Key);
            }
        }

        public void ListSkills() {
            foreach (string skill in Statics.skills) {
                Console.WriteLine(skill + "\tlvl " + GetValue("lvl" + skill) + "\t" + Math.Round(Xp[skill], 2) + "xp");
            }
        }

        /// <summary>
        /// Performs a doable, if it exists and is unlocked.
        /// </summary>
        /// <param name="doable"></param>
        public void PerformDoable(string doable) {
            if (Doables.ContainsKey(doable) && Doables[doable].Unlocked)
                Doables[doable].Perform();
            else
                Console.WriteLine("No doable with the name " + doable + " is unlocked");
        }

        /// <summary>
        /// Prints out a ForestObject, if it exists.
        /// </summary>
        /// <param name="obj"></param>
        public void EchoObject(string obj) {
            if (Growths.ContainsKey(obj))
                Growths[obj].Echo();
        }

        /// <summary>
        /// Returns a value after being modified. This may be the start of some kind of mini bytecode.
        /// The given value string may sometime become the basis of this bytecode.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public double GetValue(string value) {
            if (value.StartsWith("lvl")) {
                if (Statics.skills.Contains(value.Substring(3)))
                    return (int)Math.Log10(Xp[value.Substring(3)]);
            } else if (value.StartsWith("count")) {
                if (Growths.ContainsKey(value.Substring(5)))
                    return Growths[value.Substring(5)].Amount;
            } else if (value.StartsWith("item")) {
                if (Items.Select(item => item.Name).ToList().Contains(value.Substring(4)))
                    return 1;
            } else if (value.StartsWith("!I")) {
                return int.Parse(value.Substring(2));
            } else if (value.StartsWith("!F")) {
                return float.Parse(value.Substring(2));
            } else if (value.StartsWith("!B")) {
                return bool.Parse(value.Substring(2)) ? 1 : 0;
            }
            return Modifier.Modify(Modifiers, value, Values[value]);
        }

        public void ChangeValue(string value, double change) {
            if (Values.ContainsKey(value))
                Values[value] += change;
            else
                Values[value] = change;
        }

        /// <summary>
        /// Tests whether a giiven string requirement holds true. Another part that could be a part of a more generalized bytecode-like system, if it ends up making sense.
        /// Maybe lambda expressions or something like that will actually end up making more sense.
        /// Returns the answer.
        /// </summary>
        /// <param name="requirement"></param>
        public bool TestRequirement(string requirement) {
            double value1 = GetValue(requirement.Split('_')[0]);
            string compare = requirement.Split('_')[1];
            double value2 = GetValue(requirement.Split('_')[2]);

            if (compare == ">=")
                return value1 >= value2;
            else if (compare == "<=")
                return value1 <= value2;
            else if (compare == ">")
                return value1 > value2;
            else if (compare == "<")
                return value1 < value2;
            else if (compare == "==")
                return value1 == value2;
            else {
                Console.WriteLine("Illegal comparator " + compare);
                return false;
            }
        }

        public void Calculation() {
            int counter = 0;
            while (running) {
                Thread.Sleep(200);
                counter++;
                loop();
            }
            Console.WriteLine("Terminating");
        }
    }
}
