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
        public static string[] statList = new string[] { "Health", "Attack", "HealthRegen", "Defense", "Soothing", "Speed", "Stall" };
        public static string[] skills = new string[] { "Druidcraft", "Animal Handling", "Soothing", "Alchemy", "Transformation", "Restoration" };
        public static double xpLogBase = 1000000; // This determines what base on the organic matter the xp will be calculated from

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

        /// <summary>
        /// Get the corresponding virtual total amount of organic material to an amount of xp
        /// </summary>
        /// <param name="xp"></param>
        /// <returns></returns>
        public static double GetTotal(double xp) {
            return Math.Pow(xpLogBase, Math.Log(xp / 100, 10)) -1;
        }

        /// <summary>
        /// Gets the gained xp from a given difference in xp given the change in xp.
        /// </summary>
        /// <param name="preXp"></param>
        /// <param name="change"></param>
        /// <returns></returns>
        public static double XpGain(double preXp, double change) {
            double preTotal = GetTotal(preXp);
            double res = Math.Log(preTotal+change+1, xpLogBase) - Math.Log(preTotal+1, xpLogBase);
            res = Math.Pow(10, res) * 100 - 100;
            if (res < 0) {
                Console.WriteLine("Reduced XP!" + res +" Pre : "+ (preTotal+1) + "Post:" + (preTotal+change+1) + ", log diff: " + res);
                return 0;
            }
            return res;
        }
    }

    public class Program {

        public static Path startPath;
        public static bool debug = false;
        public static int debugCountTime = 2;
        public static bool firstInit = true;

        /// <summary>
        /// Initializes big (booty) birches upgrades
        /// </summary>
        /// <param name="forest"></param>
        private static void UpgradesBigBirches(Forest forest) {

            // Birches give more defense
            forest.AddUpgrade(new Upgrade(forest, "Big Birches", "improves Birches defense by 700%.", null,
                new Resources(new Dictionary<string, double>() { { "Organic Material", 15000 } }), new Modifier("Big Birches", new Dictionary<string, double>() { { "BirchesDefense", 8 } })));
            forest.Upgrades["Big Birches"].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue("countBirches") >= 4 && !f.OwnsUpgrade("Spiderfriends")) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });

            // Soothing upgrades Rejuvenate
            forest.AddUpgrade(new Upgrade(forest, "Soothing Rejuvenation", "Rejuvenation scales on Soothing, adding Soothing+2%*lvlDruidcraft to the health restoration.", null,
                new Resources(new Dictionary<string, double>() { { "Organic Material", Statics.GetNumber(3, 7) } }), null));
            forest.Upgrades["Soothing Rejuvenation"].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue("countFlowers") >= 80 && f.OwnsUpgrade("Big Birches")) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });

            // Increases hp, soothing and health regen. Makes the flower generate 20 and increase by 4% per flower multiplicatively
            forest.AddUpgrade(new Upgrade(forest, "Free Healthcare", "Increases Hp by 60%, Soothing by 30% and flower Health Regen by 200%. Flowers generate 20 organic matter per second, increased multiplicatively by 4% per flower.", null,
                new Resources(new Dictionary<string, double>() { { "Organic Material", Statics.GetNumber(1, 9) } }), new Modifier("Free Healthcare",
                new Dictionary<string, double>() { { "Health", 1.6 }, { "Soothing", 1.3 }, { "FlowersHealthRegen", 3 }, { "FlowersGain", 1.04 } }, // Multiplicative modifiers
                new Dictionary<string, double>() { { "FlowersGain", 20 } }))); // Absolute modifiers
            forest.Upgrades["Free Healthcare"].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue("DefeatedBosses") >= 9 && f.GetValue("countFlowers") >= 100 && f.OwnsUpgrade("Soothing Rejuvenation")) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });
            forest.Upgrades["Free Healthcare"].Injects["ownedLoop"].Add((f, g, arguments) => {
                f.Modifiers["Free Healthcare"].ModifiersF["FlowersGain"] = Math.Pow(1.04, f.GetValue("countFlowers"));
                return null;
            });

            // Unlocks spell Harmony
            forest.AddUpgrade(new Upgrade(forest, "Unlock Harmony", "Unlocks the spell Harmony.", null,
                new Resources(new Dictionary<string, double>() { { "Organic Material", Statics.GetNumber(3, 9) } }), null));
            forest.Upgrades["Unlock Harmony"].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue("DefeatedBosses") >= 15 && f.OwnsUpgrade("Free Healthcare")) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });


            // Harmony spell, increases defense, regen, and gain, lasts 15s
            forest.AddDoable(new Doable(forest, "Harmony", null, "", "Harmony!", "Harmony is on cooldown.", true));
            forest.Doables["Harmony"].Injects["perform"].Add((f, g, arguments) => {
                f.Values["HarmonyActive"] = f.GetValue("HarmonyDuration");
                f.Values["HarmonyCooldown"] = f.GetValue("HarmonyCooldownTime") + f.GetValue("HarmonyDuration");
                f.AddModifier(new Modifier("Harmony",
                    new Dictionary<string, double>() { { "BirchesDefense", 2 }, { "HealthRegen", 2 }, { "Gain", 10 } },
                    new Dictionary<string, double>() { { "BirchesGain", 70 }, { "FlowersGain", 200 }, { "OaksGain", 40 } }));
                return null;
            });
            forest.Doables["Harmony"].Injects["loop"].Add((f, g, arguments) => {
                if (f.OwnsUpgrade("Unlock Harmony")) {
                    g.Unlocked = true;
                }
                if (f.GetValue("HarmonyActive") > 0) {
                    f.Values["HarmonyActive"] -= 1;
                    if (f.GetValue("HarmonyActive") <= 0) {
                        f.RemoveModifier("Harmony");
                    }
                }
                if (f.GetValue("HarmonyCooldown") > 0) {
                    f.Values["HarmonyCooldown"] -= 1;
                }
                return null;
            });
            forest.Doables["Harmony"].Injects["tooltip"].Add((f, g, arguments) => {
                if (forest.GetValue("HarmonyActive") > 0) {
                    return new RuntimeValue(4, "[" + /*Conversion to seconds*/(forest.GetValue("HarmonyActive") / 5) + "s A]");
                } else if (forest.GetValue("HarmonyCooldown") > 0) {
                    return new RuntimeValue(4, "[" + /*Conversion to seconds*/(forest.GetValue("HarmonyCooldown") / 5) + "s CD]");
                }
                return new RuntimeValue(4, "");
            });
        }
        private static void UpgradesSpiders(Forest forest) {
            
            forest.AddUpgrade(new Upgrade(forest, "Spiderfriends", "Unlocks Spiders.", null,
                new Resources(new Dictionary<string, double>() { { "Organic Material", 2E+4 } })));
            forest.Upgrades["Spiderfriends"].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue("DefeatedBosses") >= 3 && f.GetValue("svThinks") > 0 && !f.OwnsUpgrade("BigBirches")) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });

            forest.AddUpgrade(new Upgrade(forest, "Transmogrify Rageuvenate", "Changes Rejuvenate into a new cool spell!.", null,
                new Resources(new Dictionary<string, double>() { { "Organic Material", 5E+7 } })));
            forest.Upgrades["Transmogrify Rageuvenate"].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue("DefeatedBosses") >= 6 && f.OwnsUpgrade("Spiderfriends") && f.GetValue("countSpiders") >= 100) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });

            forest.AddUpgrade(new Upgrade(forest, "Web Site", "Choose a site to start your spiders' web. This will make every magnitude of income add 20% attack to spiders, and start the construction of a Web.", null,
                new Resources(new Dictionary<string, double>() { { "Organic Material", 2E+8 } }), new Modifier("Web Site",
                new Dictionary<string, double>() { { "SpidersAttack", 1.1 } })));
            forest.Upgrades["Web Site"].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue("DefeatedBosses") >= 9 && f.OwnsUpgrade("Transmogrify Rageuvenate") && f.GetValue("countSpiders") >= 100) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });
            forest.Upgrades["Web Site"].Injects["ownedLoop"].Add((f, g, arguments) => {
                f.Modifiers["Web Site"].ModifiersF["SpidersAttack"] = 1 + f.GetValue("Web SiteMod") * Math.Log10(f.Income);
                return null;
            });
            
            forest.AddObject(new GrowthDruid(forest, new string[] { "Organic Material" }, new Formula[] { new FormulaLinear("!I0", "SpidersGain") }, "Webs",
                new ResourcesIncrement(new Dictionary<string, double>() { { "Organic Material", 1E+8 } }, "WebsInc", "countWebs"), 100, "Animal Handling", true));
            forest.Growths["Webs"].injects["loop"].Add((f, g, arguments) => {
                if (f.OwnsUpgrade("Web Development")) {
                    g.Unlocked = true;
                }
                if (g.Unlocked) {
                    f.Modifiers["Webs"].ModifiersA["Stall"] = (int)Math.Sqrt(f.GetValue("countWebs"));
                    f.Modifiers["Webs"].ModifiersA["SpidersAttack"] = f.GetValue("countWebs")*0.1;
                }
                return null;
            });

            forest.AddUpgrade(new Upgrade(forest, "Web Development", "Start developing your web! Unlocks growing the Web", null,
                new Resources(new Dictionary<string, double>() { { "Organic Material", 2E+8 } })));
            forest.Upgrades["Web Development"].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue("DefeatedBosses") >= 12 && f.OwnsUpgrade("Web Site")) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });

            // Rageuvenate spell - deals 4 seconds of damage and gives 12 seconds of current Income TODO: if something at some point gives instant money, this should probably be changed
            forest.AddDoable(new Doable(forest, "Rageuvenate", null, "", "So angry!", "Rageuvenate is on cooldown.", true));
            forest.Doables["Rageuvenate"].Injects["perform"].Add((f, g, arguments) => {
                f.Values["RageuvenateCooldown"] = f.GetValue("RageuvenateCooldownTime");
                if(f.Fighting) {
                    f.DealDamage(f.Attack * f.GetValue("RageuvenateDamageRounds"), false);
                }
                f.Growths["Organic Material"].Amount += f.Income * f.GetValue("RageuvenateIncome");
                return null;
            });
            forest.Doables["Rageuvenate"].Injects["loop"].Add((f, g, arguments) => {
                if (f.OwnsUpgrade("Transmogrify Rageuvenate")) {
                    g.Unlocked = true;
                }
                if (f.GetValue("RageuvenateCooldown") > 0) {
                    f.Values["RageuvenateCooldown"] -= 1;
                }
                return null;
            });
            forest.Doables["Rageuvenate"].Injects["tooltip"].Add((f, g, arguments) => {
                if (forest.GetValue("RageuvenateCooldown") > 0) {
                    return new RuntimeValue(4, "[" + /*Conversion to seconds*/(forest.GetValue("RageuvenateCooldown") / 5) + "s CD]");
                }
                return new RuntimeValue(4, "");
            });
        }

        private static void InitUpgrades(Forest forest) {

            // Growth upgrades in order of total expense.
            CreateGrowthUpgrade(forest, "Bush Growth 1", "Bushes", 20, 5000, 3); //x3 - 1.8*20  / 1.1 + 5E3 
            CreateGrowthUpgrade(forest, "Oak Growth 1", "Oaks", 20, 3000, 4); //x4 - 1.6E2 / 5.7 + 3E3

            CreateGrowthUpgrade(forest, "Growing Birches 1", "Birches", 3, Statics.GetNumber(1, 4), 10); //x10 - 2.1E2 / 0.5 + 1E4
            CreateGrowthUpgrade(forest, "Larger Ants 1", "Ants", 20, 10000, 5); //x5 - 3.6E2 / 1.2 + 1E4
            CreateGrowthUpgrade(forest, "Bush Growth 2", "Bushes", 50, 20000, 5); //x15 - 4.5E2 / 2.3 + 2.5E4

            CreateGrowthUpgrade(forest, "Oak Growth 2", "Oaks", 50, 80000, 4); //x16 - 1.6E3 / 1.1 + 0.8E5
            CreateGrowthUpgrade(forest, "Gnarly Yews 1", "Yews", 15, Statics.GetNumber(3, 5), 4); //x4 -  1.4E3 / 1.2 + 3 E5
            CreateGrowthUpgrade(forest, "Larger Ants 2", "Ants", 50, Statics.GetNumber(2, 5), 5); //x25 -  4.5E3 / 2.5 + 2.1E5
            CreateGrowthUpgrade(forest, "Spiderbefriending 1", "Spiders", 40, 2E+5, 5); //x5 -  8.8E4 / 4.4 + 2 E5
            CreateGrowthUpgrade(forest, "Growing Birches 2", "Birches", 7, Statics.GetNumber(2, 5), 100); //x1000 - 4.9E4 / 4.8 + 2E5

            CreateGrowthUpgrade(forest, "Gnarly Yews 2", "Yews", 35, Statics.GetNumber(1, 6), 4); //x16 -  1.2E4 / 1.1 + 1 E6
            CreateGrowthUpgrade(forest, "Oak Growth 3", "Oaks", 80, Statics.GetNumber(2, 6), 3); //x48 - 7.68E3 / 2 + 2.1E6
            CreateGrowthUpgrade(forest, "Bush Growth 3", "Bushes", 100, Statics.GetNumber(1.5, 6), 4); //x60 - 3.6E3 / 2.7 + 1.5 E6

            CreateGrowthUpgrade(forest, "Larger Ants 3", "Ants", 80, Statics.GetNumber(6, 6), 3); //x75 - 2.16E4 / 0.6 + 0.4 E7
            CreateGrowthUpgrade(forest, "Gnarly Yews 3", "Yews", 60, Statics.GetNumber(1, 6), 4); //x64 -  8.8E4 / 1.2 + 2.1 E7
            CreateGrowthUpgrade(forest, "Spiderbefriending 2", "Spiders", 80, 5E+7, 5); //x25 -  1.7E5 / 2 + 5 E7

            CreateGrowthUpgrade(forest, "Oak Growth 4", "Oaks", 120, Statics.GetNumber(0.9, 8), 3); //x240 - 5.8E4 / 0.9 + .9E8
            CreateGrowthUpgrade(forest, "Larger Ants 4", "Ants", 120, Statics.GetNumber(2, 8), 6); //x450 - 1.3E5 / 2.0 + 2.0 E8
            CreateGrowthUpgrade(forest, "Bush Growth 4", "Bushes", 150, Statics.GetNumber(1, 9), 4); //x240 - 2.2E4 / 3.2 + 1 E8
            CreateGrowthUpgrade(forest, "Growing Birches 3", "Birches", 13, Statics.GetNumber(4, 8), 20); //x20000 - 1.8E6 / 3.5 + 4E8

            CreateGrowthUpgrade(forest, "Gnarly Yews 4", "Yews", 100, Statics.GetNumber(5, 8), 3); //x192 -  4.41E5 / 0.5 + 0.5 E9

            CreateGrowthUpgrade(forest, "Oak Growth 5", "Oaks", 170, Statics.GetNumber(1, 10), 20); //x4800 - 1.6E6 / 1 + 1E10
            CreateGrowthUpgrade(forest, "Bush Growth 5", "Bushes", 200, Statics.GetNumber(1, 10), 9); //x2160 - 2.6E5 / 3.7 + 1 E10
            CreateGrowthUpgrade(forest, "Blooming Flowers 1", "Flowers", 150, Statics.GetNumber(2, 10), 8, "FlowersGain_>_0"); //x8 - 5.3E6 / 3.2 + 2.0 E10 (R Free Healthcare)

            CreateGrowthUpgrade(forest, "Gnarly Yews 5", "Yews", 150, Statics.GetNumber(4, 10), 8); //x1536 -  5.3E6 / 0.6 + 0.4 E11

            CreateGrowthUpgrade(forest, "Growing Birches 4", "Birches", 21, Statics.GetNumber(5, 11), 4); //x80000 - 1.2E7 / 2 + 0.5E12
            forest.Upgrades["Growing Birches 4"].Modifier.ModifiersF.Add("BirchesDefense", 3);
            forest.Upgrades["Growing Birches 4"].PreDescriptionText += " Also doubles birch defense bonus.";
            forest.Upgrades["Growing Birches 4"].PostDescriptionText += " Also doubles birch defense bonus.";

            // ----------------------------------------------------------------------------- END OF GROWTH UPGRADES ----------------------------------------------------------------------------------------

            // Paths
            UpgradesBigBirches(forest);
            UpgradesSpiders(forest);

            // Other Upgrades
            // Become soother TODO: move
            forest.AddUpgrade(new Upgrade(forest, "Become Soother", "Soothing is used in fighting, but all regular damage is removed..", null,
                new Resources(new Dictionary<string, double>() { { "Organic Material", Statics.GetNumber(3, 7) } }), new Modifier("Soother", new Dictionary<string, double>() { { "Attack", 0 } })));
            forest.Upgrades["Become Soother"].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetStats()["Soothing"] > 0) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });
            // Dense forest bonus (700 forest growths)
            forest.AddUpgrade(new Upgrade(forest, "Dense Forest", "Gives 300% production bonus to everything.", null,
                new Resources(new Dictionary<string, double>() { { "Organic Material", Statics.GetNumber(1, 10) } }), new Modifier("Dense Forest", new Dictionary<string, double>() { { "Gain", 4 } })));
            forest.Upgrades["Dense Forest"].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue("countOaks") + f.GetValue("countYews") + f.GetValue("countBirches") + f.GetValue("countFlowers") + f.GetValue("countBushes") >= 700) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });



            // Thinking skill upgrades

            //Druidcraft
            forest.AddUpgrade(new Upgrade(forest, "Considered Druidcraft", "Gives 1% production bonus per Druidcraft level.", null,
                new Resources(new Dictionary<string, double>() { { "Organic Material", Statics.GetNumber(1, 1) } }), new Modifier("Considered Druidcraft", new Dictionary<string, double>() { { "Gain", 0.01 } })));
            forest.Upgrades["Considered Druidcraft"].Injects["ownedLoop"].Add((f, g, arguments) => {
                f.Modifiers["Considered Druidcraft"].ModifiersF["Gain"] = f.GetValue("lvlDruidcraft")*f.GetValue("DruidcraftConsideredBonus")+1;
                return null;
            });
            forest.Upgrades["Considered Druidcraft"].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue("svThinks") > 0) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });

            //Animal Handling
            forest.AddUpgrade(new Upgrade(forest, "Handled You Before", "Gives 1% attack bonus per Animal Handling level.", null,
                new Resources(new Dictionary<string, double>() { { "Organic Material", Statics.GetNumber(1, 1) } }), new Modifier("Handled You Before", new Dictionary<string, double>() { { "Attack", 0.01 } })));
            forest.Upgrades["Handled You Before"].Injects["ownedLoop"].Add((f, g, arguments) => {
                f.Modifiers["Handled You Before"].ModifiersF["Attack"] = f.GetValue("lvlAnimal Handling") * f.GetValue("Handled You BeforeBonus") + 1;
                return null;
            });
            forest.Upgrades["Handled You Before"].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue("svThinks") > 0) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });

            //Soothing
            forest.AddUpgrade(new Upgrade(forest, "Soothing Thoughts", "Gives 1% Health regen per Soothing level.", null,
                new Resources(new Dictionary<string, double>() { { "Organic Material", Statics.GetNumber(1, 1) } }), new Modifier("Soothing Thoughts", new Dictionary<string, double>() { { "HealthRegen", 0.01 } })));
            forest.Upgrades["Soothing Thoughts"].Injects["ownedLoop"].Add((f, g, arguments) => {
                f.Modifiers["Soothing Thoughts"].ModifiersF["HealthRegen"] = f.GetValue("lvlSoothing") * f.GetValue("Soothing ThoughtsBonus") + 1;
                return null;
            });
            forest.Upgrades["Soothing Thoughts"].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue("svThinks") > 0 && f.GetValue("lvlSoothing") > 0) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });
        }
        public static void FirstInit(Forest forest) {

            forest.AddObject(new Growth(forest, "Organic Material", new string[] { null }, new Formula[] { new Formula() }, null));

            // Bushes
            forest.AddObject(new GrowthDruid(forest, new string[] { "Organic Material" }, new Formula[] { new FormulaLinear("!I0", "BushesGain") }, "Bushes",
                new ResourcesIncrement(new Dictionary<string, double>() { { "Organic Material", 20 } }, "BushesInc", "countBushes"), 1));

            // Oaks
            forest.AddObject(new GrowthDruid(forest, new string[] { "Organic Material" }, new Formula[] { new FormulaLinear("!I0", "OaksGain") }, "Oaks",
                new ResourcesIncrement(new Dictionary<string, double>() { { "Organic Material", 100 } }, "OaksInc", "countOaks"), 3));

            // Ants
            forest.AddObject(new GrowthDruid(forest, new string[] { "Organic Material" }, new Formula[] { new FormulaLinear("!I0", "AntsGain") }, "Ants",
                new ResourcesIncrement(new Dictionary<string, double>() { { "Organic Material", 220 } }, "AntsInc", "countAnts"), 3, "Animal Handling"));
            forest.Growths["Ants"].injects["loop"].Add((f, g, arguments) => {
                if (f.GetValue("DefeatedBosses") >= 1) {
                    g.Unlocked = true;
                }
                return null;
            });

            // Birches
            forest.AddObject(new GrowthDruid(forest, new string[] { "Organic Material" }, new Formula[] { new FormulaLinear("!I0", "BirchesGain") }, "Birches",
                new ResourcesIncrement(new Dictionary<string, double>() { { "Organic Material", 440 } }, "BirchesInc", "countBirches"), 20, increaseBoughtThings: true));
            forest.Growths["Birches"].injects["loop"].Add((f, g, arguments) => {
                if (f.GetValue("DefeatedBosses") >= 1) {
                    g.Unlocked = true;
                }
                return null;
            });

            // Yews; unlocked at 10 druidcraft
            forest.AddObject(new GrowthDruid(forest, new string[] { "Organic Material" }, new Formula[] { new FormulaLinear("!I0", "YewsGain") }, "Yews",
                new ResourcesIncrement(new Dictionary<string, double>() { { "Organic Material", 4000 } }, "YewsInc", "countYews"), 40));
            forest.Growths["Yews"].injects["loop"].Add((f, g, arguments) => {
                if (f.GetValue("lvlDruidcraft") >= 15) {
                    g.Unlocked = true;
                }
                return null;
            });

            // Flowers
            forest.AddObject(new GrowthDruid(forest, new string[] { "Organic Material" }, new Formula[] { new FormulaLinear("!I0", "FlowersGain") }, "Flowers",
                new ResourcesIncrement(new Dictionary<string, double>() { { "Organic Material", 2000 } }, "FlowersInc", "countFlowers"), 9));
            forest.Growths["Flowers"].injects["loop"].Add((f, g, arguments) => {
                if (f.GetValue("DefeatedBosses") >= 6) {
                    g.Unlocked = true;
                }
                return null;
            });

            // Spiders, Unlocked by spider path
            forest.AddObject(new GrowthDruid(forest, new string[] { "Organic Material" }, new Formula[] { new FormulaLinear("!I0", "SpidersGain") }, "Spiders",
                new ResourcesIncrement(new Dictionary<string, double>() { { "Organic Material", 1000 } }, "SpidersInc", "countSpiders"), 5, "Animal Handling"));
            forest.Growths["Spiders"].injects["loop"].Add((f, g, arguments) => {
                if (f.OwnsUpgrade("Spiderfriends")) {
                    g.Unlocked = true;
                }
                forest.GetModifier("SpiderAttackMod").ModifiersF["SpidersAttack"] = 1 + 0.05 * forest.GetValue("lvlAnimal Handling");
                return null;
            });

            // Rejuvenate, heals 5+0.5*druidcraft level
            forest.AddDoable(new Doable(forest, "Rejuvenate", null, "", "Rejuvenate!", "Rejuvenate is on cooldown.", true));
            forest.Doables["Rejuvenate"].Injects["perform"].Add((f, g, arguments) => {
                double hpIncrease = forest.GetValue("DruidHeal") * (1 + forest.GetValue("lvlDruidcraft") * 0.1);
                if (f.GetValue("UpgradeSoothing RejuvenationBought") != 0) {
                    hpIncrease += f.GetStats()["Soothing"] * (1 + 0.02 * f.GetValue("lvlDruidcraft"));
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
                if (f.OwnsUpgrade("Transmogrify Rageuvenate")) {
                    g.Unlocked = false;
                }
                return null;
            });
            forest.Doables["Rejuvenate"].Injects["tooltip"].Add((f, g, arguments) => {
                if (forest.GetValue("RejuvenateCooldown") > 0) {
                    return new RuntimeValue(4, "[" + /*Conversion to seconds*/((forest.GetValue("RejuvenateCooldown") / forest.GetValue("RejuvenateCooldownMod")) / 5) + "s CD]");
                }
                return new RuntimeValue(4, "");
            });


            // DEBUG DOABLE; HALF HOUR GENERATION
            forest.AddDoable(new Doable(forest, "Halfhour Offline", null, "", "Halfhour Offline!", "", true));
            forest.Doables["Halfhour Offline"].Injects["perform"].Add((f, g, arguments) => {
                f.TickOffline(5 * 60 * 30);
                return null;
            });

            //TROPHIES
            forest.AddTrophy(new Trophy(forest, "Defeated Boss 20",
                new codeInject[] { (f, g, arguments) => {
                    if(f.GetValue("DefeatedBosses") >= 20)
                        return new RuntimeValue(3, true);
                    else
                        return new RuntimeValue(3, false);
                } }, "After encountering your twentieth adversary, You feel like you have learned a lot from the path you have taken. You consider returning to your deep woods and rethinking your strategy, having eternity at your disposal. (Think unlocked)"));

            // ITEMS TODO: Do something with them
            Item.itemList.Add(new Item("wand", new Modifier("wand", null, new Dictionary<string, double>() { { "XpModDruidcraft", 0.01 } }), "Wand\t+1 wand level"));
            forest.AddItem(Item.itemList[0]);

            InitUpgrades(forest); // Initializes upgrades in its own function

        }
        

        public static void init(Forest forest) {

            if (firstInit)
                FirstInit(forest);

            ResetValues.InitValues(forest);




            Path.paths.Clear();
            startPath = new Path(forest, "Forest Street", "A little path through the forest.", "");
            for(int loop=1; loop<=12; loop++) {
                startPath.AddBoss(generateBoss(loop));
            }
            Path hpPath = new Path(forest, "Forest heavy dudes", "Heavy people.", "");

            for (int loop=13; loop<=24; loop++) {
                hpPath.AddBoss(generateBoss(loop, 1));
            }

            Path avgPath = new Path(forest, "Forest standard dudes", "Standard people.", "");
            avgPath.ShowRequirements = "svThinks_>_0";
            
            Branch startBranch = new Branch(forest, "firstBranch", "Branch");
            startBranch.AddPath(avgPath);
            startBranch.AddPath(hpPath);
            startPath.EndBranch = startBranch;

            for (int loop = 13; loop <= 24; loop++) {
                avgPath.AddBoss(generateBoss(loop));
            }
            forest.SetPath(startPath);



            /*startPath.AddBoss(new Fighter(10, 2, 0, "Bird", new Resources(new Dictionary<string, double>()), new Dictionary<string, int>(), ""));
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
            startBranch.Paths.Add(testPath3);*/






            firstInit = false;

            // DEBUG
            /*forest.SetPath(birdPath);
            forest.Growths["Organic Material"].Amount = 50000;
            forest.Values["DefeatedBosses"] = 3;*/
            // A modifier for debugging
            //forest.Modifiers.Add(new Modifier("debug", new Dictionary<string, double>() { { "BushesGain", 10000 }, { "OaksGain", 10000 }, { "BirchesGain", 10000 },
            //    { "HealthRegen", 10000 }, { "Attack", 10000 }, { "Health", 10000 } }));
        }

        public static void think(Forest forest) {
            foreach(Growth g in forest.Growths.Values) {
                g.Unlocked = false;
                g.Amount = 0;
            }
            foreach(Upgrade u in forest.Upgrades.Values) {
                u.Owned = false;
            }
            forest.Modifiers.Clear();
            forest.Values.Clear();
            forest.ChangeValue("svThinks", 1);
            init(forest);

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
        public static void CreateGrowthUpgrade(Forest forest, string name, string target, int amount, double cost, double multiplier, string requirements = null) {

            forest.AddUpgrade(new Upgrade(forest, name, "improves "+target+" gain by "+(multiplier*100-100)+"%.", null,
                new Resources(new Dictionary<string, double>() { { "Organic Material", cost } }), new Modifier(name, new Dictionary<string, double>() { { target+"Gain", multiplier } })));
            forest.Upgrades[name].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue("count"+target) >= amount) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });
            forest.Upgrades[name].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue("count" + target) >= amount) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });
            forest.Upgrades[name].Requirements = requirements;
        }

        /// <summary>
        /// Bosstypes:
        ///  0 - Average
        ///  1 - Hp Weighted
        /// </summary>
        /// <param name="boss"></param>
        /// <param name="bossType"></param>
        /// <returns></returns>
        public static Fighter generateBoss(int boss, int bossType = 0) {
            Random r = new Random(boss);
            double powerMult = 6 * boss * Math.Pow(1.15, boss);
            double hpScale = 0.8 + r.NextDouble() * 0.4;
            double attackScale = 0.8 + r.NextDouble() * 0.4;
            double defenseScale = r.NextDouble() - 0.7;
            double defense = 0;

            if(bossType == 1) {
                attackScale /= 1.5;
                hpScale *= 3;
            }

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
                    if (debug && forest.Boss != null) {
                        Console.WriteLine("DEBUG VALUE (hp*(attack-b.def)): " + forest.Hp * (forest.Attack - forest.Boss.Defense));
                        Console.WriteLine("DEBUG VALUE 2 ((b.attack-(def+hpReg))/soothing * 30) - seconds to survive: " + ((forest.Boss.Attack - (forest.Defense + forest.GetStats()["HealthRegen"])) / forest.Soothing) * 30 + "s");
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
                } else if (l.StartsWith("debug")) {
                    debug = !debug;
                    if (l.Split(' ').Length > 1) {
                        int res = 0;
                        if (int.TryParse(l.Split(' ')[1], out res)) {
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
                } else if (l.StartsWith("save")) {
                    if (l.Split(' ').Length > 1)
                        forest.Save(l.Substring(5));
                    else
                        forest.Save();
                } else if (l.StartsWith("load")) {
                    if (l.Split(' ').Length > 1)
                        forest.Load(l.Substring(5));
                    else
                        forest.Load();
                } else if (l.StartsWith("think")) {
                    if (forest.Trophies["Defeated Boss 20"].Unlocked)
                        think(forest);
                    else
                        printHelp();
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
