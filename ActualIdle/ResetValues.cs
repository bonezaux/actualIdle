using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {
    public class ResetValues {
        public static void InitValues(Forest forest) {

            forest.Growths[E.ORGANIC_MATERIAL].Unlocked = true;

            forest.Values["wandlevel"] = 0;
            forest.Values[E.BOUGHT_THINGS] = 0;
            forest.Values["allowedGrowths"] = 2;

            forest.AddModifier(new Modifier("Base Stats", modifiersA: new Dictionary<string, double>() { { "HealthRegen", 0.2 }, { E.MAXMANA, 120}, { E.MANAREGEN, 1 } }));

            //Modifier som GrowthDruids can increase når de increaser xp gain af noget
            //Druidcraft XP: Income
            //Animal Handling XP: Damage givet
            //Soothing XP: Soothing udført

            // BUSHES
            forest.Values[E.BUSHES+E.GAIN] = 0.6;
            forest.Values[E.BUSHES + E.ATTACK] = 0.2;
            forest.Values[E.BUSHES + E.HEALTH] = 0.2;
            forest.Values[E.BUSHES + E.INC] = 1.1;
            forest.Growths[E.BUSHES].Amount = 1;
            forest.Growths[E.BUSHES].Unlocked = true;
            forest.Growths[E.BUSHES].Description = "Adds 0.2 attack and 0.2 health each.";

            // OAKS
            forest.Values[E.OAKS + E.GAIN] = 2;
            forest.Values[E.OAKS + E.HEALTH] = 1;
            forest.Values[E.OAKS + E.INC] = 1.1;
            forest.Growths[E.OAKS].Unlocked = true;
            forest.Growths[E.OAKS].Description = "Adds 1 health each.";

            // ANTS
            forest.Values[E.ANTS + E.GAIN] = 3.6;
            forest.Values[E.ANTS + E.ATTACK] = 0.8;
            forest.Values[E.ANTS + E.INC] = 1.1;
            forest.Growths[E.ANTS].Description = "Adds 0.8 attack each. Gives 3xp in Animal handling.";

            // BIRCHES
            forest.Values[E.BIRCHES+E.GAIN] = 7;
            forest.Values[E.BIRCHES+E.DEFENSE] = 0.25;
            forest.Values[E.BIRCHES+E.INC] = 3;
            forest.Growths[E.BIRCHES].Description = "Adds 0.25 defense each. Is a limited growth. Each birch costs 3x the last.";



            // YEWS
            forest.Values[E.YEWS + E.GAIN] = 23;
            forest.Values[E.YEWS + E.HEALTH] = 4;
            forest.Values[E.YEWS + E.INC] = 1.1;
            forest.Growths[E.YEWS].Description = "Adds 4 health each.";

            // FLOWERS
            forest.Values[E.FLOWERS + E.HEALTHREGEN] = 0.2;
            forest.Values[E.FLOWERS + E.SOOTHING] = 2;
            forest.Values[E.FLOWERS + E.INC] = 1.1;

            // SPIDERS
            forest.Values[E.SPIDERS + E.GAIN] = 85;
            forest.Values[E.SPIDERS + E.ATTACK] = 1;
            forest.AddModifier(new Modifier(E.SPIDERS+E.ATTACK+E.MOD, new Dictionary<string, double>() { { E.SPIDERS+E.ATTACK, 1 } }));
            forest.Values[E.SPIDERS+E.INC] = 1.1;

            // DOABLES

            forest.Values[E.REJUVENATE+E.MOD] = 5;
            forest.Values[E.REJUVENATE+E.COOLDOWN+E.MOD] = 1; //Is a speed, making it higher will make it faster.
            forest.Values[E.REJUVENATE + E.MANA] = 15;
            forest.Doables[E.REJUVENATE].Unlocked = true;
            forest.Doables[E.REJUVENATE].Requirements += E.REJUVENATE+E.COOLDOWN+"_ <= _0";


            // Harmony 
            forest.Values[E.HARMONY+E.TIME] = 15 * 5; // Ticks harmony lasts
            forest.Values[E.HARMONY+E.COOLDOWN+E.TIME] = 60 * 5; // Ticks harmony cooldowns
            forest.Doables[E.HARMONY].Unlocked = false;
            forest.Doables[E.HARMONY].Requirements += E.HARMONY+E.COOLDOWN+"_<=_0";
            forest.Values[E.HARMONY + E.MANA] = 100;

            // Rageuvenate
            forest.Values[E.RAGEUVENATE+E.COOLDOWN+E.TIME] = 30 * 5;
            forest.Doables[E.RAGEUVENATE].Requirements += E.RAGEUVENATE + E.COOLDOWN+"_<=_0";
            forest.Values[E.RAGEUVENATE + E.DAMAGE + E.TIME] = 2; // How many rounds of damage Rageuvenate deals
            forest.Values[E.RAGEUVENATE + E.INCOME + E.TIME] = 12 * 5; // How many ticks of income Rageuvenate gives
            forest.Values[E.RAGEUVENATE + E.MANA] = 45;

            // Web Site
            forest.Values[E.UPG_WEB_SITE + E.MOD] = 0.2; // How much each magnitude of income increases the floating point modifier on spider attack.

            // Web Development & Webs
            forest.Values[E.WEBS + E.GAIN] = 1E+4;
            forest.Values[E.WEBS + E.DEFENSE] = 50;
            forest.Values[E.WEBS + E.ATTACK] = 5;
            forest.Values[E.WEBS + E.INC] = 10;
            forest.AddModifier(new Modifier(E.WEBS, modifiersA: new Dictionary<string, double>() { { E.STALL, 0 }, { E.SPIDERS + E.ATTACK, 0 } }));

            // Testing doable that gives half an hour of passive generation.
            forest.Doables["Halfhour Offline"].Unlocked = true;

            // Think upgrades
            forest.Values[E.UPG_DRUIDCRAFT_CONSIDERED + E.MOD] = 0.01; //How much production boost in % each level of druidcraft gives.
            forest.Values[E.UPG_HANDLED_YOU_BEFORE + E.MOD] = 0.01; //How much attack boost in % each level of Animal Handling gives.
            forest.Values[E.UPG_SOOTHING_THOUGHTS + E.MOD] = 0.01; //How much attack boost in % each level of Soothing gives.

        }
    }
}
