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
            forest.AddUpgrade(new Upgrade(forest, E.UPG_BIG_BIRCHES, "improves Birches defense by 700%.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 15000 } }), new Modifier(E.UPG_BIG_BIRCHES, new Dictionary<string, double>() { { E.BIRCHES+E.DEFENSE, 8 } })));
            forest.Upgrades[E.UPG_BIG_BIRCHES].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue("count"+E.BIRCHES) >= 4 && !f.OwnsUpgrade(E.UPG_SPIDERFRIENDS)) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });

            // Soothing upgrades Rejuvenate
            forest.AddUpgrade(new Upgrade(forest, E.UPG_SOOTHING_REJUVENATION, "Rejuvenation scales on Soothing, adding Soothing+2%*lvlDruidcraft to the health restoration.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, Statics.GetNumber(3, 7) } }), null));
            forest.Upgrades[E.UPG_SOOTHING_REJUVENATION].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue("count"+E.FLOWERS) >= 80 && f.OwnsUpgrade(E.UPG_BIG_BIRCHES)) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });

            // Increases hp, soothing and health regen. Makes the flower generate 20 and increase by 4% per flower multiplicatively
            forest.AddUpgrade(new Upgrade(forest, E.UPG_FREE_HEALTHCARE, "Increases Hp by 60%, Soothing by 30% and flower Health Regen by 200%. Flowers generate 20 organic matter per second, increased multiplicatively by 4% per flower.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, Statics.GetNumber(1, 9) } }), new Modifier(E.UPG_FREE_HEALTHCARE,
                new Dictionary<string, double>() { { E.HEALTH, 1.6 }, { E.SOOTHING, 1.3 }, { E.FLOWERS+E.HEALTHREGEN, 3 }, { E.FLOWERS+E.GAIN, 1.04 } }, // Multiplicative modifiers
                new Dictionary<string, double>() { { E.FLOWERS+E.GAIN, 20 } }))); // Absolute modifiers
            forest.Upgrades[E.UPG_FREE_HEALTHCARE].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue(E.DEFEATED_BOSSES) >= 9 && f.GetValue("count"+E.FLOWERS) >= 100 && f.OwnsUpgrade(E.UPG_SOOTHING_REJUVENATION)) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });
            forest.Upgrades[E.UPG_FREE_HEALTHCARE].Injects["ownedLoop"].Add((f, g, arguments) => {
                f.Modifiers[E.UPG_FREE_HEALTHCARE].ModifiersF[E.FLOWERS + E.GAIN] = Math.Pow(1.04, f.GetValue("count"+E.FLOWERS));
                return null;
            });

            // Unlocks spell Harmony
            forest.AddUpgrade(new Upgrade(forest, E.UPG_UNLOCK_HARMONY, "Unlocks the spell Harmony.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, Statics.GetNumber(3, 9) } }), null));
            forest.Upgrades[E.UPG_UNLOCK_HARMONY].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue(E.DEFEATED_BOSSES) >= 15 && f.OwnsUpgrade(E.UPG_FREE_HEALTHCARE)) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });


            // Harmony spell, increases defense, regen, and gain, lasts 15s
            forest.AddDoable(new Doable(forest, E.HARMONY, null, "", "Harmony!", "Harmony is on cooldown.", true));
            forest.Doables[E.HARMONY].Injects["perform"].Add((f, g, arguments) => {
                f.Values[E.HARMONY+E.ACTIVE] = f.GetValue(E.HARMONY+E.TIME);
                f.Values[E.HARMONY+E.COOLDOWN] = f.GetValue(E.HARMONY+E.COOLDOWN+E.TIME) + f.GetValue(E.HARMONY+E.TIME);
                f.AddModifier(new Modifier(E.HARMONY,
                    new Dictionary<string, double>() { { E.BIRCHES+E.DEFENSE, 2 }, { E.HEALTHREGEN, 2 }, { E.GAIN, 10 } },
                    new Dictionary<string, double>() { { E.BIRCHES+E.GAIN, 70 }, { E.FLOWERS+E.GAIN, 200 }, { E.OAKS+E.GAIN, 40 } }));
                return null;
            });
            forest.Doables[E.HARMONY].Injects["loop"].Add((f, g, arguments) => {
                if (f.OwnsUpgrade(E.UPG_UNLOCK_HARMONY)) {
                    g.Unlocked = true;
                }
                if (f.GetValue(E.HARMONY+E.ACTIVE) > 0) {
                    f.Values[E.HARMONY + E.ACTIVE] -= 1;
                    if (f.GetValue(E.HARMONY + E.ACTIVE) <= 0) {
                        f.RemoveModifier(E.HARMONY);
                    }
                }
                if (f.GetValue(E.HARMONY + E.COOLDOWN) > 0) {
                    f.Values[E.HARMONY + E.COOLDOWN] -= 1;
                }
                return null;
            });
            forest.Doables[E.HARMONY].Injects["tooltip"].Add((f, g, arguments) => {
                if (forest.GetValue(E.HARMONY + E.ACTIVE) > 0) {
                    return new RuntimeValue(4, "[" + /*Conversion to seconds*/(forest.GetValue(E.HARMONY + E.ACTIVE) / 5) + "s A]");
                } else if (forest.GetValue(E.HARMONY + E.COOLDOWN) > 0) {
                    return new RuntimeValue(4, "[" + /*Conversion to seconds*/(forest.GetValue(E.HARMONY + E.COOLDOWN) / 5) + "s CD]");
                }
                return new RuntimeValue(4, "");
            });
        }
        private static void UpgradesSpiders(Forest forest) {
            
            forest.AddUpgrade(new Upgrade(forest, E.UPG_SPIDERFRIENDS, "Unlocks Spiders.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 2E+4 } })));
            forest.Upgrades[E.UPG_SPIDERFRIENDS].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue(E.DEFEATED_BOSSES) >= 3 && f.GetValue(E.THINKS) > 0 && !f.OwnsUpgrade(E.UPG_BIG_BIRCHES)) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });

            forest.AddUpgrade(new Upgrade(forest, E.UPG_TRANSMOGRIFY_RAGEUVENATE, "Changes Rejuvenate into a new cool spell!.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 5E+7 } })));
            forest.Upgrades[E.UPG_TRANSMOGRIFY_RAGEUVENATE].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue(E.DEFEATED_BOSSES) >= 6 && f.OwnsUpgrade(E.UPG_SPIDERFRIENDS) && f.GetValue("count"+E.SPIDERS) >= 100) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });

            forest.AddUpgrade(new Upgrade(forest, E.UPG_WEB_SITE, "Choose a site to start your spiders' web. This will make every magnitude of income add 20% attack to spiders, and start the construction of a Web.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 2E+8 } }), new Modifier(E.UPG_WEB_SITE,
                new Dictionary<string, double>() { { E.SPIDERS+E.ATTACK, 1.1 } })));
            forest.Upgrades[E.UPG_WEB_SITE].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue(E.DEFEATED_BOSSES) >= 9 && f.OwnsUpgrade(E.UPG_TRANSMOGRIFY_RAGEUVENATE) && f.GetValue("count"+E.SPIDERS) >= 100) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });
            forest.Upgrades[E.UPG_WEB_SITE].Injects["ownedLoop"].Add((f, g, arguments) => {
                f.Modifiers[E.UPG_WEB_SITE].ModifiersF[E.SPIDERS+E.ATTACK] = 1 + f.GetValue(E.UPG_WEB_SITE+E.MOD) * Math.Log10(f.Income);
                return null;
            });
            
            forest.AddObject(new GrowthDruid(forest, new string[] { E.ORGANIC_MATERIAL }, new Formula[] { new FormulaLinear("!I0", E.SPIDERS+E.GAIN) }, E.WEBS,
                new ResourcesIncrement(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 1E+8 } }, E.WEBS+E.INC, E.COUNT+E.WEBS), 100, E.ANIMAL_HANDLING, true));
            forest.Growths[E.WEBS].injects["loop"].Add((f, g, arguments) => {
                if (f.OwnsUpgrade(E.UPG_WEB_DEVELOPMENT)) {
                    g.Unlocked = true;
                }
                if (g.Unlocked) {
                    f.Modifiers[E.WEBS].ModifiersA[E.STALL] = (int)Math.Sqrt(f.GetValue(E.COUNT+E.WEBS));
                    f.Modifiers[E.WEBS].ModifiersA[E.SPIDERS + E.ATTACK] = f.GetValue(E.COUNT + E.WEBS) *0.1;
                }
                return null;
            });

            forest.AddUpgrade(new Upgrade(forest, E.UPG_WEB_DEVELOPMENT, "Start developing your web! Unlocks growing Webs, which stall your enemies. Increases Rageuvenate damage based on missing health, up to 100% at 0.01 health.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 2E+8 } })));
            forest.Upgrades[E.UPG_WEB_DEVELOPMENT].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue(E.DEFEATED_BOSSES) >= 12 && f.OwnsUpgrade(E.UPG_WEB_SITE)) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });

            // Rageuvenate spell - deals 4 seconds of damage and gives 12 seconds of current Income TODO: if something at some point gives instant money, this should probably be changed
            forest.AddDoable(new Doable(forest, E.RAGEUVENATE, null, "", "So angry!", "Rageuvenate is on cooldown.", true));
            forest.Doables[E.RAGEUVENATE].Injects["perform"].Add((f, g, arguments) => {
                f.Values[E.RAGEUVENATE+E.COOLDOWN] = f.GetValue(E.RAGEUVENATE+E.COOLDOWN+E.TIME);
                if(f.Fighting) {
                    double damage = f.GetValue(E.RAGEUVENATE + E.DAMAGE + E.TIME);
                    if(f.OwnsUpgrade(E.UPG_WEB_DEVELOPMENT)) {
                        damage *= (2 - (f.Hp / f.MaxHp));
                    }
                    Console.WriteLine("Rageuvenate dealing " + damage * f.Attack + " damage!"); 
                    f.DealDamage(f.Attack * damage, false);
                }
                f.Growths[E.ORGANIC_MATERIAL].Amount += f.Income * f.GetValue(E.RAGEUVENATE+E.INCOME+E.TIME);
                return null;
            });
            forest.Doables[E.RAGEUVENATE].Injects["loop"].Add((f, g, arguments) => {
                if (f.OwnsUpgrade(E.UPG_TRANSMOGRIFY_RAGEUVENATE)) {
                    g.Unlocked = true;
                }
                if (f.GetValue(E.RAGEUVENATE + E.COOLDOWN) > 0) {
                    f.Values[E.RAGEUVENATE + E.COOLDOWN] -= 1;
                }
                return null;
            });
            forest.Doables[E.RAGEUVENATE].Injects["tooltip"].Add((f, g, arguments) => {
                if (forest.GetValue(E.RAGEUVENATE+E.COOLDOWN) > 0) {
                    return new RuntimeValue(4, "[" + /*Conversion to seconds*/(forest.GetValue(E.RAGEUVENATE+E.COOLDOWN) / 5) + "s CD]");
                }
                return new RuntimeValue(4, "");
            });
        }

        private static void InitUpgrades(Forest forest) {

            // Growth upgrades in order of total expense.
            CreateGrowthUpgrade(forest, E.UPG_BUSH_GROWTH + " 1", E.BUSHES, 20, 5000, 3); //x3 - 1.8*20  / 1.1 + 5E3 
            CreateGrowthUpgrade(forest, E.UPG_OAK_GROWTH + " 1", E.OAKS, 20, 3000, 4); //x4 - 1.6E2 / 5.7 + 3E3

            CreateGrowthUpgrade(forest, E.UPG_BIRCH_GROWTH + " 1", E.BIRCHES, 3, Statics.GetNumber(1, 4), 10); //x10 - 2.1E2 / 0.5 + 1E4
            CreateGrowthUpgrade(forest, E.UPG_ANT_GROWTH + " 1", E.ANTS, 20, 10000, 5); //x5 - 3.6E2 / 1.2 + 1E4
            CreateGrowthUpgrade(forest, E.UPG_BUSH_GROWTH + " 2", E.BUSHES, 50, 20000, 5); //x15 - 4.5E2 / 2.3 + 2.5E4

            CreateGrowthUpgrade(forest, E.UPG_OAK_GROWTH + " 2", E.OAKS, 50, 80000, 4); //x16 - 1.6E3 / 1.1 + 0.8E5
            CreateGrowthUpgrade(forest, E.UPG_YEW_GROWTH + " 1", E.YEWS, 15, Statics.GetNumber(3, 5), 4); //x4 -  1.4E3 / 1.2 + 3 E5
            CreateGrowthUpgrade(forest, E.UPG_ANT_GROWTH + " 2", E.ANTS, 50, Statics.GetNumber(2, 5), 5); //x25 -  4.5E3 / 2.5 + 2.1E5
            CreateGrowthUpgrade(forest, E.UPG_SPIDER_GROWTH + " 1", E.SPIDERS, 40, 2E+5, 5); //x5 -  8.8E4 / 4.4 + 2 E5
            CreateGrowthUpgrade(forest, E.UPG_BIRCH_GROWTH + " 2", E.BIRCHES, 7, Statics.GetNumber(2, 5), 100); //x1000 - 4.9E4 / 4.8 + 2E5

            CreateGrowthUpgrade(forest, E.UPG_YEW_GROWTH + " 2", E.YEWS, 35, Statics.GetNumber(1, 6), 4); //x16 -  1.2E4 / 1.1 + 1 E6
            CreateGrowthUpgrade(forest, E.UPG_OAK_GROWTH + " 3", E.OAKS, 80, Statics.GetNumber(2, 6), 3); //x48 - 7.68E3 / 2 + 2.1E6
            CreateGrowthUpgrade(forest, E.UPG_BUSH_GROWTH + " 3", E.BUSHES, 100, Statics.GetNumber(1.5, 6), 4); //x60 - 3.6E3 / 2.7 + 1.5 E6

            CreateGrowthUpgrade(forest, E.UPG_ANT_GROWTH + " 3", E.ANTS, 80, Statics.GetNumber(6, 6), 3); //x75 - 2.16E4 / 0.6 + 0.4 E7
            CreateGrowthUpgrade(forest, E.UPG_YEW_GROWTH + " 3", E.YEWS, 60, Statics.GetNumber(1, 6), 4); //x64 -  8.8E4 / 1.2 + 2.1 E7
            CreateGrowthUpgrade(forest, E.UPG_SPIDER_GROWTH + " 2", E.SPIDERS, 80, 5E+7, 5); //x25 -  1.7E5 / 2 + 5 E7

            CreateGrowthUpgrade(forest, E.UPG_OAK_GROWTH + " 4", E.OAKS, 120, Statics.GetNumber(0.9, 8), 3); //x240 - 5.8E4 / 0.9 + .9E8
            CreateGrowthUpgrade(forest, E.UPG_ANT_GROWTH + " 4", E.ANTS, 120, Statics.GetNumber(2, 8), 6); //x450 - 1.3E5 / 2.0 + 2.0 E8
            CreateGrowthUpgrade(forest, E.UPG_BUSH_GROWTH + " 4", E.BUSHES, 150, Statics.GetNumber(1, 9), 4); //x240 - 2.2E4 / 3.2 + 1 E8
            CreateGrowthUpgrade(forest, E.UPG_BIRCH_GROWTH + " 3", E.BIRCHES, 13, Statics.GetNumber(4, 8), 20); //x20000 - 1.8E6 / 3.5 + 4E8

            CreateGrowthUpgrade(forest, E.UPG_YEW_GROWTH + " 4", E.YEWS, 100, Statics.GetNumber(5, 8), 3); //x192 -  4.41E5 / 0.5 + 0.5 E9

            CreateGrowthUpgrade(forest, E.UPG_OAK_GROWTH + " 5", E.OAKS, 170, Statics.GetNumber(1, 10), 20); //x4800 - 1.6E6 / 1 + 1E10
            CreateGrowthUpgrade(forest, E.UPG_BUSH_GROWTH + " 5", E.BUSHES, 200, Statics.GetNumber(1, 10), 9); //x2160 - 2.6E5 / 3.7 + 1 E10
            CreateGrowthUpgrade(forest, E.UPG_FLOWER_GROWTH + " 1", E.FLOWERS, 150, Statics.GetNumber(2, 10), 8, E.FLOWERS+E.GAIN+"_>_0"); //x8 - 5.3E6 / 3.2 + 2.0 E10 (R Free Healthcare)

            CreateGrowthUpgrade(forest, E.UPG_YEW_GROWTH + " 5", E.YEWS, 150, Statics.GetNumber(4, 10), 8); //x1536 -  5.3E6 / 0.6 + 0.4 E11

            CreateGrowthUpgrade(forest, E.UPG_BIRCH_GROWTH + " 4", E.BIRCHES, 21, Statics.GetNumber(5, 11), 4); //x80000 - 1.2E7 / 2 + 0.5E12
            forest.Upgrades[E.UPG_BIRCH_GROWTH + " 4"].Modifier.ModifiersF.Add(E.BIRCHES+E.DEFENSE, 3);
            forest.Upgrades[E.UPG_BIRCH_GROWTH + " 4"].PreDescriptionText += " Also doubles birch defense bonus.";
            forest.Upgrades[E.UPG_BIRCH_GROWTH + " 4"].PostDescriptionText += " Also doubles birch defense bonus.";

            // ----------------------------------------------------------------------------- END OF GROWTH UPGRADES ----------------------------------------------------------------------------------------

            // Paths
            UpgradesBigBirches(forest);
            UpgradesSpiders(forest);

            // Other Upgrades
            // Become soother TODO: move
            forest.AddUpgrade(new Upgrade(forest, E.UPG_BECOME_SOOTHER, "Soothing is used in fighting, but all regular damage is removed..", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, Statics.GetNumber(3, 7) } }), new Modifier(E.SOOTHER, new Dictionary<string, double>() { { E.ATTACK, 0 } })));
            forest.Upgrades[E.UPG_BECOME_SOOTHER].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetStats()[E.SOOTHING] > 0) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });
            // Dense forest bonus (700 forest growths)
            forest.AddUpgrade(new Upgrade(forest, E.UPG_DENSE_FOREST, "Gives 300% production bonus to everything.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, Statics.GetNumber(1, 10) } }), new Modifier(E.UPG_DENSE_FOREST, new Dictionary<string, double>() { { E.GAIN, 4 } })));
            forest.Upgrades[E.UPG_DENSE_FOREST].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue(E.COUNT+E.OAKS) + f.GetValue(E.COUNT+E.YEWS) + f.GetValue(E.COUNT+E.BIRCHES) + f.GetValue(E.COUNT+E.FLOWERS) + f.GetValue(E.COUNT+E.BUSHES) >= 700) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });



            // Thinking skill upgrades

            //Druidcraft
            forest.AddUpgrade(new Upgrade(forest, E.UPG_DRUIDCRAFT_CONSIDERED, "Gives 1% production bonus per Druidcraft level.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, Statics.GetNumber(1, 1) } }), new Modifier(E.UPG_DRUIDCRAFT_CONSIDERED, new Dictionary<string, double>() { { E.GAIN, 0.01 } })));
            forest.Upgrades[E.UPG_DRUIDCRAFT_CONSIDERED].Injects["ownedLoop"].Add((f, g, arguments) => {
                f.Modifiers[E.UPG_DRUIDCRAFT_CONSIDERED].ModifiersF[E.GAIN] = f.GetValue("lvl"+E.DRUIDCRAFT)*f.GetValue(E.UPG_DRUIDCRAFT_CONSIDERED+E.MOD) +1;
                return null;
            });
            forest.Upgrades[E.UPG_DRUIDCRAFT_CONSIDERED].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue(E.THINKS) > 0) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });

            //Animal Handling
            forest.AddUpgrade(new Upgrade(forest, E.UPG_HANDLED_YOU_BEFORE, "Gives 1% attack bonus per Animal Handling level.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, Statics.GetNumber(1, 1) } }), new Modifier("Handled You Before", new Dictionary<string, double>() { { "Attack", 0.01 } })));
            forest.Upgrades[E.UPG_HANDLED_YOU_BEFORE].Injects["ownedLoop"].Add((f, g, arguments) => {
                f.Modifiers[E.UPG_HANDLED_YOU_BEFORE].ModifiersF[E.ATTACK] = f.GetValue("lvl"+E.ANIMAL_HANDLING) * f.GetValue(E.UPG_HANDLED_YOU_BEFORE+E.MOD) + 1;
                return null;
            });
            forest.Upgrades[E.UPG_HANDLED_YOU_BEFORE].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue(E.THINKS) > 0) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });

            //Soothing
            forest.AddUpgrade(new Upgrade(forest, E.UPG_SOOTHING_THOUGHTS, "Gives 1% Health regen per Soothing level.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, Statics.GetNumber(1, 1) } }), new Modifier(E.UPG_SOOTHING_THOUGHTS, new Dictionary<string, double>() { { E.HEALTHREGEN, 0.01 } })));
            forest.Upgrades[E.UPG_SOOTHING_THOUGHTS].Injects["ownedLoop"].Add((f, g, arguments) => {
                f.Modifiers[E.UPG_SOOTHING_THOUGHTS].ModifiersF[E.HEALTHREGEN] = f.GetValue("lvl"+E.SOOTHING) * f.GetValue(E.UPG_SOOTHING_THOUGHTS+E.MOD) + 1;
                return null;
            });
            forest.Upgrades[E.UPG_SOOTHING_THOUGHTS].Injects["unlocked"].Add((f, g, arguments) => {
                if (f.GetValue(E.THINKS) > 0 && f.GetValue("lvl"+E.SOOTHING) > 0) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });
        }
        public static void FirstInit(Forest forest) {

            forest.AddObject(new Growth(forest, E.ORGANIC_MATERIAL, new string[] { null }, new Formula[] { new Formula() }, null));

            // Bushes
            forest.AddObject(new GrowthDruid(forest, new string[] { E.ORGANIC_MATERIAL }, new Formula[] { new FormulaLinear("!I0", "BushesGain") }, "Bushes",
                new ResourcesIncrement(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 20 } }, "BushesInc", "countBushes"), 1));

            // Oaks
            forest.AddObject(new GrowthDruid(forest, new string[] { E.ORGANIC_MATERIAL }, new Formula[] { new FormulaLinear("!I0", "OaksGain") }, "Oaks",
                new ResourcesIncrement(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 100 } }, "OaksInc", "countOaks"), 3));

            // Ants
            forest.AddObject(new GrowthDruid(forest, new string[] { E.ORGANIC_MATERIAL }, new Formula[] { new FormulaLinear("!I0", "AntsGain") }, "Ants",
                new ResourcesIncrement(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 220 } }, "AntsInc", "countAnts"), 3, "Animal Handling"));
            forest.Growths["Ants"].injects["loop"].Add((f, g, arguments) => {
                if (f.GetValue("DefeatedBosses") >= 1) {
                    g.Unlocked = true;
                }
                return null;
            });

            // Birches
            forest.AddObject(new GrowthDruid(forest, new string[] { E.ORGANIC_MATERIAL }, new Formula[] { new FormulaLinear("!I0", "BirchesGain") }, "Birches",
                new ResourcesIncrement(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 440 } }, "BirchesInc", "countBirches"), 20, increaseBoughtThings: true));
            forest.Growths["Birches"].injects["loop"].Add((f, g, arguments) => {
                if (f.GetValue("DefeatedBosses") >= 1) {
                    g.Unlocked = true;
                }
                return null;
            });

            // Yews; unlocked at 10 druidcraft
            forest.AddObject(new GrowthDruid(forest, new string[] { E.ORGANIC_MATERIAL }, new Formula[] { new FormulaLinear("!I0", "YewsGain") }, "Yews",
                new ResourcesIncrement(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 4000 } }, "YewsInc", "countYews"), 40));
            forest.Growths["Yews"].injects["loop"].Add((f, g, arguments) => {
                if (f.GetValue("lvlDruidcraft") >= 15) {
                    g.Unlocked = true;
                }
                return null;
            });

            // Flowers
            forest.AddObject(new GrowthDruid(forest, new string[] { E.ORGANIC_MATERIAL }, new Formula[] { new FormulaLinear("!I0", "FlowersGain") }, "Flowers",
                new ResourcesIncrement(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 2000 } }, "FlowersInc", "countFlowers"), 9));
            forest.Growths["Flowers"].injects["loop"].Add((f, g, arguments) => {
                if (f.GetValue("DefeatedBosses") >= 6) {
                    g.Unlocked = true;
                }
                return null;
            });

            // Spiders, Unlocked by spider path
            forest.AddObject(new GrowthDruid(forest, new string[] { E.ORGANIC_MATERIAL }, new Formula[] { new FormulaLinear("!I0", "SpidersGain") }, "Spiders",
                new ResourcesIncrement(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 1000 } }, "SpidersInc", "countSpiders"), 5, "Animal Handling"));
            forest.Growths["Spiders"].injects["loop"].Add((f, g, arguments) => {
                if (f.OwnsUpgrade("Spiderfriends")) {
                    g.Unlocked = true;
                }
                forest.GetModifier(E.SPIDERS+E.ATTACK+E.MOD).ModifiersF["SpidersAttack"] = 1 + 0.05 * forest.GetValue("lvlAnimal Handling");
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
            avgPath.ShowRequirements = E.THINKS+"_>_0";
            
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
                hpScale *= 4.5;
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
