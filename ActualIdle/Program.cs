using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace ActualIdle {
    public delegate RuntimeValue codeInject(Forest f, IPerformer e, params RuntimeValue[] arguments); 

    

    public static class Program {

        public static bool debug = false;
        public static int debugCountTime = 200;


        public static void Think(this Forest forest) {
            forest.LifeCount = 0;
            foreach (Doable d in forest.Doables.Values) {
                d.Unlocked = false;
            }
            forest.Modifiers.Clear();
            forest.Values.Clear();
            forest.ChangeValue(E.SV_THINKS, 1);
            foreach(string skill in Statics.skills) {

            }
            foreach (Entity e in forest.Entities.Values) {
                forest.SoftValues[E.SV_COUNT + e.Name] = e.Amount;
                if (!forest.SoftValues.ContainsKey(E.SV_MAX_COUNT + e.Name)) {
                    //forest.Values[E.SV_MAX_COUNT + e.Name] = 0;
                }
                if (e.Amount > forest.SoftValues[E.SV_MAX_COUNT+e.Name])
                    forest.SoftValues[E.SV_MAX_COUNT + e.Name] = e.Amount;
            }

            foreach (Entity g in forest.Entities.Values) {
                g.OnReset(1);
            }
            Initializer.Init(forest);

            forest.Trigger(E.TRG_THINK_COMPLETED);
            foreach(Trophy g in forest.GetEntities(E.GRP_TROPHIES)) {
                g.Reapply();
            }
            if(!forest.Running) {
                forest.Running = true;
                forest.StartCalculation();
            }
        }
        
        static void Main(string[] args) {
            var culture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = culture;
            Forest forest = new Forest();

            Initializer.Init(forest);

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
                        string growth = l.Substring(6).Trim();
                        if (!forest.Entities.ContainsKey(growth))
                            Console.WriteLine("No Growth by name " + growth);
                        else
                        {
                            forest.Entities[growth].Echo(true);
                        }
                    }
                } else if (l.StartsWith("create")) {
                    if (l.Split(' ').Length == 1)
                        forest.ListPrices();
                    else if (forest.Entities.ContainsKey(l.Substring(7).Trim())) {
                        string thing = l.Substring(7).Trim();
                        if (!forest.Entities.ContainsKey(thing) || !forest.Entities[thing].HasExtension(E.EEXT_BUYABLE))
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
                    forest.ListGrowths(E.GRP_ITEMS);
                } else if (l.StartsWith("skills")) {
                    forest.ListSkills();
                } else if (l.StartsWith("hp")) {
                    Console.WriteLine(Math.Round(forest.Hp, 2) + " / " + Math.Round(forest.Stats[E.HEALTH], 2));
                } else if (l.StartsWith("mana")) {
                    Console.WriteLine(Math.Round(forest.Mana, 2) + " / " + Math.Round(forest.Stats[E.MAXMANA], 2));
                } else if (l.StartsWith("fight")) {
                    forest.StartFighting();
                } else if (l.StartsWith("boss")) {
                    forest.EchoBoss();
                } else if (l.StartsWith("stats")) {
                    foreach (KeyValuePair<string, double> entry in forest.GetStats()) {
                        if (entry.Value != 0)
                            Console.WriteLine(entry.Key + ": " + entry.Value);
                    }
                    if (debug && forest.Boss != null) {
                        Console.WriteLine("DEBUG VALUE (hp*(attack-b.def)): " + Statics.GetDisplayNumber(forest.Hp * (forest.Stats[E.ATTACK] - forest.Boss.Stats[E.DEFENSE])));
                        Console.WriteLine("DEBUG VALUE 2 ((b.attack-(def+hpReg))/soothing * 30) - seconds to survive: " + ((forest.Boss.Stats[E.ATTACK] - (forest.Stats[E.DEFENSE] + forest.Stats[E.HEALTHREGEN])) / forest.Stats[E.SOOTHING]) * 30 + "s");
                    }
                } else if (l.StartsWith("upgrades")) {
                    forest.ListAvailableUpgrades();
                } else if (l.StartsWith("upgrade")) { // Get data on a specific Upgrade and maybe buy it
                    if (l.Split(' ').Length == 1)
                        forest.ListAvailableUpgrades();
                    else {
                        string upgrade = l.Substring(8);
                        if(upgrade == "all") {
                            foreach(Upgrade u in forest.GetEntities(E.GRP_UPGRADES)) {
                                if(u.Unlocked && !u.Owned) {
                                    u.Create(1);
                                }
                            }
                        }
                        else if (!forest.Entities.ContainsKey(upgrade) || !forest.Entities[upgrade].Unlocked || forest.Entities[upgrade].Group != E.GRP_UPGRADES)
                            Console.WriteLine("No upgrade by name " + upgrade);
                        else {
                            forest.Entities[upgrade].Echo();
                            if (!((Upgrade)forest.Entities[upgrade]).Owned) { //Let you buy the upgrade
                                Console.WriteLine("Do you want to buy it? [Y/N]");
                                string answer = Console.ReadLine();
                                if (answer.Trim().ToLower().Equals("y")) {
                                    forest.Entities[upgrade].Create(1);
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
                }  else if (l.StartsWith("path")) {
                    forest.EchoPath();
                } else if (l.StartsWith("style")) {
                    if (forest.GetValue(E.CHANGED_STYLE) > 0) {
                        Console.WriteLine("You have already picked your style. You need time and contemplation if you want to repick.");
                    } else if (l.Length == 5) {
                        Console.WriteLine(E.STYLE_FIGHT + ": Lock in to the path of fighting for your right.");
                        if(forest.Stats[E.SOOTHING] > 0)
                            Console.WriteLine(E.STYLE_SOOTHE + ": Soothe your enemies, but don't deal them damage");
                    } else {
                        string style = l.Substring(6);
                        if (style == E.STYLE_FIGHT) {
                            Console.WriteLine("You are now locked in as a fighter.");
                            forest.FightingStyle = E.STYLE_FIGHT;
                            forest.Values[E.CHANGED_STYLE] = 1;
                        } else if (style == E.STYLE_SOOTHE && forest.Stats[E.SOOTHING] > 0) {
                            Console.WriteLine("You are now a Soother!");
                            forest.FightingStyle = E.STYLE_SOOTHE;
                            forest.Values[E.CHANGED_STYLE] = 1;
                        } else {
                            Console.WriteLine("Invalid style!");
                        }
                    }

                } else if (l.StartsWith("debug")) {
                    debug = !debug;
                    if (l.Split(' ').Length > 1) {
                        if (int.TryParse(l.Split(' ')[1], out int res)) {
                            if (res > 0)
                                debugCountTime = res;
                            debug = true;
                        }
                    }
                    if (debug)
                        Console.WriteLine("Now debugging...");
                    else
                        Console.WriteLine("Not debugging anymore.");
                } else if (l.StartsWith("time")) {
                    Console.WriteLine(forest.Count / 5 + "s, offline " + forest.OfflineTicks / 5 + "s");
                    Console.WriteLine(forest.LifeCount / 5 + "s alive");
                } else if (l.StartsWith("save")) {
                    if (l.Split(' ').Length > 1)
                        forest.Save(l.Substring(5));
                    else
                        forest.Save();
                } else if (l.StartsWith("load")) {
                    forest.Running = false;
                    forest = new Forest();
                    Initializer.firstInit = true;
                    Initializer.Init(forest);
                    if (l.Split(' ').Length > 1)
                        forest.Load(l.Substring(5));
                    else
                        forest.Load();
                } else if (l.StartsWith("think")) {
                    if (forest.Entities[E.TROPHY_SOOTHED_20TH_BOSS].Unlocked)
                        forest.Think();
                    else
                        PrintHelp();
                } else {
                    PrintHelp();
                }
            }
        }

        public static void PrintHelp() {
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
