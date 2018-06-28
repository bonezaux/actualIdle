using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ActualIdle {

    class Statics {
        public static string[] statList = new string[] { "Health", "Attack", "HealthRegen" };
        public static string[] skills = new string[] { "Druidcraft" };


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

            val2 *= product;
            return val2;
        }
        /// <summary>
        /// For getting what a list of modifiers modifies a valuename by
        /// Returns an array, [mod added, mod multiply]
        /// </summary>
        /// <param name="modifiers"></param>
        /// <param name="modName"></param>
        /// <returns></returns>
        public double[] GetModification(Modifier[] modifiers, string modName) {

            double sums = 0;
            foreach (Modifier mod in modifiers) {
                sums += mod.GetModA(modName);
            }

            double product = 1;
            foreach (Modifier mod in modifiers) {
                product *= mod.GetModF(modName);
            }

            return new double[] { sums, product };
        }

        /// <summary>
        /// Modifies things
        /// </summary>
        public class Modifier {
            public string Name { get; private set; }
            public Dictionary<string, double> ModifiersF { get; private set; }
            public Dictionary<string, double> ModifiersA { get; private set; }

            public Modifier(string name, Dictionary<string, double> modifiersF, Dictionary<string, double> modifiersA = null) {
                this.Name = name;
                this.ModifiersF = modifiersF;
                this.ModifiersA = modifiersA;
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
        }


        /// <summary>
        /// A Formula calculates a value from itself, a given number (x), and the forest.
        ///  Everything in formulas is given in forest values
        /// </summary>
        public class Formula {
            public Formula() {

            }

            public virtual double Calculate(double number, Forest forest) {
                return 0;
            }
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
            /// </summary>
            /// <param name="forest"></param>
            /// <param name="amount">How many times it is to be paid for.</param>
            /// <returns></returns>
            public virtual bool CanAfford(Forest forest, int amount) {
                foreach (KeyValuePair<string, double> entry in Table) {
                    if (forest.Things[entry.Key] < amount * entry.Value)
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
                    forest.Things[entry.Key] -= entry.Value * amount;
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
                    res += (entry.Value * amount + " " + entry.Key) + "\n";
                }
                return res.Substring(0, res.Length - 1);
            }
        }

        /// <summary>
        /// Indicates a price that increases by the function x^a each time it is bought.
        /// </summary>
        public class ResourcesIncrement : Resources {
            /// <summary>
            /// How much the price is times'd by when it increases. (a in x^a)
            /// </summary>
            public string Inc { get; private set; }
            /// <summary>
            /// Defines what x is in x^a. This is presumed to increase by one every time this is bought, otherwise things will go wrong.
            /// </summary>
            public string XValue { get; private set; }

            public ResourcesIncrement(Dictionary<string, double> table, string inc, string xValue) : base(table) {
                Inc = inc;
                XValue = xValue;
            }

            /// <summary>
            /// Returns what the price of a given purchase will be modified by. 
            /// The modification is equal to Inc^(lift+base)
            /// 
            /// Lift indicates how many more purchases this modifier counts for than the current base.
            /// This is used for getting the price of paying this Resources several times at the same time.
            /// </summary>
            /// <param name="lift"></param>
            /// <param name="forest"></param>
            /// <returns></returns>
            public double Modifier(int lift, Forest forest) {
                return Math.Pow(forest.GetValue(Inc), forest.GetValue(XValue) + lift);
            }

            /// <summary>
            /// Gets how much of a single resource is to be paid when this price is to be paid a specific amount of times..
            /// </summary>
            /// <param name="forest"></param>
            /// <param name="thing"></param>
            /// <param name="amount"></param>
            /// <returns></returns>
            public double GetThingPrice(Forest forest, string thing, int amount) {
                double thingPrice = Table[thing];
                double result = 0;
                for (int loop = 0; loop < amount; loop++) {
                    result += thingPrice * Modifier(loop, forest);
                }
                return result;
            }

            public override bool CanAfford(Forest forest, int amount) {
                foreach (KeyValuePair<string, double> entry in Table) {
                    if (forest.Things[entry.Key] < GetThingPrice(forest, entry.Key, amount)) {
                        return false;
                    }
                }
                return true;
            }

            public override void Apply(Forest forest, int amount) {
                foreach (KeyValuePair<string, double> entry in Table) {
                    forest.Things[entry.Key] -= GetThingPrice(forest, entry.Key, amount);
                }
            }

            public override string Text(Forest forest, int amount) {
                string result = "";
                foreach (KeyValuePair<string, double> entry in Table) {
                    result += Math.Round(GetThingPrice(forest, entry.Key, amount), 2) + " " + entry.Key + "\n";
                }
                return result.Substring(0, result.Length - 1);
            }
        }

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

                return new Statics.Resources(resTable);
            }
        }

        /// <summary>
        /// An object in the forest, like oaks or bushes. Produce other things by a formula based on how many of them there are, and give stats.
        /// </summary>
        public class ForestObject {
            public string Name { get; set; }
            public Forest forest { get; set; }
            public string[] AddedThings { get; set; }
            public Formula[] AddedFormulas { get; set; }
            public Resources Price { get; set; }
            public bool Unlocked { get; set; }

            public ForestObject(Forest forest, string name, string[] addedThings, Formula[] addedFormulas, Resources price) {
                Name = name;
                this.forest = forest;
                forest.Things[name] = 0;
                AddedThings = addedThings;
                AddedFormulas = addedFormulas;
                Price = price;
                Unlocked = false;

                foreach (string stat in Statics.statList) {
                    if (!forest.Values.ContainsKey(name + stat)) {
                        forest.Values[name + stat] = 0;
                    }
                }
            }

            public virtual void Loop() {
                for (int loop = 0; loop < AddedThings.Length; loop++) {
                    if (AddedThings[loop] != null) {
                        forest.Things[AddedThings[loop]] += AddedFormulas[loop].Calculate(forest.Things[Name], forest);
                    }
                }
            }

            public virtual bool Buy(int amount) {
                if (Price.CanAfford(forest, amount)) {
                    Console.WriteLine("You bought " + amount + " " + Name + " for ");
                    Price.Print(forest, amount);
                    Price.Apply(forest, amount);
                    forest.Things[Name] += amount;
                    forest.Values["boughtThings"] += amount;
                    return true;
                } else {
                    Console.WriteLine("You don't have enough to buy " + amount + " " + Name + "! You need");
                    Price.Print(forest, amount);
                }
                return false;
            }

            public void Echo() {
                string result = Name + ": " + Math.Round(forest.Things[Name], 2);
                for (int loop = 0; loop < AddedThings.Length; loop++) {
                    result += ", " + Math.Round(AddedFormulas[loop].Calculate(forest.Things[Name], forest), 2) + " " + AddedThings[loop] + "/t";
                }
                Console.WriteLine(result);
            }

            public void EchoPrice() {
                if (Price == null)
                    return;
                string result = Name + ": ";
                result += Price.Text(forest, 1);
                Console.WriteLine(result);
            }

            public Dictionary<string, double> GetStats() {
                Dictionary<string, double> result = new Dictionary<string, double>();
                foreach (string stat in Statics.statList) {
                    result[stat] = forest.GetValue(Name + stat);
                }
                return result;
            }
        }

        public class DruidObject : ForestObject {
            public double Xp { get; private set; }

            public DruidObject(Forest forest, string[] addedThings, Formula[] addedFormulas, string name, Resources price, double xp) : base(forest, name, addedThings, addedFormulas, price) {
                Xp = xp;
            }

            public override bool Buy(int amount) {
                if (base.Buy(amount)) {
                    forest.AddXp("Druidcraft", amount * Xp * (1 + forest.GetValue("wandlevel") * 0.01));
                    return true;
                } else
                    return false;
            }
        }

        /// <summary>
        /// An inventory Item. You can have as many as you want.
        /// </summary>
        public class Item {
            public static List<Item> itemList = new List<Item>();

            /// <summary>
            /// The way values are changed when this is equipped. Will probably be changed into modifiers or something.
            /// Keys: 
            /// </summary>
            public Modifier Modifier;
            public string Name { get; private set; }
            public string Text { get; private set; }

            public Item(string name, Modifier modifier, string text) {
                Name = name;
                Modifier = modifier;
                Text = text;
            }

            public virtual void Loop(Forest forest) {

            }

            /// <summary>
            /// Called when the Druid attains the Item.
            /// </summary>
            /// <param name="forest"></param>
            public virtual void Get(Forest forest) {
                //TODO: Figure out modifiers
            }

            /// <summary>
            /// Called when the Druid loses the Item.
            /// </summary>
            /// <param name="forest"></param>
            public virtual void Lose(Forest forest) {
                //TODO: Figure out modifiers
            }

            public void Echo(Forest forest) {
                Console.WriteLine(Text);
            }
        }

        /// <summary>
        /// Something you can do at some point. Something like creating an oak or picking an ally or something like that.
        /// </summary>
        public class Doable {
            public Forest forest { get; private set; }
            public string Name { get; private set; }
            public Resources ResourceChange { get; private set; }
            public string[] Requirements { get; private set; }
            public string Text { get; private set; }
            public string FailText { get; private set; }
            public string[] UnlockedThings { get; private set; }
            public List<string> UnlockedDoables { get; private set; }
            public List<string> UnlockedUnlocks { get; private set; }
            public bool Unlocked { get; private set; }
            public bool RemainUnlocked { get; private set; }

            public Doable(Forest forest, string name, Resources resourceChange, string[] reqs, string text, string failText, string[] unlockedThings, bool remainUnlocked) {
                this.forest = forest;
                Name = name;
                ResourceChange = resourceChange;
                Requirements = reqs;
                Text = text;
                FailText = failText;
                UnlockedThings = unlockedThings;
                UnlockedThings = unlockedThings;
                RemainUnlocked = remainUnlocked;

                UnlockedDoables = new List<string>();
                UnlockedUnlocks = new List<string>();
                Unlocked = false;
            }

            /// <summary>
            /// Adds doables to add when this doable is performed. Likely deprecated.
            /// </summary>
            public void AddUnlockedDoables(string[] doables) {
                foreach (string doable in doables) {
                    UnlockedDoables.Add(doable);
                }
            }

            /// <summary>
            /// Adds unlocks to add when this doable is performed. Likely deprecated.
            /// </summary>
            /// <param name="unlocks"></param>
            public void AddUnlockedUnlocks(string[] unlocks) {
                foreach (string unlock in unlocks) {
                    UnlockedUnlocks.Add(unlock);
                }
            }

            /// <summary>
            /// Tests whether the requirements are met for this doable.
            /// </summary>
            /// <returns></returns>
            public bool TestRequirements() {
                bool result = true;
                foreach (string requirement in Requirements) {
                    if (!forest.TestRequirement(requirement))
                        result = false;
                }
                return result;
            }

            public bool Perform() {
                if ((ResourceChange == null || ResourceChange.CanAfford(forest, 1)) &&
                    TestRequirements()) {
                    if (ResourceChange != null) {
                        ResourceChange.Print(forest, 1);
                        ResourceChange.Apply(forest, 1);
                    }
                    Console.WriteLine(Text);
                    Unlocked = RemainUnlocked;

                    foreach (string unlockedThing in UnlockedThings)
                        forest.Objects[unlockedThing].Unlocked = true;
                    return true;
                } else {
                    Console.WriteLine(FailText);
                    return false;
                }

            }

            internal void Loop() {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Will become a trophy somehow
        /// </summary>
        public class Unlockable {
            public string Name { get; private set; }
            public string[] Requirements { get; set; }
            public string Text { get; set; }
            public bool Unlocked { get; set; }
            public Forest forest { get; private set; }

            public Unlockable(Forest forest, string name, string[] requirements, string text) {
                Name = name;
                Requirements = requirements;
                Text = text;
                Unlocked = false;
                this.forest = forest;
            }

            public void Loop() {
                if (!Unlocked) {
                    bool result = true;
                    foreach (string requirement in Requirements)
                        if (!forest.TestRequirement(requirement))
                            result = false;
                    if (result) {
                        Console.WriteLine(Text);
                        Unlocked = true;
                        ///TODO: add trohphy code.
                    }
                }
            }

        }

        public class Fighter {
            public double Hp { get; protected set; }
            public double MaxHp { get; protected set; }
            public double Attack { get; protected set; }
            public double Defense { get; protected set; }
            public string Name { get; protected set; }
            public Resources Reward { get; protected set; }
            public Dictionary<string, int> Xp { get; protected set; }

            public Fighter(double maxHp, double attack, double defense, string name, Resources reward, Dictionary<string, int> xp) {
                MaxHp = maxHp;
                Hp = MaxHp;
                Attack = attack;
                Defense = defense;
                Name = name;
                Reward = reward;
                Xp = xp;
            }
        }


        public class Forest : Fighter {

            /// <summary>
            /// Amount of each thing the Druid has.
            /// </summary>
            public Dictionary<string, double> Things { get; private set; }
            /// <summary>
            /// The function of the things that the amount of are given above. TODO: These two, Things and Objects, should be merged into one. It's stupid having two.
            /// </summary>
            public Dictionary<string, ForestObject> Objects { get; private set; }
            public Dictionary<string, Doable> Doables { get; private set; }
            public Dictionary<string, Unlockable> Unlocks { get; private set; }
            public Dictionary<string, double> Values { get; private set; }
            /// <summary>
            /// The Druids xp in all skills specified in the Statics statlist. Levels are at specific intervals, which will probably be changed at some point.
            /// </summary>
            public Dictionary<string, double> Xp { get; private set; }
            public List<Item> Items { get; private set; }
            public List<Modifier> Modifiers { get; private set; }
            public Fighter Boss { get; private set; }
            public bool running { get; private set; }

            public Forest() : base(0, 0, 0, "Druid", null, null) {
                Things = new Dictionary<string, double>();
                Objects = new Dictionary<string, ForestObject>();
                Doables = new Dictionary<string, Doable>();
                Unlocks = new Dictionary<string, Unlockable>();
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

                foreach (KeyValuePair<string, double> entry in Things) {
                    Dictionary<string, double> thingStats = Objects[entry.Key].GetStats();
                    result = (from t in result.Concat(thingStats)
                              group t by t.Key into g
                              select new { Name = g.Key, Count = g.Sum(kvp => kvp.Value) }).ToDictionary(item => item.Name, item => item.Count);
                }
                result = (from e in result
                          select new { Name = e.Key, Count = Statics.Modify(Modifiers, e.Key, e.Value) }).ToDictionary(item => item.Name, item => item.Count);
                return result;
            }
            public void loop() {

                foreach (KeyValuePair<string, ForestObject> entry in Objects) {
                    entry.Value.Loop();
                }
                foreach (KeyValuePair<string, Unlockable> entry in Unlocks) {
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

            public void AddObject(ForestObject obj) {
                Objects.Add(obj.Name, obj);
            }

            public void AddDoable(Doable doable) {
                Doables.Add(doable.Name, doable);
            }

            public void addUnlockable(Unlockable unlockable) {
                Unlocks.Add(unlockable.Name, unlockable);
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
                Objects[obj].Buy(amount);
            }

            /// <summary>
            /// Echoes the stats of the next boss. Maybe this should also echo like boss options or something like that.
            /// </summary>
            public void EchoBoss() {
                Console.WriteLine("Boss:");
                Console.WriteLine(Boss.Name);
                Console.WriteLine(Boss.Hp);
            }

            public void ListThings() {
                int i = 0;
                Console.WriteLine();
                foreach (KeyValuePair<string, double> entry in Things) {
                    if (Objects[entry.Key].Unlocked) {
                        Objects[entry.Key].Echo();
                        i++;
                        if (i == 4) {
                            i = 0;
                            Console.WriteLine();
                        }
                    }
                }
            }

            public void ListPrices() {
                foreach (KeyValuePair<string, double> entry in Things) {
                    if (Objects[entry.Key].Unlocked) {
                        Objects[entry.Key].EchoPrice();
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
                if (Objects.ContainsKey(obj))
                    Objects[obj].Echo();
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
                } else if (value.StartsWith("thing")) {
                    if (Things.ContainsKey(value.Substring(5)))
                        return Things[value.Substring(5)];
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
                return Statics.Modify(Modifiers, value, Values[value]);
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



        public static void main(string[] args) {

            Statics.Forest forest = new Forest();


            forest.AddObject(new ForestObject(forest, "Organic Material", new string[] { null }, new Formula[] { new Formula() }, null));
            forest.Values["boughtThings"] = 2;
            forest.Values["wandlevel"] = 0;
            forest.Things["Bushes"] = 2;

            // BUSHES
            forest.Values["BushesGain"] = 0.6;
            forest.Values["BushesAttack"] = 0.2;
            forest.Values["BushesHealth"] = 0.2;
            forest.Values["BushesInc"] = 1.1;
            forest.AddObject(new DruidObject(forest, new string[] { "Organic Material" }, new Formula[] { new FormulaLinear("!I0", "BushesGain") }, "Bushes",
                new ResourcesIncrement(new Dictionary<string, double>() { { "Organic Material", 10 } }, "BushesInc", "boughtThings"), 1));

            // OAKS
            forest.Values["OaksGain"] = 2;
            forest.Values["OaksHealth"] = 1;
            forest.Values["OaksInc"] = 1.1;
            forest.AddObject(new DruidObject(forest, new string[] { "Organic Material" }, new Formula[] { new FormulaLinear("!I0", "OaksGain") }, "Oaks",
                new ResourcesIncrement(new Dictionary<string, double>() { { "Organic Material", 50 } }, "OaksInc", "boughtThings"), 3));

            // BIRCHES
            forest.Values["BirchesGain"] = 7;
            forest.Values["BirchesAttack"] = 2;
            forest.Values["BirchesInc"] = 1.1;
            forest.AddObject(new DruidObject(forest, new string[] { "Organic Material" }, new Formula[] { new FormulaLinear("!I0", "BirchesGain") }, "Birches",
                new ResourcesIncrement(new Dictionary<string, double>() { { "Organic Material", 220 } }, "BirchesInc", "boughtThings"), 27));

            // YEWS
            forest.Values["YewsGain"] = 23;
            forest.Values["YewsHealth"] = 4;
            forest.Values["YewsAttack"] = 2;
            forest.Values["YewsInc"] = 1.1;
            forest.AddObject(new DruidObject(forest, new string[] { "Organic Material" }, new Formula[] { new FormulaLinear("!I0", "YewsGain") }, "Yews",
                new ResourcesIncrement(new Dictionary<string, double>() { { "Organic Material", 950 } }, "YewsInc", "boughtThings"), 100));

            // FLOWERS
            forest.Values["FlowersGain"] = 0;
            forest.Values["FlowersHealth"] = 1;
            forest.Values["FlowersHealthRegen"] = 0.2;
            forest.Values["FlowersInc"] = 1.1;
            forest.AddObject(new DruidObject(forest, new string[] { "Organic Material" }, new Formula[] { new FormulaLinear("!I0", "FlowersGain") }, "Flowers",
                new ResourcesIncrement(new Dictionary<string, double>() { { "Organic Material", 2000 } }, "FlowersInc", "boughtThings"), 100));

            // SPIDERS
            forest.Values["SpidersGain"] = 85;
            forest.Values["SpidersAttack"] = 9;
            forest.Values["SpidersInc"] = 1.1;
            forest.AddObject(new DruidObject(forest, new string[] { "Organic Material" }, new Formula[] { new FormulaLinear("!I0", "SpidersGain") }, "Spiders",
                new ResourcesIncrement(new Dictionary<string, double>() { { "Organic Material", 4100 } }, "SpidersInc", "boughtThings"), 150));

            forest.Modifiers.Add(new Modifier("debug", new Dictionary<string, double>() { { "BushesGain", 10000 }, { "OaksGain", 10000 }, { "BirchesGain", 10000 } }));

            forest.Objects["Bushes"].Unlocked = true;
            forest.Objects["Organic Material"].Unlocked = true;
            forest.Things["Bushes"] = 2;

            Item.itemList.Add(new Item("wand", new Modifier("wand", new Dictionary<string, double>(), new Dictionary<string, double>() { { "wandlevel", 1 } }), "Wand\t+1 wand level"));
            forest.AddItem(Item.itemList[0]);

            ThreadStart calculation = new ThreadStart(forest.Calculation);
            Console.WriteLine("Calculation is starting nao.");

            Thread calcThread = new Thread(calculation);
            calcThread.Start();


            while (true) {
                Console.Write("\nWhat do you do? ");
                string l = Console.ReadLine();
                if (l.StartsWith("do")) {
                    if (l.Split(' ').Length == 1)
                        forest.ListDoables();
                    else if (forest.Doables.ContainsKey(l.Substring(3).Trim()))
                        forest.PerformDoable(l.Substring(3));
                    else
                        Console.WriteLine("No doable by name " + l.Substring(3).Trim());
                } else if (l.StartsWith("things")) {
                    forest.ListThings();
                } else if (l.StartsWith("create")) {
                    if (l.Split(' ').Length == 1)
                        forest.ListPrices();
                    else if (forest.Things.ContainsKey(l.Substring(7).Trim())) {
                        string thing = l.Substring(7).Trim();
                        if (forest.Objects[thing].Price == null)
                            Console.WriteLine("This cannot be created.");
                        else {
                            Console.Write("How many? ");
                            int amount = int.Parse(Console.ReadLine());
                            forest.BuyObject(thing, amount);
                        }
                    }
                } else if (l.StartsWith("items")) {
                    forest.ListItems();
                } else if (l.StartsWith("skills")) {
                    forest.ListSkills();
                } else if (l.StartsWith("hp")) {
                    Console.WriteLine(Math.Round(forest.Hp, 2) + " / " + Math.Round(forest.MaxHp, 2));
                } else if (l.StartsWith("fight")) {
                    forest.EchoBoss();
                } else if (l.StartsWith("boss")) {
                    forest.EchoBoss();
                } else if (l.StartsWith("stats")) {
                    foreach (KeyValuePair<string, double> entry in forest.GetStats()) {
                        Console.WriteLine(entry.Key + ": " + entry.Value);
                    }
                }
            }
        }
    }

    public class Program {
        static void Main(string[] args) {
            Statics.main(args);
        }
    }
}
