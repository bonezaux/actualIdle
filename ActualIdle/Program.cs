using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ActualIdle {
    public delegate RuntimeValue growthCodeInject(Forest f, Growth g, RuntimeValue[] arguments); 

    /// <summary>
    /// Contains static values.
    /// </summary>
    class Statics {
        public static string[] statList = new string[] { "Health", "Attack", "HealthRegen" };
        public static string[] skills = new string[] { "Druidcraft" };
    }

    public class Program {
        static void Main(string[] args) {
            Forest forest = new Forest();

            forest.AddObject(new Growth(forest, "Organic Material", new string[] { null }, new Formula[] { new Formula() }, null));
            forest.Values["boughtThings"] = 2;
            forest.Values["wandlevel"] = 0;

            // BUSHES
            forest.Values["BushesGain"] = 0.6;
            forest.Values["BushesAttack"] = 0.2;
            forest.Values["BushesHealth"] = 0.2;
            forest.Values["BushesInc"] = 1.1;
            forest.AddObject(new DruidObject(forest, new string[] { "Organic Material" }, new Formula[] { new FormulaLinear("!I0", "BushesGain") }, "Bushes",
                new ResourcesIncrement(new Dictionary<string, double>() { { "Organic Material", 10 } }, "BushesInc", "boughtThings"), 1));
            forest.Growths["Bushes"].Amount = 2;
            forest.Growths["Bushes"].injects["create"].Add((f, g, arguments) => { Console.WriteLine("Bob"); return null; });

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

            forest.Growths["Bushes"].Unlocked = true;
            forest.Growths["Organic Material"].Unlocked = true;

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
                } else if (l.StartsWith("count")) {
                    forest.ListGrowths();
                } else if (l.ToLower().StartsWith("growth")) {
                    if(l.Split(' ').Length ==  1)
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
                    forest.EchoBoss();
                } else if (l.StartsWith("boss")) {
                    forest.EchoBoss();
                } else if (l.StartsWith("stats")) {
                    foreach (KeyValuePair<string, double> entry in forest.GetStats()) {
                        if(entry.Value != 0)
                            Console.WriteLine(entry.Key + ": " + entry.Value);
                    }
                }
            }
        }
    }
}
