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
        public Dictionary<string, Upgrade> Upgrades { get; private set; }
        public Dictionary<string, double> Values { get; private set; }
        /// <summary>
        /// The Druids xp in all skills specified in the Statics statlist. Levels are at specific intervals, which will probably be changed at some point.
        /// </summary>
        public Dictionary<string, double> Xp { get; private set; }
        public List<Item> Items { get; private set; }
        public List<Modifier> Modifiers { get; private set; }
        public Fighter Boss { get; private set; }
        public bool Running { get; private set; }
        /// <summary>
        /// The Forest's current path.
        /// </summary>
        public Path CurPath { get; private set; }
        /// <summary>
        /// The current boss number on the path. Starts at zero
        /// </summary>
        public int CurBoss { get; private set; }

        /// <summary>
        /// Controls whether the Druid is currently fighting a boss.
        /// </summary>
        public bool Fighting { get; set; }
        /// <summary>
        /// What calculation step we're currently at. Every 5th is a second
        /// </summary>
        public int Count { get; private set; }

        public Forest() : base(0, 0, 0, "Druid", null, null, "!Btrue") {
            Growths = new Dictionary<string, Growth>();
            Doables = new Dictionary<string, Doable>();
            Trophies = new Dictionary<string, Trophy>();
            Upgrades = new Dictionary<string, Upgrade>();
            Values = new Dictionary<string, double>();
            Xp = new Dictionary<string, double>();
            Items = new List<Item>();
            Modifiers = new List<Modifier>();
            Modifiers.Add(new Modifier("BaseStats", modifiersA: new Dictionary<string, double>() {
                { "HealthRegen", 0.2 }
                }));
            foreach (string skill in Statics.skills) {
                Xp[skill] = 100;
            }

            Running = true;
            Fighting = false;
            Values["DefeatedBosses"] = 0;
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

        public override void Lose() {
            Console.WriteLine("You were defeated!");
            Boss.Hp = Boss.MaxHp;
            Fighting = false;
            Hp = 0;
        }

        public void loop() {

            foreach (KeyValuePair<string, Growth> entry in Growths) {
                entry.Value.Loop();
            }
            foreach (KeyValuePair<string, Trophy> entry in Trophies) {
                entry.Value.Loop();
            }
            foreach (KeyValuePair<string, Upgrade> entry in Upgrades) {
                entry.Value.Loop();
            }
            foreach (Item item in Items) {
                item.Loop(this);
            }
            foreach (KeyValuePair<string, Doable> entry in Doables) {
                entry.Value.Loop();
            }

            if (Fighting && Count % 5 == 0) {
                Boss.FightLoop(this);
                if(Fighting) { // Only damage the boss if the boss didn't kill the Druid first.
                    Console.WriteLine("You dealt " + Boss.Name + " " + (Attack - Boss.Defense) + " damage!");
                    Boss.Hp -= (Attack - Boss.Defense);
                    if (Boss.Hp <= 0) {
                        WinBattle();
                    }
                }
            }

            Dictionary<string, double> stats = GetStats();
            Defense = stats["Defense"];
            Attack = stats["Attack"];
            MaxHp = stats["Health"];
            Hp += stats["HealthRegen"] / 5.0;
            if (Hp > MaxHp) {
                Hp = MaxHp;
            }
        }

        /// <summary>
        /// Called when the battle against the current boss is won. Can be used to insta-win a boss battle too, if necessary.
        /// </summary>
        public void WinBattle() {
            Boss.Lose();
            Values["Defeated" + Boss.Name] = 1;
            Values["DefeatedBosses"] += 1;
            Boss = null;
            Fighting = false;
            CurBoss++;
            Console.WriteLine("CB:" + CurBoss);
            if (CurBoss >= CurPath.Length()) {
                Console.WriteLine("You're through this path now..");
            } else {
                Boss = CurPath.Bosses[CurBoss];
                Console.WriteLine("Changing boss:" + Boss);
            }
        }

        /// <summary>
        /// Sets the currently traveled path.
        /// </summary>
        /// <param name="path"></param>
        public void SetPath(Path path) {
            CurPath = path;
            CurBoss = 0;
            Boss = CurPath.Bosses[CurBoss];
        }

        public void StartFighting() {
            if(Boss == null) {
                Console.WriteLine("You don't have a next boss to fight right now.");
                return;
            }
            Console.WriteLine();
            Console.WriteLine("You are now fighting " + Boss.Name + "!");
            Fighting = true;
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

        public void AddTrophy(Trophy trophy) {
            Trophies.Add(trophy.Name, trophy);
        }

        public void AddUpgrade(Upgrade upgrade) {
            Upgrades.Add(upgrade.Name, upgrade);
        }

        public void AddXp(string skillName, double xp) {
            if (Statics.skills.Contains(skillName)) {
                Xp[skillName] += xp*GetValue("XpMod"+ skillName);
            }
        }

        public void AddModifier(Modifier modifier) {
            Modifiers.Add(modifier);
        }

        public void RemoveModifier(Modifier modifier) {
            Modifiers.Remove(modifier);
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
                int lvl = (int)GetValue("lvl" + skill);
                double nextXp = Math.Pow(1.1, lvl + 1) * 100;
                Console.WriteLine(skill + "\tlvl " + GetValue("lvl" + skill) + "\t" + Math.Round(Xp[skill], 2) + "/ " + nextXp + " xp");
            }
        }

        public void ListAvailableUpgrades() {
            string owned = "";
            string unowned = "";
            foreach (KeyValuePair<string, Upgrade> entry in Upgrades) {
                if(entry.Value.Unlocked) {
                    if (entry.Value.Owned)
                        owned += ", " + entry.Key;
                    else
                        unowned += ", " + entry.Key;
                }
            }
            
            Console.WriteLine(" --- Available Upgrades --- ");
            if(unowned.Length > 0) {
                Console.WriteLine(unowned.Substring(2));
            } else {
                Console.WriteLine("N/A");
            }
                Console.WriteLine(" ----- Owned Upgrades ----- ");
            if (owned.Length > 0) {
                Console.WriteLine(owned.Substring(2));
            } else {
                Console.WriteLine("N/A");
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

        public void PickPath() {
            if(CurBoss < CurPath.Bosses.Count) {
                Console.WriteLine("You are not at the end of a path right now.");
                return;
            } else if(CurPath.EndBranch == null) {
                Console.WriteLine("There's no branch from here..");
            }
            Path nextPath = CurPath.EndBranch.PickPath();
            if (nextPath != null) {
                SetPath(nextPath);
            }
        }

        public void EchoPath() {
            if(CurPath != null) {
                CurPath.Echo();
            } else {
                Console.WriteLine("You're not currently on a path.");
            }
        }

        public void ListModifiers() {
            Console.WriteLine(" --- Modifiers ---");
            foreach(Modifier modifier in Modifiers) {
                modifier.Echo();
            }
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
                    return (int)Math.Log(Xp[value.Substring(3)]/100, 1.1);
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
            } else if (Values.ContainsKey(value)) {
                return Modifier.Modify(Modifiers, value, Values[value]);
            }

            return 0;

        }

        public void ChangeValue(string value, double change) {
            if (Values.ContainsKey(value))
                Values[value] += change;
            else
                Values[value] = change;
        }

        /// <summary>
        /// Tests whether a giiven string requirement holds true. Used when Lambda expressions are overkill.
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
            else if (compare == "!=")
                return value1 != value2;
            else {
                Console.WriteLine("Illegal comparator " + compare);
                return false;
            }
        }

        /// <summary>
        /// Runs TestRequirement for every line in the string. Returns true if they're all true, otherwise false.
        /// </summary>
        /// <param name="requirements"></param>
        /// <returns></returns>
        public bool TextRequirements(string requirements) {
            if (requirements == "")
                return true;
            string[] requirementList = requirements.Split('\n');
            foreach(string requirement in requirementList) {
                if (!TestRequirement(requirement)) return false;
            }
            return true;
        }

        public void Calculation() {
            Count = 0;
            while (Running) {
                Thread.Sleep(200);
                Count++;
                loop();
            }
            Console.WriteLine("Terminating");
        }
    }
}
