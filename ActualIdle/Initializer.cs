﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ActualIdle.Entity_Extensions;

namespace ActualIdle {
    public class Initializer {
        public static bool firstInit = true;
        /// <summary>
        /// Injects for use in stuff that are premade
        /// Contents:
        ///  E.INJ_TOOLTIP + E.COOLDOWN: Displays a cooldown, if any
        ///  E.INJ_TOOLTIP + E.ACTIVE: Displays active and cooldown, if any
        /// </summary>
        public static Dictionary<string, CodeInject> premadeInjects;
        public static Dictionary<string, (double chance, string item)[]> lootTables = new Dictionary<string, (double, string)[]>();
        public static Path startPath;
        public static Random r = new Random();

        /// <summary>
        /// Initializes big (booty) birches upgrades
        /// </summary>
        /// <param name="forest"></param>
        private static void UpgradesBigBirches(Forest forest) {

            // Birches give more defense
            forest.AddEntity(new Upgrade(forest, E.UPG_BIG_BIRCHES, "improves Birches defense by 700%.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 15000 } }), new Modifier(E.UPG_BIG_BIRCHES, new Dictionary<string, double>() { { E.ENTITY_BIRCHES + E.DEFENSE, 8 } }),
                ()=> forest.GetValue("count" + E.ENTITY_BIRCHES) >= 4 && !forest.OwnsEntity(E.UPG_SPIDERFRIENDS), E.TRG_ENTITY_ADDED));
            forest.Entities[E.UPG_BIG_BIRCHES].AddTrigger(E.INJ_CREATE, (f, g, args) => {
                forest.Entities[E.UPG_SPIDERFRIENDS].Unlocked = false;
                return null;
            });

            // Soothing upgrades Rejuvenate
            forest.AddEntity(new Upgrade(forest, E.UPG_SOOTHING_REJUVENATION, "Rejuvenation scales on Soothing, adding Soothing+2%*DC level to the health restoration.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, Statics.GetNumber(3, 7) } }), null,
                ()=> forest.GetValue("count" + E.ENTITY_FLOWERS) >= 80 && forest.OwnsEntity(E.UPG_BIG_BIRCHES), E.TRG_ENTITY_ADDED));

            // Increases hp, soothing and health regen. Makes the flower generate 20 and increase by 4% per flower multiplicatively
            forest.AddEntity(new Upgrade(forest, E.UPG_FREE_HEALTHCARE, "Increases Hp by 60%, Soothing by 30% and flower Health Regen by 200%. Flowers generate 20 organic matter per second, increased multiplicatively by 4% per flower.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, Statics.GetNumber(1, 9) } }), new Modifier(E.UPG_FREE_HEALTHCARE,
                new Dictionary<string, double>() { { E.HEALTH, 1.6 }, { E.SOOTHING, 1.3 }, { E.ENTITY_FLOWERS + E.HEALTHREGEN, 3 }, { E.ENTITY_FLOWERS + E.GAIN, 1.04 } }, // Multiplicative modifiers
                new Dictionary<string, double>() { { E.ENTITY_FLOWERS + E.GAIN, 20 } }), // Absolute modifiers
                () => forest.GetValue(E.DEFEATED_BOSSES) >= 9 && forest.GetValue("count" + E.ENTITY_FLOWERS) >= 100 && forest.OwnsEntity(E.UPG_SOOTHING_REJUVENATION),
                E.TRG_DEFEATED_BOSS, E.TRG_ENTITY_ADDED));
            forest.Entities[E.UPG_FREE_HEALTHCARE].Injects["ownedLoop"].Add((f, g, arguments) => {
                f.Modifiers[E.UPG_FREE_HEALTHCARE].ModifiersF[E.ENTITY_FLOWERS + E.GAIN] = Math.Pow(1.04, f.GetValue("count" + E.ENTITY_FLOWERS));
                return null;
            });

            // Unlocks spell Harmony
            forest.AddEntity(new Upgrade(forest, E.UPG_UNLOCK_HARMONY, "Unlocks the spell Harmony.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, Statics.GetNumber(3, 9) } }), null,
                ()=> forest.GetValue(E.DEFEATED_BOSSES) >= 15 && forest.OwnsEntity(E.UPG_FREE_HEALTHCARE), E.TRG_DEFEATED_BOSS, E.TRG_ENTITY_ADDED));

