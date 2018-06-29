using System;
using System.Collections.Generic;
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
        public static string[] statList = new string[] { "Health", "Attack", "HealthRegen", "Defense" };
        public static string[] skills = new string[] { "Druidcraft", "Fighting", "Animal Handling" };
    }

    public class Program {
        static void Main(string[] args) {
            Forest forest = new Forest();

            forest.AddObject(new Growth(forest, "Organic Material", new string[] { null }, new Formula[] { new Formula() }, null));
            forest.Growths["Organic Material"].Unlocked = true;

            forest.Values["wandlevel"] = 0;

            // BUSHES
            forest.Values["BushesGain"] = 0.6;
            forest.Values["BushesAttack"] = 0.2;
            forest.Values["BushesHealth"] = 0.2;
            forest.Values["BushesInc"] = 1.1;
            forest.AddObject(new DruidObject(forest, new string[] { "Organic Material" }, new Formula[] { new FormulaLinear("!I0", "BushesGain") }, "Bushes",
                new ResourcesIncrement(new Dictionary<string, double>() { { "Organic Material", 10 } }, "BushesInc", "boughtThings"), 1));
            forest.Growths["Bushes"].Amount = 2;
            forest.Values["boughtThings"] = 2;
            forest.Growths["Bushes"].Unlocked = true;

            // OAKS
            forest.Values["OaksGain"] = 2;
            forest.Values["OaksHealth"] = 1;
            forest.Values["OaksInc"] = 1.1;
            forest.AddObject(new DruidObject(forest, new string[] { "Organic Material" }, new Formula[] { new FormulaLinear("!I0", "OaksGain") }, "Oaks",
                new ResourcesIncrement(new Dictionary<string, double>() { { "Organic Material", 50 } }, "OaksInc", "boughtThings"), 3));
            forest.Growths["Oaks"].Unlocked = true;

            // BIRCHES
            forest.Values["BirchesGain"] = 7;
            forest.Values["BirchesAttack"] = 2;
            forest.Values["BirchesInc"] = 1.1;
            forest.AddObject(new DruidObject(forest, new string[] { "Organic Material" }, new Formula[] { new FormulaLinear("!I0", "BirchesGain") }, "Birches",
                new ResourcesIncrement(new Dictionary<string, double>() { { "Organic Material", 220 } }, "BirchesInc", "boughtThings"), 27));
            forest.Growths["Birches"].injects["loop"].Add((f, g, arguments) => {
                if (f.GetValue("DefeatedBosses") >= 1) {
                    g.Unlocked = true;
                }
                return null;
            });


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

            forest.Modifiers.Add(new Modifier("debug", new Dictionary<string, double>() { { "BushesGain", 10000 }, { "OaksGain", 10000 }, { "BirchesGain", 10000 }, { "HealthRegen", 10000 } }));
            
            forest.AddTrophy(new Trophy(forest, "Defeated Ferret",
                new codeInject[] { (f, g, arguments) => {
                    if(f.GetValue("DefeatedFerret") == 1)
                        return new RuntimeValue(3, true);
                    else
                        return new RuntimeValue(3, false);
                } }, "You defeated the Ferret! Good job!"));

            Item.itemList.Add(new Item("wand", new Modifier("wand", null, new Dictionary<string, double>() { { "XpModDruidcraft", 0.01 } }), "Wand\t+1 wand level"));
            forest.AddItem(Item.itemList[0]);

            forest.AddUpgrade(new Upgrade(forest, "Better Oaks", "This upgrade improves oak gain by 200%. Mutually exclusive with Better Bushes.", "This upgrade improves oak gain by 200%.",
                new Resources(new Dictionary<string, double>() { { "Organic Material", 2000 } }), new Modifier("Better Oaks", new Dictionary<string, double>() { { "OaksGain", 3 } }),
                "UpgradeBetter BushesBought_==_0"));

            forest.AddUpgrade(new Upgrade(forest, "Better Bushes", "this upgrade improves bushes base attack by 0.3. Mutually exclusive with Better Oaks.", "This upgrade improves oak gain by 200%.",
                new Resources(new Dictionary<string, double>() { { "Organic Material", 500 } }), new Modifier("Better Bushes", null, new Dictionary<string, double>() { { "BushesAttack", 0.3 } }),
                "UpgradeBetter OaksBought_==_0"));

            ThreadStart calculation = new ThreadStart(forest.Calculation);
            Console.WriteLine("Calculation is starting nao.");

            Thread calcThread = new Thread(calculation);
            calcThread.Start();
            



            Path startPath = new Path(forest, "Forest Street", "A little path through the forest.", "");
            startPath.AddBoss(new Fighter(10, 2, 0, "Bird", new Resources(new Dictionary<string, double>()), new Dictionary<string, int>(), ""));
            startPath.AddBoss(new Fighter(20, 5, 0, "Ferret", new Resources(new Dictionary<string, double>()), new Dictionary<string, int>(), ""));
            startPath.AddBoss(new Fighter(30, 6, 1, "Fox", new Resources(new Dictionary<string, double>()), new Dictionary<string, int>(), ""));
            Branch crossBranch = new Branch("The Forest Cross", "You're at the forest cross. You have a few ways to go now.");
            startPath.EndBranch = crossBranch;
            Path testPath = new Path(forest, "Here you go", "Yes wewy gut", "");
            testPath.AddBoss(new Fighter(1000, 2, 0, "Megacool", new Resources(new Dictionary<string, double>()), new Dictionary<string, int>(), ""));

            crossBranch.Paths.Add(testPath);


            forest.SetPath(startPath);

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
                            forest.Growths[growth].Echo();
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
                    forest.StartFighting();
                } else if (l.StartsWith("boss")) {
                    forest.EchoBoss();
                } else if (l.StartsWith("stats")) {
                    foreach (KeyValuePair<string, double> entry in forest.GetStats()) {
                        if (entry.Value != 0)
                            Console.WriteLine(entry.Key + ": " + entry.Value);
                    }
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
