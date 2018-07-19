using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ActualIdle {
    public delegate RuntimeValue codeInject(Forest f, IEntity e, RuntimeValue[] arguments); 

    /// <summary>
    /// Contains static values.
    /// </summary>
    class Statics {
        public static string[] statList = new string[] { "Health", "Attack", "HealthRegen", "Defense", "Soothing" };
        public static string[] skills = new string[] { "Druidcraft", "Animal Handling", "Alchemy", "Transformation", "Restoration" };

        /// <summary>
        /// Returns a number equal to leading*10^exponent
        /// </summary>
        /// <param name="leading"></param>
        /// <param name="exponent"></param>
        public static double GetNumber(double leading, int exponent) {
            return leading * Math.Pow(10, exponent);
        }

        public static string GetDisplayNumber(double number) {
            if (number == 0)
                return "0";
            int log = (int)Math.Log10(number);
            double leading = number / Math.Pow(10, log);
            return Math.Round(leading, 3) + "E" + log;
        }
    }

    public class Program {

        public static Path startPath;
        public static bool debug = false;
        public static int debugCountTime = 2;

        public static void init(Forest forest) {

            forest.AddObject(new Growth(forest, "Organic Material", new string[] { null }, new Formula[] { new Formula() }, null));
            forest.Growths["Organic Material"].Unlocked = true;

            forest.Values["wandlevel"] = 0;
            forest.Values["boughtThings"] = 0;
            forest.Values["allowedGrowths"] = 2;

            // BUSHES
            forest.Values["BushesGain"] = 0.6;
            forest.Values["BushesAttack"] = 0.2;
            forest.Values["BushesHealth"] = 0.2;
            forest.Values["BushesInc"] = 1.1;
            forest.AddObject(new GrowthDruid(forest, new string[] { "Organic Material" }, new Formula[] { new FormulaLinear("!I0", "BushesGain") }, "Bushes",
                new ResourcesIncrement(new Dictionary<string, double>() { { "Organic Material", 20 } }, "BushesInc", "countBushes"), 1));
            forest.Growths["Bushes"].Amount = 1;
            forest.Growths["Bushes"].Unlocked = true;
            forest.Growths["Bushes"].Description = "Adds 0.2 attack and 0.2 health each.";

            // OAKS
            forest.Values["OaksGain"] = 2;
            forest.Values["OaksHealth"] = 1;
            forest.Values["OaksInc"] = 1.1;
            forest.AddObject(new GrowthDruid(forest, new string[] { "Organic Material" }, new Formula[] { new FormulaLinear("!I0", "OaksGain") }, "Oaks",
                new ResourcesIncrement(new Dictionary<string, double>() { { "Organic Material", 100 } }, "OaksInc", "countOaks"), 3));
            forest.Growths["Oaks"].Unlocked = true;
            forest.Growths["Oaks"].Description = "Adds 1 health each.";

            // ANTS
            forest.Values["AntsGain"] = 3.6;
            forest.Values["AntsAttack"] = 0.8;
            forest.Values["AntsInc"] = 1.1;
            forest.AddObject(new GrowthDruid(forest, new string[] { "Organic Material" }, new Formula[] { new FormulaLinear("!I0", "AntsGain") }, "Ants",
                new ResourcesIncrement(new Dictionary<string, double>() { { "Organic Material", 220 } }, "AntsInc", "countAnts"), 3, "Animal Handling"));
            forest.Growths["Ants"].injects["loop"].Add((f, g, arguments) => {
                if (f.GetValue("DefeatedBosses") >= 1) {
                    g.Unlocked = true;
                }
                return null;
            });
            forest.Growths["Ants"].Description = "Adds 0.8 attack each. Gives 3xp in Animal handling.";

            // BIRCHES
            forest.Values["BirchesGain"] = 7;
            forest.Values["BirchesDefense"] = 0.25;
            forest.Values["BirchesInc"] = 3;
            forest.AddObject(new GrowthDruid(forest, new string[] { "Organic Material" }, new Formula[] { new FormulaLinear("!I0", "BirchesGain") }, "Birches",
                new ResourcesIncrement(new Dictionary<string, double>() { { "Organic Material", 440 } }, "BirchesInc", "countBirches"), 20, increaseBoughtThings : true));
            forest.Growths["Birches"].injects["loop"].Add((f, g, arguments) => {
                if (f.GetValue("DefeatedBosses") >= 1) {
                    g.Unlocked = true;
                }
                return null;
            });
            forest.Growths["Birches"].Description = "Adds 0.25 defense each. Is a limited growth. Each birch costs 3x the last.";



            // YEWS
            forest.Values["YewsGain"] = 23;
            forest.Values["YewsHealth"] = 4;
            forest.Values["YewsAttack"] = 0;
            forest.Values["YewsInc"] = 1.1;
            forest.AddObject(new GrowthDruid(forest, new string[] { "Organic Material" }, new Formula[] { new FormulaLinear("!I0", "YewsGain") }, "Yews",
                new ResourcesIncrement(new Dictionary<string, double>() { { "Organic Material", 4000 } }, "YewsInc", "countYews"), 40));
            forest.Growths["Yews"].injects["loop"].Add((f, g, arguments) => {
                if (f.GetValue("lvlDruidcraft") >= 10) {
                    g.Unlocked = true;
                }
                return null;
            });
            forest.Growths["Birches"].Description = "Adds 4 health each.";

            // FLOWERS
            forest.Values["FlowersGain"] = 0;
            forest.Values["FlowersHealthRegen"] = 0.2;
            forest.Values["FlowersSoothing"] = 2;
            forest.Values["FlowersInc"] = 1.1;
            forest.AddObject(new GrowthDruid(forest, new string[] { "Organic Material" }, new Formula[] { new FormulaLinear("!I0", "FlowersGain") }, "Flowers",
                new ResourcesIncrement(new Dictionary<string, double>() { { "Organic Material", 2000 } }, "FlowersInc", "countFlowers"), 9));
            forest.Growths["Flowers"].injects["loop"].Add((f, g, arguments) => {
                if (f.GetValue("DefeatedBosses") >= 6) {
                    g.Unlocked = true;
                }
                return null;
            });

            // SPIDERS
            forest.Values["SpidersGain"] = 85;
            forest.Values["SpidersAttack"] = 1;
            forest.AddModifier(new Modifier("SpiderAttackMod", new Dictionary<string, double>() { { "SpidersAttack",  1 } }));
            forest.Values["SpidersInc"] = 1.1;
            forest.AddObject(new GrowthDruid(forest, new string[] { "Organic Material" }, new Formula[] { new FormulaLinear("!I0", "SpidersGain") }, "Spiders",
                new ResourcesIncrement(new Dictionary<string, double>() { { "Organic Material", 1000 } }, "SpidersInc", "countSpiders"), 5, "Animal Handling"));
            forest.Growths["Spiders"].injects["loop"].Add((f, g, arguments) => {
                if (f.GetValue("DefeatedSwallow") > 0) {
                    g.Unlocked = true;
                }
                forest.GetModifier("SpiderAttackMod").ModifiersF["SpidersAttack"] = 1 + 0.05 * forest.GetValue("lvlAnimal Handling");
                return null;
            });

            // DOABLES

            forest.Values["DruidHeal"] = 5;
            forest.Values["RejuvenateCooldownMod"] = 1; //Is a speed, making it higher will make it faster.
            forest.AddDoable(new Doable(forest, "Rejuvenate", null, "", "Rejuvenate!", "Rejuvenate is on cooldown.", true));
            forest.Doables["Rejuvenate"].Injects["perform"].Add((f, g, arguments) => {
                double hpIncrease = forest.GetValue("DruidHeal") * (1 + forest.GetValue("lvlDruidcraft") * 0.1);
                if(f.GetValue("UpgradeSoothing RejuvenationBought") != 0) {
                    hpIncrease += f.GetStats()["Soothing"]* (1+0.02*f.GetValue("lvlDruidcraft"));
                }
                forest.Hp += hpIncrease;
                Console.WriteLine("Gained " + Math.Round(hpIncrease, 2));
                forest.Values["RejuvenateCooldown"] = 50;
                return null;
            });
            forest.Doables["Rejuvenate"].Injects["loop"].Add((f, g, arguments) => {
                if (forest.GetValue("RejuvenateCooldown") > 0) {
                    forest.Values["RejuvenateCooldown"] -= forest.GetValue("RejuvenateCooldownMod");
                }
                return null;
            });
            forest.Doables["Rejuvenate"].Injects["tooltip"].Add((f, g, arguments) => {
                if (forest.GetValue("RejuvenateCooldown") > 0) {
                    return new RuntimeValue(4, "[" + /*Conversion to seconds*/((forest.GetValue("RejuvenateCooldown") / forest.GetValue("RejuvenateCooldownMod")) / 5) + "s CD]");
                }
                return new RuntimeValue(4, "");
            });
            forest.Doables["Rejuvenate"].Unlocked = true;
            forest.Doables["Rejuvenate"].Requirements += "RejuvenateCooldown_<=_0";

            
            forest.AddDoable(new Doable(forest, "Halfhour Offline", null, "", "Halfhour Offline!", "", true));
            forest.Doables["Halfhour Offline"].Injects["perform"].Add((f, g, arguments) => {
                f.TickOffline(5 * 60 * 30);
                return null;
            });
            forest.Doables["Halfhour Offline"].Unlocked = true;

            // TROPHIES

            forest.AddTrophy(new Trophy(forest, "Defeated Ferret",
                new codeInject[] { (f, g, arguments) => {
                    if(f.GetValue("DefeatedFerret") == 1)
                        return new RuntimeValue(3, true);
                    else
                        return new RuntimeValue(3, false);
                } }, "You defeated the Ferret! Good job!"));

            Item.itemList.Add(new Item("wand", new Modifier("wand", null, new Dictionary<string, double>() { { "XpModDruidcraft", 0.01 } }), "Wand\t+1 wand level"));
            forest.AddItem(Item.itemList[0]);

            ThreadStart calculation = new ThreadStart(forest.Calculation);
            Console.WriteLine("Calculation is starting nao.");

            Thread calcThread = new Thread(calculation);
            calcThread.Start();




            startPath = new Path(forest, "Forest Street", "A little path through the forest.", "");
            for(int loop=1; loop<=100; loop++) {
                startPath.AddBoss(generateBoss(loop));
            }

            startPath.AddBoss(new Fighter(10, 2, 0, "Bird", new Resources(new Dictionary<string, double>()), new Dictionary<string, int>(), ""));
            startPath.AddBoss(new Fighter(20, 5, 0, "Ferret", new Resources(new Dictionary<string, double>()), new Dictionary<string, int>(), ""));
            startPath.AddBoss(new Fighter(30, 6, 1, "Red Fox", new Resources(new Dictionary<string, double>()), new Dictionary<string, int>(), ""));
            Branch startBranch = new Branch(forest, "The Forest Cross", "You're at the forest cross. You have a few ways to go now.");
            startPath.EndBranch = startBranch;

            // Bird path
            Path birdPath = new Path(forest, "Bird Sanctuary", "You hear lots of birds chirping down this way.", "");
            birdPath.AddBoss(new Fighter(80, 5, 0, "Magpie", new Resources(new Dictionary<string, double>()), new Dictionary<string, int>(), ""));
            birdPath.AddBoss(new Fighter(190, 9, 0, "Bluebird", new Resources(new Dictionary<string, double>()), new Dictionary<string, int>(), ""));
            Branch birdBossBranch = new Branch(forest, "Birds Grove", "You're in the center of the Bird Sanctuary. There are two central bird nests here.");
            birdPath.EndBranch = birdBossBranch;


            Path sparrowPath = new Path(forest, "Sparrow's Nest", "You see sparrows down this path.", "");
            sparrowPath.AddBoss(new Fighter(750, 15, 0, "Sparrow", new Resources(new Dictionary<string, double>()), new Dictionary<string, int>(), ""));
            Path swallowPath = new Path(forest, "Swallow's Nest", "You see swallows down this path.", "");
            swallowPath.AddBoss(new Fighter(150, 28, 0, "Swallow", new Resources(new Dictionary<string, double>()), new Dictionary<string, int>(), ""));
            Branch birdEndBranch = new Branch(forest, "Low Eckshire Pass", "You've come through the bird's sanctuary into the Low Eckshire Pass.");
            sparrowPath.EndBranch = birdEndBranch;
            swallowPath.EndBranch = birdEndBranch;

            birdBossBranch.Paths.Add(sparrowPath);
            birdBossBranch.Paths.Add(swallowPath);

            startBranch.Paths.Add(birdPath);

            Path testPath2 = new Path(forest, "Nonono", "Not gut", "DefeatedBosses_>=_!I5");
            Path testPath3 = new Path(forest, "Not even shown", "Not gut", "", "DefeatedBosses_>=_!I6");
            startBranch.Paths.Add(testPath2);
            startBranch.Paths.Add(testPath3);



            forest.AddUpgrade(new Upgrade(forest, "Big Birches", "This upgrade improves birch defense by 700%.", "This upgrade improves birch defense by 700%.",
                new Resources(new Dictionary<string, double>() { { "Organic Material", 15000 } }), new Modifier("Big Birches", new Dictionary<string, double>() { { "BirchesDefense", 8 } })));
            forest.Upgrades["Big Birches"].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue("countBirches") >= 4) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });

            // Growth upgrades in order of how many growths you need. If several happen at the same number, they are ordered in the following order:
            // 1. bushes 2. oaks 3. ants 4. birches 5. yews
            CreateGrowthUpgrade(forest, "Growing Birches 1", "Birches", 3, (int)Statics.GetNumber(1, 4), 10); //x10 - 2.1E2 / 0.5 + 1E4
            CreateGrowthUpgrade(forest, "Growing Birches 2", "Birches", 7, (int)Statics.GetNumber(2, 5), 100); //x1000 - 4.9E4 / 4.8 + 2E5

            CreateGrowthUpgrade(forest, "Gnarly Yews 1", "Yews", 15, (int)Statics.GetNumber(3, 5), 4); //x4 -  1.4E3 / 1.2 + 3 E5
            CreateGrowthUpgrade(forest, "Bush Growth 1", "Bushes", 20, 5000, 3); //x3 - 1.8*20  / 1.1 + 5E3 
            CreateGrowthUpgrade(forest, "Oak Growth 1", "Oaks", 20, 3000, 4); //x4 - 1.6E2 / 5.7 + 3E3
            CreateGrowthUpgrade(forest, "Larger Ants 1", "Ants", 20, 10000, 5); //x5 - 3.6E2 / 1.2 + 1E4

            CreateGrowthUpgrade(forest, "Gnarly Yews 2", "Yews", 35, (int)Statics.GetNumber(1, 6), 4); //x16 -  1.2E4 / 1.1 + 1 E6

            CreateGrowthUpgrade(forest, "Bush Growth 2", "Bushes", 50, 20000, 5); //x15 - 4.5E2 / 2.3 + 2.5E4
            CreateGrowthUpgrade(forest, "Oak Growth 2", "Oaks", 50, 80000, 4); //x16 - 1.6E3 / 1.1 + 0.8E5
            CreateGrowthUpgrade(forest, "Larger Ants 2", "Ants", 50, (int)Statics.GetNumber(2, 5), 5); //x25 -  4.5E3 / 2.5 + 2.1E5

            CreateGrowthUpgrade(forest, "Gnarly Yews 3", "Yews", 60, (int)Statics.GetNumber(1, 6), 4); //x64 -  8.8E4 / 1.2 + 2.1 E7

            CreateGrowthUpgrade(forest, "Oak Growth 3", "Oaks", 80, (int)Statics.GetNumber(2, 6), 3); //x48 - 7.68E3 / 2 + 2.1E6
            CreateGrowthUpgrade(forest, "Larger Ants 3", "Ants", 80, (int)Statics.GetNumber(6, 6), 3); //x75 - 2.16E4 / 0.6 + 0.4 E7

            CreateGrowthUpgrade(forest, "Bush Growth 3", "Bushes", 100, (int)Statics.GetNumber(1.5, 6), 4); //x60 - 3.6E3 / 2.7 + 1.5 E6

            CreateGrowthUpgrade(forest, "Oak Growth 4", "Oaks", 120, (int)Statics.GetNumber(0.9, 8), 3); //x240 - 5.8E4 / 0.9 + .9E8
            CreateGrowthUpgrade(forest, "Larger Ants 4", "Ants", 120, (int)Statics.GetNumber(2, 8), 4); //x300 - 1.3E4 / 2.0 + 2.0 E8

            // Soothing upgrades Rejuvenate
            forest.AddUpgrade(new Upgrade(forest, "Soothing Rejuvenation", "This upgrade makes Rejuvenation scale on Soothing, adding Soothing+2%*lvlDruidcraft to the health restoration.", null,
                new Resources(new Dictionary<string, double>() { { "Organic Material", Statics.GetNumber(3, 7) } }), null));
            forest.Upgrades["Soothing Rejuvenation"].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue("countFlowers") >= 80) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });

            forest.SetPath(startPath);

            // DEBUG
            /*forest.SetPath(birdPath);
            forest.Growths["Organic Material"].Amount = 50000;
            forest.Values["DefeatedBosses"] = 3;*/
            // A modifier for debugging
            //forest.Modifiers.Add(new Modifier("debug", new Dictionary<string, double>() { { "BushesGain", 10000 }, { "OaksGain", 10000 }, { "BirchesGain", 10000 },
            //    { "HealthRegen", 10000 }, { "Attack", 10000 }, { "Health", 10000 } }));
        }

        /// <summary>
        /// Creates a new upgrade that increases the gain of a Growth.
        /// name - name of the upgrade
        /// target - name of the growth
        /// amount - how many growths are needed for it
        /// cost - price of the upgrade
        /// multiplier - what modF is added to the growth.
        /// </summary>
        /// <param name="forest"></param>
        /// <param name="name"></param>
        /// <param name="target"></param>
        /// <param name="amount"></param>
        /// <param name="neededAmount"></param>
        /// <param name="cost"></param>
        /// <param name="multiplier"></param>
        public static void CreateGrowthUpgrade(Forest forest, string name, string target, int amount, int cost, double multiplier) {

            forest.AddUpgrade(new Upgrade(forest, name, "This upgrade improves "+target+" gain by "+(multiplier*100-100)+"%.", null,
                new Resources(new Dictionary<string, double>() { { "Organic Material", cost } }), new Modifier(name, new Dictionary<string, double>() { { target+"Gain", multiplier } })));
            forest.Upgrades[name].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue("count"+target) >= amount) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });
        }

        public static Fighter generateBoss(int boss) {
            Random r = new Random(boss);
            double powerMult = 6 * boss * Math.Pow(1.15, boss);
            double hpScale = 0.8 + r.NextDouble() * 0.4;
            double attackScale = 0.8 + r.NextDouble() * 0.4;
            double defenseScale = r.NextDouble() - 0.7;
            double defense = 0;

            if(defenseScale > 0) {
                hpScale -= defenseScale * 3;
                defense = defenseScale * powerMult;
            }

            return new Fighter(powerMult * 10 * hpScale, powerMult * attackScale * 0.5, defense, "Boss " + boss, new Resources(new Dictionary<string, double>()), new Dictionary<string, int>(), "", "");
        }

        public static void reset(Forest forest) {
            foreach(KeyValuePair<string, Growth> entry in forest.Growths) {
                entry.Value.Amount = 0;
            }
            forest.Values.Clear();
            forest.Growths["Bushes"].Amount = 1;
            forest.Values["boughtThings"] = 1;
            forest.SetPath(startPath);
        }
        

        static void Main(string[] args) {
            var culture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = culture;
            Forest forest = new Forest();

            init(forest);

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
                } else if (l.StartsWith("count")) {
                    forest.ListGrowths();
                } else if (l.ToLower().StartsWith("growth")) {
                    if (l.Split(' ').Length == 1)
                        forest.ListGrowths();
                    else {
                        string growth = l.Split(' ')[1];
                        if (!forest.Growths.ContainsKey(growth))
                            Console.WriteLine("No Growth by name " + growth);
                        else
                            forest.Growths[growth].Echo(true);
                    }
                } else if (l.StartsWith("create")) {
                    if (l.Split(' ').Length == 1)
                        forest.ListPrices();
                    else if (forest.Growths.ContainsKey(l.Substring(7).Trim())) {
                        string thing = l.Substring(7).Trim();
                        if (forest.Growths[thing].Price == null)
                            Console.WriteLine("This cannot be created.");
                        else {
                            Console.Write("How many? ");
                            l = Console.ReadLine();
                            int res = 0;
                            if (l.EndsWith("%")) {
                                if (int.TryParse(l.Substring(0, l.Length - 1), out res)) {
                                    forest.BuyObject(thing, res, true);
                                }
                            } else {
                                res = 0;
                                if (int.TryParse(l, out res)) {
                                    if (res > 0)
                                        forest.BuyObject(thing, res);
                                }
                            }
                        }
                    }
                } else if (l.StartsWith("items")) {
                    forest.ListItems();
                } else if (l.StartsWith("skills")) {
                    forest.ListSkills();
                } else if (l.StartsWith("hp")) {
                    Console.WriteLine(Math.Round(forest.Hp, 2) + " / " + Math.Round(forest.MaxHp, 2));
                } else if (l.StartsWith("fight")) {
                    forest.StartFighting();
                } else if (l.StartsWith("boss")) {
                    forest.EchoBoss();
                } else if (l.StartsWith("stats")) {
                    foreach (KeyValuePair<string, double> entry in forest.GetStats()) {
                        if (entry.Value != 0)
                            Console.WriteLine(entry.Key + ": " + entry.Value);
                    }
                    if (debug && forest.Boss != null)
                        Console.WriteLine("DEBUG VALUE (hp*(attack-f.def)): " + forest.Hp * (forest.Attack - forest.Boss.Defense));
                } else if (l.StartsWith("upgrades")) {
                    forest.ListAvailableUpgrades();
                } else if (l.StartsWith("upgrade")) { // Get data on a specific Upgrade and maybe buy it
                    if (l.Split(' ').Length == 1)
                        forest.ListAvailableUpgrades();
                    else {
                        string upgrade = l.Substring(8);
                        if (!forest.Upgrades.ContainsKey(upgrade) || !forest.Upgrades[upgrade].Unlocked)
                            Console.WriteLine("No upgrade by name " + upgrade);
                        else {
                            forest.Upgrades[upgrade].Echo();
                            if (!forest.Upgrades[upgrade].Owned) { //Let you buy the upgrade
                                Console.WriteLine("Do you want to buy it? [Y/N]");
                                string answer = Console.ReadLine();
                                if (answer.Trim().ToLower().Equals("y")) {
                                    forest.Upgrades[upgrade].Buy();
                                } else {
                                    Console.WriteLine("Okay :)");
                                }
                            }
                        }
                    }
                } else if (l.StartsWith("modifiers")) {
                    forest.ListModifiers();
                } else if (l.StartsWith("branch")) {
                    forest.PickPath();
                } else if (l.StartsWith("path")) {
                    forest.EchoPath();
                } else if (l.StartsWith("debug")) {
                    debug = !debug;
                    if (l.Split(' ').Length > 1) {
                        int res = 0;
                        if (int.TryParse(l.Split(' ')[1], out res)) {
                            if (res > 0)
                                debugCountTime = res;
                        }
                    }
                } else if (l.StartsWith("time")) {
                    Console.WriteLine(forest.Count/5 + "s, offline " + forest.OfflineTicks/5 + "s");
                } else if (l.StartsWith("save")) {
                    forest.Save();
                } else if (l.StartsWith("load")) {
                    forest.Load();
                } else {
                    printHelp();
                }
            }
        }

        public static void printHelp() {
            Console.WriteLine("This is the help screen.");
            Console.WriteLine(" --- Commands ---");
            Console.WriteLine(" - do <doable>: Perform a doable or get the list of doables.");
            Console.WriteLine(" - count: Get a count of owned Growths.");
            Console.WriteLine(" - create <Growth>: Create a Growth or get a list of Growth prices.");
            Console.WriteLine(" - items: Get a list of your items.");
            Console.WriteLine(" - skills: Get a list of your skill levels and xp.");
            Console.WriteLine(" - hp: Get your current hp.");
            Console.WriteLine(" - fight: Fight the currently chosen boss.");
            Console.WriteLine(" - boss: See the stats of the currrently chosen boss.");
            Console.WriteLine(" - stats: See your own stats.");
            Console.WriteLine(" - upgrades: List upgrades.");
            Console.WriteLine(" - upgrade <upgrade>: See the description of a single upgrade and maybe buy it, or list upgrades.");
            Console.WriteLine(" - modifiers : List modifiers.");
            Console.WriteLine(" - branch : If you're currently at a branch, pick your next path..");
        }
        
    }
}