            // Harmony spell, increases defense, regen, and gain, lasts 15s
            forest.AddDoable(new Doable(forest, E.ABIL_HARMONY, null, "", "Harmony!", "Harmony is on cooldown.", true, E.ABIL_HARMONY + E.MANA));
            forest.Doables[E.ABIL_HARMONY].Injects["perform"].Add((f, g, arguments) => {
                f.Values[E.ABIL_HARMONY + E.ACTIVE] = f.GetValue(E.ABIL_HARMONY + E.TIME);
                f.Values[E.ABIL_HARMONY + E.COOLDOWN] = f.GetValue(E.ABIL_HARMONY + E.COOLDOWN + E.TIME) + f.GetValue(E.ABIL_HARMONY + E.TIME);
                f.AddModifier(new Modifier(E.ABIL_HARMONY,
                    new Dictionary<string, double>() { { E.ENTITY_BIRCHES + E.DEFENSE, 2 }, { E.HEALTHREGEN, 2 }, { E.GAIN, 10 } },
                    new Dictionary<string, double>() { { E.ENTITY_BIRCHES + E.GAIN, 70 }, { E.ENTITY_FLOWERS + E.GAIN, 200 }, { E.ENTITY_OAKS + E.GAIN, 40 } }));
                return null;
            });
            forest.Doables[E.ABIL_HARMONY].Injects["loop"].Add((f, g, arguments) => {
                if (f.OwnsEntity(E.UPG_UNLOCK_HARMONY)) {
                    g.Unlocked = true;
                }
                if (f.GetValue(E.ABIL_HARMONY + E.ACTIVE) > 0) {
                    f.Values[E.ABIL_HARMONY + E.ACTIVE] -= 1;
                    if (f.GetValue(E.ABIL_HARMONY + E.ACTIVE) <= 0) {
                        f.RemoveModifier(E.ABIL_HARMONY);
                    }
                }
                if (f.GetValue(E.ABIL_HARMONY + E.COOLDOWN) > 0) {
                    f.Values[E.ABIL_HARMONY + E.COOLDOWN] -= 1;
                }
                return null;
            });
            forest.Doables[E.ABIL_HARMONY].Injects["tooltip"].Add((f, g, arguments) => {
                if (forest.GetValue(E.ABIL_HARMONY + E.ACTIVE) > 0) {
                    return new RuntimeValue(4, "[" + /*Conversion to seconds*/(forest.GetValue(E.ABIL_HARMONY + E.ACTIVE) / 5) + "s A]");
                } else if (forest.GetValue(E.ABIL_HARMONY + E.COOLDOWN) > 0) {
                    return new RuntimeValue(4, "[" + /*Conversion to seconds*/(forest.GetValue(E.ABIL_HARMONY + E.COOLDOWN) / 5) + "s CD]");
                }
                return new RuntimeValue(4, "");
            });
        }
        private static void UpgradesSpiders(Forest forest) {

            forest.AddEntity(new Upgrade(forest, E.UPG_SPIDERFRIENDS, "Unlocks Spiders.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 2E+4 } }), null,
                ()=> (forest.GetValue(E.DEFEATED_BOSSES) >= 3 && forest.GetValue(E.SV_THINKS) > 0 && !forest.OwnsEntity(E.UPG_BIG_BIRCHES)),
                E.TRG_DEFEATED_BOSS));
            forest.Entities[E.UPG_SPIDERFRIENDS].Injects[E.INJ_CREATE].Add((f, g, arguments) => {
                f.Entities[E.ENTITY_SPIDERS].Unlocked = true;
                f.Entities[E.UPG_BIG_BIRCHES].Unlocked = false;
                return null;
            });

            forest.AddEntity(new Upgrade(forest, E.UPG_TRANSMOGRIFY_RAGEUVENATE, "Changes Rejuvenate into a new cool spell!.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 5E+7 } }), null, 
                ()=> forest.GetValue(E.DEFEATED_BOSSES) >= 6 && forest.OwnsEntity(E.UPG_SPIDERFRIENDS) && forest.GetValue("count" + E.ENTITY_SPIDERS) >= 100,
                E.TRG_DEFEATED_BOSS, E.TRG_ENTITY_ADDED));

            forest.AddEntity(new Upgrade(forest, E.UPG_WEB_SITE, "Choose a site to start your spiders' web. This will make every magnitude of income add 20% attack to spiders, and start the construction of a Web. Yews are also ideal for spiders, and will protect coverage. +1% to yew health per spider owned.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 2E+8 } }), new Modifier(E.UPG_WEB_SITE,
                new Dictionary<string, double>() { { E.ENTITY_SPIDERS + E.ATTACK, 1.1 }, { E.ENTITY_YEWS + E.HEALTH, 1.01 } }),
                () => forest.GetValue(E.DEFEATED_BOSSES) >= 9 && forest.OwnsEntity(E.UPG_TRANSMOGRIFY_RAGEUVENATE),
                E.TRG_DEFEATED_BOSS, E.TRG_ENTITY_ADDED
                ));
            forest.Entities[E.UPG_WEB_SITE].Injects["ownedLoop"].Add((f, g, arguments) => {
                f.Modifiers[E.UPG_WEB_SITE].ModifiersF[E.ENTITY_SPIDERS + E.ATTACK] = 1 + f.GetValue(E.UPG_WEB_SITE + E.MOD) * Math.Log10(f.Income);
                f.Modifiers[E.UPG_WEB_SITE].ModifiersF[E.ENTITY_YEWS + E.HEALTH] = 1 + f.GetValue(E.COUNT + E.ENTITY_SPIDERS) * 0.01;
                return null;
            });

