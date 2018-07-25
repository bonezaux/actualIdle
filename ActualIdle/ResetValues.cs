using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {
    public class ResetValues {
        public static void InitValues(Forest forest) {

            forest.Entities[E.ORGANIC_MATERIAL].Unlocked = true;
            forest.FightingStyle = E.STYLE_FIGHT;

            forest.Values[E.DEFEATED_BOSSES] = 0;
            forest.Values["wandlevel"] = 0;
            forest.Values[E.BOUGHT_THINGS] = 0;
            forest.Values["allowedGrowths"] = 2;

            forest.AddModifier(new Modifier("Base Stats", modifiersA: new Dictionary<string, double>() { { "HealthRegen", 0.2 }, { E.MAXMANA, 120}, { E.MANAREGEN, 1 } }));

            forest.AddModifier(new Modifier(E.XP + E.GAIN, modifiersF: new Dictionary<string, double>() { { E.XP + E.GAIN, 1 } }));
            foreach (string skill in Statics.skills) {
                forest.Values[skill + E.XP + E.GAIN] = 1;
                forest.Modifiers[E.XP + E.GAIN].ModifiersF[skill + E.XP + E.GAIN] = 0;
            }
            //Modifier som GrowthDruids can increase når de increaser xp gain af noget
            //Druidcraft XP: Income
            //Animal Handling XP: Damage givet
            //Soothing XP: Soothing udført

            // BUSHES
            forest.Values[E.ENTITY_BUSHES+E.GAIN] = 0.6;
            forest.Values[E.ENTITY_BUSHES + E.ATTACK] = 0.2;
            forest.Values[E.ENTITY_BUSHES + E.HEALTH] = 0.2;
            forest.Values[E.ENTITY_BUSHES + E.INC] = 1.1;
            forest.Entities[E.ENTITY_BUSHES].Amount = 1;
            forest.Entities[E.ENTITY_BUSHES].Unlocked = true;
            forest.Entities[E.ENTITY_BUSHES].Description = "Adds 0.2 attack and 0.2 health each.";

            // OAKS
            forest.Values[E.ENTITY_OAKS + E.GAIN] = 2;
            forest.Values[E.ENTITY_OAKS + E.HEALTH] = 1;
            forest.Values[E.ENTITY_OAKS + E.INC] = 1.1;
            forest.Entities[E.ENTITY_OAKS].Unlocked = true;
            forest.Entities[E.ENTITY_OAKS].Description = "Adds 1 health each.";

            // ANTS
            forest.Values[E.ENTITY_ANTS + E.GAIN] = 3.6;
            forest.Values[E.ENTITY_ANTS + E.ATTACK] = 0.8;
            forest.Values[E.ENTITY_ANTS + E.INC] = 1.1;
            forest.Entities[E.ENTITY_ANTS].Description = "Adds 0.8 attack each. Gives 3% xp mod in Animal handling.";

            // BIRCHES
            forest.Values[E.ENTITY_BIRCHES+E.GAIN] = 7;
            forest.Values[E.ENTITY_BIRCHES+E.DEFENSE] = 0.25;
            forest.Values[E.ENTITY_BIRCHES+E.INC] = 3;
            forest.Entities[E.ENTITY_BIRCHES].Description = "Adds 0.25 defense each. Is a limited growth. Each birch costs 3x the last.";



            // YEWS
            forest.Values[E.ENTITY_YEWS + E.GAIN] = 23;
            forest.Values[E.ENTITY_YEWS + E.HEALTH] = 4;
            forest.Values[E.ENTITY_YEWS + E.INC] = 1.1;
            forest.Entities[E.ENTITY_YEWS].Description = "Adds 4 health each.";

            // FLOWERS
            forest.Values[E.ENTITY_FLOWERS + E.HEALTHREGEN] = 0.2;
            forest.Values[E.ENTITY_FLOWERS + E.SOOTHING] = 2;
            forest.Values[E.ENTITY_FLOWERS + E.INC] = 1.1;

            // SPIDERS
            forest.Values[E.ENTITY_SPIDERS + E.GAIN] = 85;
            forest.Values[E.ENTITY_SPIDERS + E.ATTACK] = 1;
            forest.AddModifier(new Modifier(E.ENTITY_SPIDERS+E.ATTACK+E.MOD, new Dictionary<string, double>() { { E.ENTITY_SPIDERS+E.ATTACK, 1 } }));
            forest.Values[E.ENTITY_SPIDERS+E.INC] = 1.1;

            // DOABLES

            forest.Values[E.ABIL_REJUVENATE+E.MOD] = 5;
            forest.Values[E.ABIL_REJUVENATE + E.COOLDOWN+E.MOD] = 1; //Is a speed, making it higher will make it faster.
            forest.Values[E.ABIL_REJUVENATE + E.MANA] = 15;
            forest.Doables[E.ABIL_REJUVENATE].Unlocked = true;


            // Harmony 
            forest.Values[E.ABIL_HARMONY+E.TIME] = 15 * 5; // Ticks harmony lasts
            forest.Values[E.ABIL_HARMONY+E.COOLDOWN+E.TIME] = 60 * 5; // Ticks harmony cooldowns
            forest.Doables[E.ABIL_HARMONY].Unlocked = false;
            forest.Doables[E.ABIL_HARMONY].Requirements += E.ABIL_HARMONY+E.COOLDOWN+"_<=_0";
            forest.Values[E.ABIL_HARMONY + E.MANA] = 100;

            // Rageuvenate
            forest.Values[E.ABIL_RAGEUVENATE+E.COOLDOWN+E.TIME] = 30 * 5;
            forest.Doables[E.ABIL_RAGEUVENATE].Requirements += E.ABIL_RAGEUVENATE + E.COOLDOWN+"_<=_0";
            forest.Values[E.ABIL_RAGEUVENATE + E.DAMAGE + E.TIME] = 2; // How many rounds of damage Rageuvenate deals
            forest.Values[E.ABIL_RAGEUVENATE + E.INCOME + E.TIME] = 12 * 5; // How many ticks of income Rageuvenate gives
            forest.Values[E.ABIL_RAGEUVENATE + E.MANA] = 45;

            // Surf the Web
            forest.Values[E.ABIL_SURF_THE_WEB + E.TIME] = 20 * 5; // Ticks Surf the Web lasts
            forest.Doables[E.ABIL_SURF_THE_WEB].Unlocked = false;
            forest.Doables[E.ABIL_SURF_THE_WEB].Requirements += E.ABIL_SURF_THE_WEB + E.COOLDOWN + "_<=_0";
            forest.Values[E.ABIL_SURF_THE_WEB + E.MANA] = 100;
            forest.Values[E.ABIL_SURF_THE_WEB + E.TREES + E.MOD] = 0.05;
            forest.Values[E.ABIL_SURF_THE_WEB + E.ANIMALS + E.MOD] = 0.05;

            // Web Site
            forest.Values[E.UPG_WEB_SITE + E.MOD] = 0.2; // How much each magnitude of income increases the floating point modifier on spider attack.

            // Web Development & Webs
            forest.Values[E.ENTITY_WEBS + E.GAIN] = 1E+4;
            forest.Values[E.ENTITY_WEBS + E.DEFENSE] = 50;
            forest.Values[E.ENTITY_WEBS + E.ATTACK] = 5;
            forest.Values[E.ENTITY_WEBS + E.INC] = 10;
            forest.AddModifier(new Modifier(E.ENTITY_WEBS, modifiersA: new Dictionary<string, double>() { { E.STALL, 0 }, { E.ENTITY_SPIDERS + E.ATTACK, 0 } }));

            // Testing doable that gives half an hour of passive generation.
            forest.Doables["Halfhour Offline"].Unlocked = true;

            // Think upgrades
            forest.Values[E.UPG_DRUIDCRAFT_CONSIDERED + E.MOD] = 0.03; //How much production boost in % each level of druidcraft gives.
            forest.Values[E.UPG_HANDLED_YOU_BEFORE + E.MOD] = 0.03; //How much attack boost in % each level of Animal Handling gives.
            forest.Values[E.UPG_SOOTHING_THOUGHTS + E.MOD] = 0.03; //How much attack boost in % each level of Soothing gives.

        }
    }
}