            forest.AddEntity(new Entity(forest, E.ENTITY_WEBS)
            .Add(new EExtGenerate(new(string, Formula)[] { (E.ORGANIC_MATERIAL, new FormulaLinear("!I0", E.ENTITY_WEBS + E.GAIN)) }))
            .Add(new EExtBuyable(new ResourcesIncrement(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 1E+8 } }, E.ENTITY_WEBS + E.INC, E.COUNT + E.ENTITY_WEBS)))
            .Add(new EExtXpMod(100, E.ANIMAL_HANDLING))
            .Add(new EExtLimited()));
            forest.Entities[E.ENTITY_WEBS].Injects["loop"].Add((f, g, arguments) => {
                if (f.OwnsEntity(E.UPG_WEB_DEVELOPMENT)) {
                    g.Unlocked = true;
                }
                if (g.Unlocked) {
                    f.Modifiers[E.ENTITY_WEBS].ModifiersA[E.STALL] = (int)Math.Sqrt(f.GetValue(E.COUNT + E.ENTITY_WEBS));
                    f.Modifiers[E.ENTITY_WEBS].ModifiersA[E.ENTITY_SPIDERS + E.ATTACK] = f.GetValue(E.COUNT + E.ENTITY_WEBS) * 0.1;
                }
                return null;
            });

            // Web development upgrade
            forest.AddEntity(new Upgrade(forest, E.UPG_WEB_DEVELOPMENT, "Start developing your web! Unlocks growing Webs, which stall your enemies. Increases Rageuvenate damage based on missing health, up to 100% at 0.01 hp.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 2E+8 } }), null,
                ()=>forest.GetValue(E.DEFEATED_BOSSES) >= 12 && forest.OwnsEntity(E.UPG_WEB_SITE),
                E.TRG_DEFEATED_BOSS, E.TRG_ENTITY_ADDED));

            // Unlocks the Surf the Web ability
            forest.AddEntity(new Upgrade(forest, E.UPG_UNLOCK_SURF_THE_WEB, "Unlocks the ability Surf the Web.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 1E+9 } }), null,
                ()=> forest.GetValue(E.DEFEATED_BOSSES) >= 15 && forest.OwnsEntity(E.UPG_WEB_DEVELOPMENT),
                E.TRG_DEFEATED_BOSS, E.TRG_ENTITY_ADDED));
            forest.Entities[E.UPG_UNLOCK_SURF_THE_WEB].Injects[E.INJ_CREATE].Add((f, g, arguments) => {
                f.Doables[E.ABIL_SURF_THE_WEB].Unlocked = true;
                return null;
            });

            // Rageuvenate spell - deals 4 seconds of damage and gives 12 seconds of current Income TODO: if something at some point gives instant money, this should probably be changed
            forest.AddDoable(new Doable(forest, E.ABIL_RAGEUVENATE, null, "", "So angry!", "Rageuvenate is on cooldown.", true, E.ABIL_RAGEUVENATE + E.MANA));
            forest.Doables[E.ABIL_RAGEUVENATE].Injects["perform"].Add((f, g, arguments) => {
                f.Values[E.ABIL_RAGEUVENATE + E.COOLDOWN] = f.GetValue(E.ABIL_RAGEUVENATE + E.COOLDOWN + E.TIME);
                if (f.Fighting) {
                    double damage = f.GetValue(E.ABIL_RAGEUVENATE + E.DAMAGE + E.TIME);
                    if (f.OwnsEntity(E.UPG_WEB_DEVELOPMENT)) {
                        damage *= (2 - (f.Hp / f.Stats[E.HEALTH]));
                    }
                    Console.WriteLine("Rageuvenate dealing " + damage * f.Stats[E.ATTACK] + " damage!");
                    f.DealDamage(f.Stats[E.ATTACK] * damage, false);
                }
                f.Entities[E.ORGANIC_MATERIAL].Amount += f.Income * f.GetValue(E.ABIL_RAGEUVENATE + E.INCOME + E.TIME);
                return null;
            });
            forest.Doables[E.ABIL_RAGEUVENATE].Injects["loop"].Add((f, g, arguments) => {
                if (f.OwnsEntity(E.UPG_TRANSMOGRIFY_RAGEUVENATE)) {
                    g.Unlocked = true;
                }
                if (f.GetValue(E.ABIL_RAGEUVENATE + E.COOLDOWN) > 0) {
                    f.Values[E.ABIL_RAGEUVENATE + E.COOLDOWN] -= 1;
                }
                return null;
            });
            forest.Doables[E.ABIL_RAGEUVENATE].Injects["tooltip"].Add((f, g, arguments) => {
                if (forest.GetValue(E.ABIL_RAGEUVENATE + E.COOLDOWN) > 0) {
                    return new RuntimeValue(4, "[" + /*Conversion to seconds*/(forest.GetValue(E.ABIL_RAGEUVENATE + E.COOLDOWN) / 5) + "s CD]");
                }
                return new RuntimeValue(4, "");
            });


            // Surf the Web spell
            forest.AddDoable(new Doable(forest, E.ABIL_SURF_THE_WEB, null, "", "", "", true, E.ABIL_SURF_THE_WEB + E.MANA));
            forest.Doables[E.ABIL_SURF_THE_WEB].Injects["perform"].Add((f, g, arguments) => {
                f.Values[E.ABIL_SURF_THE_WEB + E.ACTIVE] = f.GetValue(E.ABIL_SURF_THE_WEB + E.TIME);
                f.Values[E.ABIL_SURF_THE_WEB + E.COOLDOWN] = f.GetValue(E.ABIL_SURF_THE_WEB + E.TIME);

                double trees = f.GetValue(E.COUNT + E.ENTITY_BIRCHES) + f.GetValue(E.COUNT + E.ENTITY_OAKS) + f.GetValue(E.COUNT + E.ENTITY_YEWS);
                double treeMod = trees * f.GetValue(E.ABIL_SURF_THE_WEB + E.TREES + E.MOD) + 1;
                double animals = f.GetValue(E.COUNT + E.ENTITY_ANTS) + f.GetValue(E.COUNT + E.ENTITY_SPIDERS);
                double gainMod = animals * f.GetValue(E.ABIL_SURF_THE_WEB + E.ANIMALS + E.MOD) + 1;
                treeMod *= f.GetValue(E.COUNT + E.ENTITY_WEBS);
                gainMod *= f.GetValue(E.COUNT + E.ENTITY_WEBS);

                f.AddModifier(new Modifier(E.ABIL_SURF_THE_WEB,
                    new Dictionary<string, double>() { { E.ENTITY_SPIDERS + E.GAIN, treeMod }, { E.ENTITY_ANTS + E.GAIN, treeMod }, { E.GAIN, gainMod } }));
                return null;
            });
            forest.Doables[E.ABIL_SURF_THE_WEB].Injects["loop"].Add((f, g, arguments) => {
                if (f.GetValue(E.ABIL_SURF_THE_WEB + E.ACTIVE) > 0) {
                    f.Values[E.ABIL_SURF_THE_WEB + E.ACTIVE] -= 1;
                    if (f.GetValue(E.ABIL_SURF_THE_WEB + E.ACTIVE) <= 0) {
                        f.RemoveModifier(E.ABIL_SURF_THE_WEB);
                    }
                }
                if (f.GetValue(E.ABIL_SURF_THE_WEB + E.COOLDOWN) > 0) {
                    f.Values[E.ABIL_SURF_THE_WEB + E.COOLDOWN] -= 1;
                }
                return null;
            });
            forest.Doables[E.ABIL_SURF_THE_WEB].Injects["tooltip"].Add(premadeInjects[E.INJ_TOOLTIP + E.ACTIVE]);

            // Spider prestige upgrade
            forest.AddEntity(new Upgrade(forest, E.UPG_SPIDER_PRESTIGE, "Makes every boss killed as the Spider faction after the 20th add 2% attack.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 1E+11 } }),
                new Modifier(E.UPG_SPIDER_PRESTIGE, new Dictionary<string, double>() { { E.ATTACK, 1.00 } }),
                ()=> forest.GetValue(E.DEFEATED_BOSSES) >= 20 && forest.OwnsEntity(E.UPG_SPIDERFRIENDS),
                E.TRG_DEFEATED_BOSS, E.TRG_ENTITY_ADDED));
            forest.Entities[E.UPG_SPIDER_PRESTIGE].Strength = 1;
            forest.Entities[E.UPG_SPIDER_PRESTIGE].AddTrigger(E.TRG_DEFEATED_BOSS, (f, g, arguments) => {
                if (f.GetValue(E.DEFEATED_BOSSES) > 20+f.GetValue(E.SV+E.UPG_SPIDER_PRESTIGE+E.KILLS) && g.Unlocked) {
                    f.ChangeValue(E.SV+E.UPG_SPIDER_PRESTIGE + E.KILLS, 1);
                    forest.GetModifier(E.UPG_SPIDER_PRESTIGE).ModifiersF[E.ATTACK] = f.GetValue(E.SV + E.UPG_SPIDER_PRESTIGE + E.KILLS)*0.02+1;
                }
                return null;
            });
        }
        private static void InitGrowthUpgrades(Forest forest) {
            // Growth upgrades in order of total expense.
            CreateGrowthUpgrade(forest, E.UPG_BUSH_GROWTH, E.ENTITY_BUSHES, 20, 5000, 3); //x3 - 1.8*20  / 1.1 + 5E3 
            CreateGrowthUpgrade(forest, E.UPG_OAK_GROWTH, E.ENTITY_OAKS, 20, 3000, 4); //x4 - 1.6E2 / 5.7 + 3E3

            CreateGrowthUpgrade(forest, E.UPG_BIRCH_GROWTH, E.ENTITY_BIRCHES, 3, Statics.GetNumber(1, 4), 10); //x10 - 2.1E2 / 0.5 + 1E4
            CreateGrowthUpgrade(forest, E.UPG_ANT_GROWTH, E.ENTITY_ANTS, 20, 10000, 5); //x5 - 3.6E2 / 1.2 + 1E4
            CreateGrowthUpgrade(forest, E.UPG_BUSH_GROWTH, E.ENTITY_BUSHES, 50, 20000, 5); //x15 - 4.5E2 / 2.3 + 2.5E4

            CreateGrowthUpgrade(forest, E.UPG_OAK_GROWTH, E.ENTITY_OAKS, 50, 80000, 4); //x16 - 1.6E3 / 1.1 + 0.8E5
            CreateGrowthUpgrade(forest, E.UPG_YEW_GROWTH, E.ENTITY_YEWS, 15, Statics.GetNumber(3, 5), 4); //x4 -  1.4E3 / 1.2 + 3 E5
            CreateGrowthUpgrade(forest, E.UPG_ANT_GROWTH, E.ENTITY_ANTS, 50, Statics.GetNumber(2, 5), 5); //x25 -  4.5E3 / 2.5 + 2.1E5
            CreateGrowthUpgrade(forest, E.UPG_SPIDER_GROWTH, E.ENTITY_SPIDERS, 40, 2E+5, 5); //x5 -  8.8E4 / 4.4 + 2 E5
            CreateGrowthUpgrade(forest, E.UPG_BIRCH_GROWTH, E.ENTITY_BIRCHES, 7, Statics.GetNumber(2, 5), 100); //x1000 - 4.9E4 / 4.8 + 2E5

            CreateGrowthUpgrade(forest, E.UPG_YEW_GROWTH, E.ENTITY_YEWS, 35, Statics.GetNumber(1, 6), 4); //x16 -  1.2E4 / 1.1 + 1 E6
            CreateGrowthUpgrade(forest, E.UPG_OAK_GROWTH, E.ENTITY_OAKS, 80, Statics.GetNumber(2, 6), 3); //x48 - 7.68E3 / 2 + 2.1E6
            CreateGrowthUpgrade(forest, E.UPG_BUSH_GROWTH, E.ENTITY_BUSHES, 100, Statics.GetNumber(1.5, 6), 4); //x60 - 3.6E3 / 2.7 + 1.5 E6

            CreateGrowthUpgrade(forest, E.UPG_ANT_GROWTH, E.ENTITY_ANTS, 80, Statics.GetNumber(6, 6), 3); //x75 - 2.16E4 / 0.6 + 0.4 E7
            CreateGrowthUpgrade(forest, E.UPG_YEW_GROWTH, E.ENTITY_YEWS, 60, Statics.GetNumber(1, 6), 4); //x64 -  8.8E4 / 1.2 + 2.1 E7
            CreateGrowthUpgrade(forest, E.UPG_SPIDER_GROWTH, E.ENTITY_SPIDERS, 80, 5E+7, 5); //x25 -  1.7E5 / 2 + 5 E7

            CreateGrowthUpgrade(forest, E.UPG_OAK_GROWTH, E.ENTITY_OAKS, 120, Statics.GetNumber(0.9, 8), 3); //x240 - 5.8E4 / 0.9 + .9E8
            CreateGrowthUpgrade(forest, E.UPG_ANT_GROWTH, E.ENTITY_ANTS, 120, Statics.GetNumber(2, 8), 6); //x450 - 1.3E5 / 2.0 + 2.0 E8
            CreateGrowthUpgrade(forest, E.UPG_BUSH_GROWTH, E.ENTITY_BUSHES, 150, Statics.GetNumber(1, 9), 4); //x240 - 2.2E4 / 3.2 + 1 E8
            CreateGrowthUpgrade(forest, E.UPG_BIRCH_GROWTH, E.ENTITY_BIRCHES, 13, Statics.GetNumber(4, 8), 20); //x20000 - 1.8E6 / 3.5 + 4E8

            CreateGrowthUpgrade(forest, E.UPG_YEW_GROWTH, E.ENTITY_YEWS, 100, Statics.GetNumber(5, 8), 3); //x192 -  4.41E5 / 0.5 + 0.5 E9

            CreateGrowthUpgrade(forest, E.UPG_OAK_GROWTH, E.ENTITY_OAKS, 170, Statics.GetNumber(1, 10), 20); //x4800 - 1.6E6 / 1 + 1E10
            CreateGrowthUpgrade(forest, E.UPG_BUSH_GROWTH, E.ENTITY_BUSHES, 200, Statics.GetNumber(1, 10), 9); //x2160 - 2.6E5 / 3.7 + 1 E10
            CreateGrowthUpgrade(forest, E.UPG_SPIDER_GROWTH, E.ENTITY_SPIDERS, 150, 0.4E+10, 10); //x250 -  3.2E6 / 1.6 + 0.4 E10
            CreateGrowthUpgrade(forest, E.UPG_FLOWER_GROWTH, E.ENTITY_FLOWERS, 150, Statics.GetNumber(2, 10), 8, () => forest.OwnsEntity(E.UPG_FREE_HEALTHCARE)); //x8 - 5.3E6 / 3.2 + 2.0 E10 (R Free Healthcare)

            CreateGrowthUpgrade(forest, E.UPG_YEW_GROWTH, E.ENTITY_YEWS, 150, Statics.GetNumber(4, 10), 8); //x1536 -  5.3E6 / 0.6 + 0.4 E11
            CreateGrowthUpgrade(forest, E.UPG_ANT_GROWTH, E.ENTITY_ANTS, 200, 2E+11, 8); //x3600 - 1.3E5 / 4.2 + 2.0 E11    

            CreateGrowthUpgrade(forest, E.UPG_BIRCH_GROWTH, E.ENTITY_BIRCHES, 21, Statics.GetNumber(5, 11), 4,
                addModifier: new Modifier("add", new Dictionary<string, double>() { { E.ENTITY_BIRCHES + E.DEFENSE, 3 } })); //x80000 - 1.2E7 / 2 + 0.5E12
            CreateGrowthUpgrade(forest, E.UPG_SPIDER_GROWTH, E.ENTITY_SPIDERS, 200, 4E+12, 4); //x1000 -  1.7E7 / 1.8 + 4 E12
            CreateGrowthUpgrade(forest, E.UPG_BUSH_GROWTH, E.ENTITY_BUSHES, 250, 2E+12, 8,
                addModifier: new Modifier("add", new Dictionary<string, double>() { { E.ENTITY_BUSHES + E.HEALTH, 1.5 } })); //x17280 - 2.6E6 / 4.4+2 E12

            CreateGrowthUpgrade(forest, E.UPG_OAK_GROWTH, E.ENTITY_OAKS, 250, 1E+13, 5); //x19200 - 1.2E7 / 2.2 + 1E13
            CreateGrowthUpgrade(forest, E.UPG_YEW_GROWTH, E.ENTITY_YEWS, 200, 3E+13, 10); //x15360 -  7.1E7 / 0.8 + 3 E13
            CreateGrowthUpgrade(forest, E.UPG_ANT_GROWTH, E.ENTITY_ANTS, 250, 4.2E+13, 5); //x18000 - 1.3E5 / 4.9 + 4.2 E13

            CreateGrowthUpgrade(forest, E.UPG_BIRCH_GROWTH, E.ENTITY_BIRCHES, 25, Statics.GetNumber(2, 14), 12,
                addModifier: new Modifier("add", new Dictionary<string, double>() { { E.ENTITY_BIRCHES + E.DEFENSE, 4 } })); //x960000 - 1.7E8 / 1.9 + 2E14
            CreateGrowthUpgrade(forest, E.UPG_BUSH_GROWTH, E.ENTITY_BUSHES, 300, 2E+14, 5); //x86400 - 1.6E7 / 5.2+2 E14

            CreateGrowthUpgrade(forest, E.UPG_ANT_GROWTH, E.ENTITY_ANTS, 300, 0.5E+15, 5); //x18000 - 7.8E7 / 0.5 + 1 E15
            CreateGrowthUpgrade(forest, E.UPG_YEW_GROWTH, E.ENTITY_YEWS, 250, 2E+15, 4); //x61440 -  7.1E7 / 0.9 + 2 E15
            CreateGrowthUpgrade(forest, E.UPG_OAK_GROWTH, E.ENTITY_OAKS, 300, 2E+15, 9); //153600 - 9.2E7 / 2.6 + 2E15
            CreateGrowthUpgrade(forest, E.UPG_SPIDER_GROWTH, E.ENTITY_SPIDERS, 250, 4E+15, 3); //x3000 -  7.1E7 / 3.9 + 5 E15

            CreateGrowthUpgrade(forest, E.UPG_YEW_GROWTH, E.ENTITY_YEWS, 300, 4E+17, 6,
                addModifier: new Modifier("add", new Dictionary<string, double>() { { E.GAIN, 2 } })); //x368640 -  2.5E9 / 1 + 4 E17 - Yew upgrade bliver dyrere og dyrere i forhold til yewsne selv

            CreateGrowthUpgrade(forest, E.UPG_BUSH_GROWTH, E.ENTITY_BUSHES, 400, 1.5E+18, 10); //x864000 - 2.1E9 / 7.2+1.5 E18

            CreateGrowthUpgrade(forest, E.UPG_YEW_GROWTH, E.ENTITY_YEWS, 350, 1.5E+20, 4); //x3E6 -  2.4E10 / 0.1 + 1.5 E20 - Yew upgrade bliver dyrere og dyrere i forhold til yewsne selv

            CreateGrowthUpgrade(forest, E.UPG_YEW_GROWTH, E.ENTITY_YEWS, 400, 4E+22, 4,
                addModifier: new Modifier("add", new Dictionary<string, double>() { { E.GAIN, 2 } })); //x12E6 -  4.4E11 / 0.1 + 4 E22 - Yew upgrade bliver dyrere og dyrere i forhold til yewsne selv

            CreateGrowthUpgrade(forest, E.UPG_BUSH_GROWTH, E.ENTITY_BUSHES, 500, 0.5E23, 5,
                addModifier: new Modifier("add", new Dictionary<string, double>() { { E.ENTITY_BUSHES + E.HEALTH, 2 } })); //x4.3E10 - 1.3E10 / 1+0.5 E23
        }

        private static void InitUpgrades(Forest forest) {
            InitGrowthUpgrades(forest);
            // Paths
            UpgradesBigBirches(forest);
            UpgradesSpiders(forest);

            // Other Upgrades
            // Dense forest bonus (500 DC growths)
            forest.AddEntity(new Upgrade(forest, E.UPG_DENSE_FOREST, "Gives 300% production bonus to everything.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, Statics.GetNumber(1, 10) } }), new Modifier(E.UPG_DENSE_FOREST, new Dictionary<string, double>() { { E.GAIN, 4 } }),
                () => forest.GetValue(E.COUNT + E.ENTITY_OAKS) + forest.GetValue(E.COUNT + E.ENTITY_YEWS) +
                forest.GetValue(E.COUNT + E.ENTITY_BIRCHES) + forest.GetValue(E.COUNT + E.ENTITY_BUSHES) >= 500));
            forest.Entities[E.UPG_DENSE_FOREST].Injects[E.INJ_REQUIREMENTS].Add((f, g, arguments) => {
                if (f.GetValue(E.COUNT + E.ENTITY_OAKS) + f.GetValue(E.COUNT + E.ENTITY_YEWS) + f.GetValue(E.COUNT + E.ENTITY_BIRCHES) + f.GetValue(E.COUNT + E.ENTITY_BUSHES) >= 500) {
                    return new RuntimeValue(3, true);
                }
                return new RuntimeValue(3, false);
            });



            // Thinking skill upgrades

            //Druidcraft
            forest.AddEntity(new Upgrade(forest, E.UPG_DRUIDCRAFT_CONSIDERED, "Gives 3% production bonus per Druidcraft level.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, Statics.GetNumber(1, 1) } }), new Modifier(E.UPG_DRUIDCRAFT_CONSIDERED, new Dictionary<string, double>() { { E.GAIN, 1.00 } }),
                "", E.TRG_THINK_COMPLETED));
            forest.Entities[E.UPG_DRUIDCRAFT_CONSIDERED].Injects["ownedLoop"].Add((f, g, arguments) => {
                f.Modifiers[E.UPG_DRUIDCRAFT_CONSIDERED].ModifiersF[E.GAIN] = f.GetValue(E.LEVEL + E.DRUIDCRAFT) * f.GetValue(E.UPG_DRUIDCRAFT_CONSIDERED + E.MOD) + 1;
                return null;
            });

            //Animal Handling
            forest.AddEntity(new Upgrade(forest, E.UPG_HANDLED_YOU_BEFORE, "Gives 1% attack bonus per Animal Handling level.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, Statics.GetNumber(1, 1) } }), new Modifier("Handled You Before", new Dictionary<string, double>() { { "Attack", 1.00 } }),
                "", E.TRG_THINK_COMPLETED));
            forest.Entities[E.UPG_HANDLED_YOU_BEFORE].Injects["ownedLoop"].Add((f, g, arguments) => {
                f.Modifiers[E.UPG_HANDLED_YOU_BEFORE].ModifiersF[E.ATTACK] = f.GetValue(E.LEVEL + E.ANIMAL_HANDLING) * f.GetValue(E.UPG_HANDLED_YOU_BEFORE + E.MOD) + 1;
                return null;
            });

            //Soothing
            forest.AddEntity(new Upgrade(forest, E.UPG_SOOTHING_THOUGHTS, "Gives 1% Health regen per Soothing level.", null,
                new Resources(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, Statics.GetNumber(1, 1) } }), new Modifier(E.UPG_SOOTHING_THOUGHTS, new Dictionary<string, double>() { { E.HEALTHREGEN, 1 } }),
                E.SV_THINKS + "_>_1", E.TRG_THINK_COMPLETED));
            forest.Entities[E.UPG_SOOTHING_THOUGHTS].Injects["ownedLoop"].Add((f, g, arguments) => {
                f.Modifiers[E.UPG_SOOTHING_THOUGHTS].ModifiersF[E.HEALTHREGEN] = f.GetValue(E.LEVEL + E.SOOTHING) * f.GetValue(E.UPG_SOOTHING_THOUGHTS + E.MOD) + 1;
                return null;
            });
        }

        public static void FirstInit(Forest forest) {
            upgrades.Clear();
            foreach (string skill in Statics.skills) {
                forest.SoftValues[E.SV_LEVEL + skill] = 0;
            }
            forest.AddEntity(new Entity(forest, E.ORGANIC_MATERIAL));


            //Bushes
            forest.AddEntity(new Entity(forest, E.ENTITY_BUSHES)
            .Add(new EExtGenerate(new(string, Formula)[] { (E.ORGANIC_MATERIAL, new FormulaLinear("!I0", E.ENTITY_BUSHES + E.GAIN)) }))
            .Add(new EExtBuyable(new ResourcesIncrement(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 20 } }, E.ENTITY_BUSHES + E.INC, E.COUNT + E.ENTITY_BUSHES)))
            .Add(new EExtXpMod(1)));

            // Oaks
            forest.AddEntity(new Entity(forest, E.ENTITY_OAKS)
            .Add(new EExtGenerate(new(string, Formula)[] { (E.ORGANIC_MATERIAL, new FormulaLinear("!I0", E.ENTITY_OAKS + E.GAIN)) }))
            .Add(new EExtBuyable(new ResourcesIncrement(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 100 } }, E.ENTITY_OAKS + E.INC, E.COUNT + E.ENTITY_OAKS)))
            .Add(new EExtXpMod(3)));

            // Ants
            forest.AddEntity(new Entity(forest, E.ENTITY_ANTS)
            .Add(new EExtGenerate(new(string, Formula)[] { (E.ORGANIC_MATERIAL, new FormulaLinear("!I0", E.ENTITY_ANTS + E.GAIN)) }))
            .Add(new EExtBuyable(new ResourcesIncrement(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 220 } }, E.ENTITY_ANTS + E.INC, E.COUNT + E.ENTITY_ANTS)))
            .Add(new EExtXpMod(3, E.ANIMAL_HANDLING)));
            forest.Entities[E.ENTITY_ANTS].Injects["loop"].Add((f, g, arguments) => {
                if (f.GetValue("DefeatedBosses") >= 1) {
                    g.Unlocked = true;
                }
                return null;
            });

            // Birches
            forest.AddEntity(new Entity(forest, E.ENTITY_BIRCHES)
            .Add(new EExtGenerate(new(string, Formula)[] { (E.ORGANIC_MATERIAL, new FormulaLinear("!I0", E.ENTITY_BIRCHES + E.GAIN)) }))
            .Add(new EExtBuyable(new ResourcesIncrement(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 440 } }, E.ENTITY_BIRCHES + E.INC, E.COUNT + E.ENTITY_BIRCHES)))
            .Add(new EExtXpMod(20))
            .Add(new EExtLimited()));
            forest.Entities[E.ENTITY_BIRCHES].Injects["loop"].Add((f, g, arguments) => {
                if (!g.Unlocked && f.GetValue(E.DEFEATED_BOSSES) >= 1) {
                    g.Unlocked = true;
                }
                return null;
            });

            // Yews; unlocked at 15 druidcraft
            forest.AddEntity(new Entity(forest, E.ENTITY_YEWS)
            .Add(new EExtGenerate(new(string, Formula)[] { (E.ORGANIC_MATERIAL, new FormulaLinear("!I0", E.ENTITY_YEWS + E.GAIN)) }))
            .Add(new EExtBuyable(new ResourcesIncrement(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 4000 } }, E.ENTITY_YEWS + E.INC, E.COUNT + E.ENTITY_YEWS)))
            .Add(new EExtXpMod(40)));
            forest.Entities[E.ENTITY_YEWS].Injects["loop"].Add((f, g, arguments) => {
                if (!g.Unlocked && f.GetValue(E.SV_LEVEL + E.DRUIDCRAFT) >= 15) {
                    g.Unlocked = true;
                }
                return null;
            });

            // Flowers; unlocked after sixth boss
            forest.AddEntity(new Entity(forest, E.ENTITY_FLOWERS)
            .Add(new EExtGenerate(new(string, Formula)[] { (E.ORGANIC_MATERIAL, new FormulaLinear("!I0", E.ENTITY_FLOWERS + E.GAIN)) }))
            .Add(new EExtBuyable(new ResourcesIncrement(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 2000 } }, E.ENTITY_FLOWERS + E.INC, E.COUNT + E.ENTITY_FLOWERS)))
            .Add(new EExtXpMod(9, E.SOOTHING)));
            forest.Entities[E.ENTITY_FLOWERS].Injects["loop"].Add((f, g, arguments) => {
                if (!g.Unlocked && f.GetValue(E.DEFEATED_BOSSES) >= 6) {
                    g.Unlocked = true;
                }
                return null;
            });

            // Spiders, Unlocked by spider path
            forest.AddEntity(new Entity(forest, E.ENTITY_SPIDERS)
            .Add(new EExtGenerate(new(string, Formula)[] { (E.ORGANIC_MATERIAL, new FormulaLinear("!I0", E.ENTITY_SPIDERS + E.GAIN)) }))
            .Add(new EExtBuyable(new ResourcesIncrement(new Dictionary<string, double>() { { E.ORGANIC_MATERIAL, 1000 } }, E.ENTITY_SPIDERS + E.INC, E.COUNT + E.ENTITY_SPIDERS)))
            .Add(new EExtXpMod(5, E.ANIMAL_HANDLING)));
            forest.Entities[E.ENTITY_SPIDERS].Injects["loop"].Add((f, g, arguments) => {
                if (g.Unlocked)
                    forest.GetModifier(E.ENTITY_SPIDERS + E.ATTACK + E.MOD).ModifiersF[E.ENTITY_SPIDERS + E.ATTACK] = 1 + 0.05 * forest.GetValue(E.SV_LEVEL + E.ANIMAL_HANDLING);
                return null;
            });

            // Rejuvenate, heals 5+0.5*druidcraft level
            forest.AddDoable(new Doable(forest, "Rejuvenate", null, "", "Rejuvenate!", "Rejuvenate is on cooldown.", true, E.ABIL_REJUVENATE + E.MANA));
            forest.Doables["Rejuvenate"].Injects["perform"].Add((f, g, arguments) => {
                double hpIncrease = forest.GetValue(E.ABIL_REJUVENATE + E.MOD) * (1 + forest.GetValue(E.SV_LEVEL + E.DRUIDCRAFT) * 0.1);
                if (f.GetValue("UpgradeSoothing RejuvenationBought") != 0) {
                    hpIncrease += f.GetStats()["Soothing"] * (1 + 0.02 * f.GetValue(E.SV_LEVEL + E.DRUIDCRAFT));
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
                if (f.OwnsEntity("Transmogrify Rageuvenate")) {
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
            forest.Doables[E.ABIL_REJUVENATE].Requirements += E.ABIL_REJUVENATE + E.COOLDOWN + "_<=_0";


            // DEBUG DOABLE; HALF HOUR GENERATION
            forest.AddDoable(new Doable(forest, "Halfhour Offline", null, "", "Halfhour Offline!", "", true, "!I0"));
            forest.Doables["Halfhour Offline"].Injects["perform"].Add((f, g, arguments) => {
                f.TickOffline(5 * 60 * 30);
                return null;
            });

            InitTrophies(forest);
            InitLoot(forest);
            InitTalents(forest);

            forest.AddEntity(new Entity(forest, "wand", E.GRP_ITEMS)
                .Add(new EExtXpMod(1)));

            // ITEMS TODO: Do something with them

            InitUpgrades(forest); // Initializes upgrades in its own function

            foreach (Entity e in forest.Entities.Values) {
                forest.SoftValues[E.SV_COUNT + e.Name] = 0;
                forest.SoftValues[E.SV_MAX_COUNT + e.Name] = 0;
            }

        }

        public static CodeInject CreateRequirementInject(string requirements) {
            return (f, g, arguments) => {
                if (f.TestRequirements(requirements)) {
                    g.Unlocked = true;
                }
                return null;
            };
        }
        public static CodeInject CreateRequirementInject(Func<bool> requirements) {
            return (f, g, arguments) => {
                if (requirements()) {
                    g.Unlocked = true;
                }
                return null;
            };
        }

        public static void InitTrophies(Forest forest) {
            //TROPHIES
            forest.AddEntity(new Trophy(forest, E.TROPHY_SOOTHED_20TH_BOSS, "After encountering your twentieth adversary, You feel like you have learned a lot from the path you have taken. You consider returning to your deep woods and rethinking your strategy, having eternity at your disposal. (Think unlocked)",
                E.DEFEATED_BOSSES + "_>=_!I20\n" + E.STYLE_SOOTHE + "_>_0", E.TRG_DEFEATED_BOSS));
            forest.AddEntity(new Trophy(forest, E.TROPHY_WEBBED_20TH_BOSS, "You webbed the 20th boss! Very nice.",
                E.DEFEATED_BOSSES + "_>=_!I20\n" + E.UPGRADE + E.UPG_SPIDERFRIENDS + E.BOUGHT + "_>_0", E.TRG_DEFEATED_BOSS, E.TRG_ENTITY_ADDED));

            // AMOUNT TROPHIES
            forest.AddEntity(new Trophy(forest, E.TROPHY_250_BUSHES, "You have acquired 250 bushes.",
                E.COUNT + E.ENTITY_BUSHES + "_>=_!I250", E.TRG_ENTITY_ADDED));
        }

        public static void InitTalents(Forest forest) {
            // DRUIDCRAFT
            forest.AddEntity(new Talent(forest, E.TALENT_DC_CLEAR_SKIES, "Druidically control the weather, making photosynthetics grow 250% faster.", E.DRUIDCRAFT,
                new Modifier(E.TALENT_DC_CLEAR_SKIES, new Dictionary<string, double>()
                { { E.ENTITY_BUSHES+E.GAIN, 3.5}, { E.ENTITY_OAKS+E.GAIN, 3.5 },{ E.ENTITY_BIRCHES+E.GAIN, 3.5 },
                    {E.ENTITY_YEWS+E.GAIN, 3.5 },{E.ENTITY_FLOWERS+E.GAIN,3.5 } }))
                .AddRequirements(()=>forest.GetValue(E.LEVEL+E.DRUIDCRAFT)>=10, E.TRG_LEVEL_UP));

            forest.AddEntity(new Talent(forest, E.TALENT_DC_IRONBARK, "Develop Ironbark, reducing damage taken by 15%.", E.DRUIDCRAFT,
                new Modifier(E.TALENT_DC_IRONBARK, new Dictionary<string, double>()
                { { E.DAMAGE_TAKEN, 0.85 } }))
                .AddRequirements(() => forest.GetValue(E.LEVEL + E.DRUIDCRAFT) >= 10, E.TRG_LEVEL_UP));

            // ANIMAL HANDLING
            forest.AddEntity(new Talent(forest, E.TALENT_AH_WEBS, "Unlock webs. If you're going down the Spider path, this talent will not help you.", E.ANIMAL_HANDLING)
                .AddRequirements(() => forest.GetValue(E.LEVEL + E.ANIMAL_HANDLING) >= 10 && !forest.OwnsEntity(E.UPG_SPIDERFRIENDS), E.TRG_LEVEL_UP));
            forest.Entities[E.TALENT_AH_WEBS].AddTrigger(E.INJ_CREATE, (f, g, args) => {
                forest.Entities[E.ENTITY_WEBS].Unlocked = true;
                return null;
            });
        }

        public static string GetLoot(string tableName) {
            (double chance, string item)[] table = lootTables[tableName];
            double weight = r.NextDouble();
            double runningTotal = 0;
            foreach (var (chance, item) in table ) {
                runningTotal += chance;
                if (weight < runningTotal)
                    return item;
            }
            return null;
        }
        public static void InitLoot(Forest forest) {
            // LOOT
            // Drops from the 24th boss on the avg path
            forest.AddEntity(new Entity(forest, E.ITEM_CORRUPT_LIFE_SEED, E.GRP_ITEMS, 1));
            forest.Entities[E.ITEM_CORRUPT_LIFE_SEED].AddTrigger(E.TRG_DEFEATED_BOSS, (f, g, args) => { //Should probably be added some other way.
                if (f.CurPath.Name == "Forest standard dudes" && f.GetValue(E.DEFEATED_BOSSES) == 24 && !g.Unlocked) {
                    Console.WriteLine("You found the Seed of Life!");
                    f.AddItem(E.ITEM_CORRUPT_LIFE_SEED, 1);
                }
                return null;
            });
            forest.Entities[E.ITEM_CORRUPT_LIFE_SEED].Description = "The Seed of life, currently corrupted. A spatula must be found, that can contain its corruption, before it can be planted.";


            // Random loot
            // Heavy rock
            forest.AddEntity(new Entity(forest, E.ITEM_HEAVY_ROCK, E.GRP_ITEMS, 0)
                .Add(new EExtModifier(new Modifier(E.ITEM_HEAVY_ROCK, new Dictionary<string, double>() { { E.HEALTH, 1.1 }, { E.DEFENSE, 1.25 } }), true)));
            forest.Entities[E.ITEM_HEAVY_ROCK].Description = "A heavy rock. +10% health, +25% defense.";
            // Tasty Seeds
            forest.AddEntity(new Entity(forest, E.ITEM_TASTY_SEEDS, E.GRP_ITEMS, 0)
                .Add(new EExtModifier(new Modifier(E.ITEM_TASTY_SEEDS, new Dictionary<string, double>() { { E.ANIMAL_HANDLING+E.XP+E.GAIN, 1.5 } }), true)));
            forest.Entities[E.ITEM_TASTY_SEEDS].Description = "Tasty seeds. +50% Animal Handling xp gain, +20% bird gain.";
            // Torn Journal
            forest.AddEntity(new Entity(forest, E.ITEM_TORN_JOURNAl, E.GRP_ITEMS, 0)
                .Add(new EExtModifier(new Modifier(E.ITEM_TORN_JOURNAl, new Dictionary<string, double>() { { E.DRUIDCRAFT + E.XP + E.GAIN, 1.4 }, { E.SOOTHING, 1.2 } }), true)));
            forest.Entities[E.ITEM_TORN_JOURNAl].Description = "A torn journal. +40% Druidcraft xp, +20% soothing.";
            if(lootTables.Count == 0) {
                lootTables.Add(E.LOOT_RANDOM, new(double, string)[] { (0.3333, E.ITEM_HEAVY_ROCK), (0.3333, E.ITEM_TASTY_SEEDS), (0.3334, E.ITEM_TORN_JOURNAl) });
            }
        }

        public static void InitBosses(Forest forest) {
            Path.paths.Clear();
            startPath = new Path(forest, "Forest Street", "A little path through the forest.", "");
            for (int loop = 1; loop <= 12; loop++) {
                startPath.AddBoss(GenerateBoss(loop));
                if(loop%4 == 0) {
                    startPath.AddBoss(new Chest("Chest", E.LOOT_RANDOM, 3));
                }
            }
            Path hpPath = new Path(forest, "Forest heavy dudes", "Heavy people.", "");

            for (int loop = 13; loop <= 24; loop++) {
                hpPath.AddBoss(GenerateBoss(loop, 1));
            }

            Path avgPath = new Path(forest, "Forest standard dudes", "Standard people.", "") {
                ShowRequirements = E.SV_THINKS + "_>_0"
            };

            Branch startBranch = new Branch(forest, "firstBranch", "Branch");
            startBranch.AddPath(avgPath);
            startBranch.AddPath(hpPath);
            startPath.EndBranch = startBranch;

            for (int loop = 13; loop <= 23; loop++) {
                avgPath.AddBoss(GenerateBoss(loop));
            }

            avgPath.AddBoss(GenerateBoss(24,mod:2));


            forest.SetPath(startPath);
        }

        public static void Init(Forest forest) {

            premadeInjects = new Dictionary<string, CodeInject>() {
                { E.INJ_TOOLTIP + E.COOLDOWN, (f,g,arguments) => {
                    if (forest.GetValue(g.Name+E.COOLDOWN) > 0) {
                        return new RuntimeValue(4, "[" + /*Conversion to seconds*/(forest.GetValue(g.Name+E.COOLDOWN) / 5) + "s CD]");
                    }
                    return new RuntimeValue(4, "");
                } },
                { E.INJ_TOOLTIP + E.ACTIVE, (f,g,arguments) => {
                    if (forest.GetValue(g.Name + E.ACTIVE) > 0) {
                        return new RuntimeValue(4, "[" + /*Conversion to seconds*/(forest.GetValue(g.Name + E.ACTIVE) / 5) + "s A]");
                    } else if (forest.GetValue(g.Name+E.COOLDOWN) > 0) {
                        return new RuntimeValue(4, "[" + /*Conversion to seconds*/(forest.GetValue(g.Name+E.COOLDOWN) / 5) + "s CD]");
                    }
                    return new RuntimeValue(4, "");
                } }
            };


            if (firstInit)
                FirstInit(forest);
            ResetValues.InitValues(forest);

            if (firstInit)
                forest.AddItem("wand", 1);

            InitBosses(forest);




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




            if (firstInit) {
                forest.StartCalculation();
            }

            firstInit = false;

            // DEBUG
            /*forest.SetPath(birdPath);
            forest.Growths["Organic Material"].Amount = 50000;
            forest.Values["DefeatedBosses"] = 3;*/
            // A modifier for debugging
            //forest.Modifiers.Add(new Modifier("debug", new Dictionary<string, double>() { { "BushesGain", 10000 }, { "OaksGain", 10000 }, { "BirchesGain", 10000 },
            //    { "HealthRegen", 10000 }, { "Attack", 10000 }, { "Health", 10000 } }));
        }

        public static Dictionary<string, int> upgrades = new Dictionary<string, int>();
        /// <summary>
        /// Creates a new upgrade that increases the gain of a Growth.
        /// name - name of the upgrade
        /// target - name of the growth
        /// amount - how many growths are needed for it
        /// cost - price of the upgrade
        /// multiplier - what modF is added to the growth.
        /// addModifier - modifier added to the upgrade's modifier.
        /// </summary>
        /// <param name="forest"></param>
        /// <param name="name"></param>
        /// <param name="target"></param>
        /// <param name="amount"></param>
        /// <param name="neededAmount"></param>
        /// <param name="cost"></param>
        /// <param name="multiplier"></param>
        public static void CreateGrowthUpgrade(Forest forest, string name, string target, int amount, double cost, double multiplier, Func<bool> requirements = null, Modifier addModifier = null) {
            if (!upgrades.ContainsKey(name))
                upgrades.Add(name, 1);
            else
                upgrades[name] += 1;
            name += " " + upgrades[name];
            Modifier resModifier = new Modifier(name, new Dictionary<string, double>() { { target + "Gain", multiplier } });
            if (addModifier != null)
                resModifier.AddModifier(addModifier);
            forest.AddEntity(new Upgrade(forest, name, "improves " + target + " gain by " + (multiplier * 100 - 100) + "%.", null,
                new Resources(new Dictionary<string, double>() { { "Organic Material", cost } }), resModifier, ()=> forest.GetValue(E.COUNT + target) >= amount && (requirements != null ? requirements() : true), E.TRG_ENTITY_ADDED));
            if (addModifier != null) {
                if (resModifier.ModifiersA != null)
                    foreach (KeyValuePair<string, double> aKvp in resModifier.ModifiersA) {
                        ((Upgrade)forest.Entities[name]).PreDescriptionText += "\n+" + (aKvp.Value * 100 - 100) + " base " + aKvp.Key;
                    }
                if (resModifier.ModifiersF != null)
                    foreach (KeyValuePair<string, double> fKvp in resModifier.ModifiersF) {
                        if (fKvp.Key != target + E.GAIN)
                            ((Upgrade)forest.Entities[name]).PreDescriptionText += "\n+" + (fKvp.Value * 100 - 100) + "% " + fKvp.Key;
                    }
                if (resModifier.ModifiersAAfter != null)
                    foreach (KeyValuePair<string, double> aKvp in resModifier.ModifiersAAfter) {
                        ((Upgrade)forest.Entities[name]).PreDescriptionText += "\n+" + (aKvp.Value * 100 - 100) + " " + aKvp.Key + " after modifiers.";
                    }
            }
        }

        /// <summary>
        /// Bosstypes:
        ///  0 - Average
        ///  1 - Hp Weighted
        ///  2 - Light & Fast TODO
        /// </summary>
        /// <param name="boss"></param>
        /// <param name="bossType"></param>
        /// <returns></returns>
        public static Fighter GenerateBoss(int boss, int bossType = 0, double mod=1) {
            Random r = new Random(boss);
            double powerMult = 6 * boss * Math.Pow(1.15, boss)*mod;
            double hpScale = 0.8 + r.NextDouble() * 0.4;
            double attackScale = 0.8 + r.NextDouble() * 0.4;
            double defenseScale = r.NextDouble() - 0.7;
            double defense = 0;

            if (bossType == 1) {
                attackScale /= 1.5;
                hpScale *= 4.5;
            }

            if (defenseScale > 0) {
                hpScale -= defenseScale * 3;
                defense = defenseScale * powerMult;
            }

            return new Fighter(powerMult * 10 * hpScale, powerMult * attackScale * 0.5, defense, "Boss " + boss, new Resources(new Dictionary<string, double>()), new Dictionary<string, int>(), "", "")
                .AddLootTable(E.LOOT_RANDOM);
        }
    }
}
