using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {
    public class ResetValues {
        public static void InitValues(Forest forest) {

            forest.Growths["Organic Material"].Unlocked = true;

            forest.Values["wandlevel"] = 0;
            forest.Values["boughtThings"] = 0;
            forest.Values["allowedGrowths"] = 2;

            forest.AddModifier(new Modifier("Base Stats", modifiersA: new Dictionary<string, double>() { { "HealthRegen", 0.2 } }));

            //Modifier som GrowthDruids can increase når de increaser xp gain af noget
            //Druidcraft XP: Income
            //Animal Handling XP: Damage givet
            //Soothing XP: Soothing udført
            forest.AddModifier(new Modifier("Xp Gain", new Dictionary<string, double>() { { "DruidcraftXpGain", 0.01 }, { "Animal HandlingXpGain", 0.00 } }));

            // BUSHES
            forest.Values["BushesGain"] = 0.6;
            forest.Values["BushesAttack"] = 0.2;
            forest.Values["BushesHealth"] = 0.2;
            forest.Values["BushesInc"] = 1.1;
            forest.Growths["Bushes"].Amount = 1;
            forest.Growths["Bushes"].Unlocked = true;
            forest.Growths["Bushes"].Description = "Adds 0.2 attack and 0.2 health each.";

            // OAKS
            forest.Values["OaksGain"] = 2;
            forest.Values["OaksHealth"] = 1;
            forest.Values["OaksInc"] = 1.1;
            forest.Growths["Oaks"].Unlocked = true;
            forest.Growths["Oaks"].Description = "Adds 1 health each.";

            // ANTS
            forest.Values["AntsGain"] = 3.6;
            forest.Values["AntsAttack"] = 0.8;
            forest.Values["AntsInc"] = 1.1;
            forest.Growths["Ants"].Description = "Adds 0.8 attack each. Gives 3xp in Animal handling.";

            // BIRCHES
            forest.Values["BirchesGain"] = 7;
            forest.Values["BirchesDefense"] = 0.25;
            forest.Values["BirchesInc"] = 3;
            forest.Growths["Birches"].Description = "Adds 0.25 defense each. Is a limited growth. Each birch costs 3x the last.";



            // YEWS
            forest.Values["YewsGain"] = 23;
            forest.Values["YewsHealth"] = 4;
            forest.Values["YewsAttack"] = 0;
            forest.Values["YewsInc"] = 1.1;
            forest.Growths["Birches"].Description = "Adds 4 health each.";

            // FLOWERS
            forest.Values["FlowersGain"] = 0;
            forest.Values["FlowersHealthRegen"] = 0.2;
            forest.Values["FlowersSoothing"] = 2;
            forest.Values["FlowersInc"] = 1.1;

            // SPIDERS
            forest.Values["SpidersGain"] = 85;
            forest.Values["SpidersAttack"] = 1;
            forest.AddModifier(new Modifier("SpiderAttackMod", new Dictionary<string, double>() { { "SpidersAttack", 1 } }));
            forest.Values["SpidersInc"] = 1.1;

            // DOABLES

            forest.Values["DruidHeal"] = 5;
            forest.Values["RejuvenateCooldownMod"] = 1; //Is a speed, making it higher will make it faster.
            forest.Doables["Rejuvenate"].Unlocked = true;
            forest.Doables["Rejuvenate"].Requirements += "RejuvenateCooldown_<=_0";


            // Harmony 
            forest.Values["HarmonyDuration"] = 15 * 5; // Ticks harmony lasts
            forest.Values["HarmonyCooldownTime"] = 60 * 5; // Ticks harmony cooldowns
            forest.Doables["Harmony"].Unlocked = false;
            forest.Doables["Harmony"].Requirements += "HarmonyCooldown_<=_0";

            // Rageuvenate
            forest.Values["RageuvenateCooldownTime"] = 30 * 5;
            forest.Doables["Rageuvenate"].Requirements += "RageuvenateCooldown_<=_0";
            forest.Values["RageuvenateDamageRounds"] = 2; // How many rounds of damage Rageuvenate deals
            forest.Values["RageuvenateIncome"] = 12 * 5; // How many ticks of income Rageuvenate gives

            // Web Site
            forest.Values["Web SiteMod"] = 0.2; // How much each magnitude of income increases the floating point modifier on spider attack.

            // Web Development & Webs
            forest.Values["WebsGain"] = 1E+4;
            forest.Values["WebsDefense"] = 5;
            forest.Values["WebsAttack"] = 5;
            forest.Values["WebsInc"] = 10;
            forest.AddModifier(new Modifier("Webs", modifiersA: new Dictionary<string, double>() { { "Stall", 0 }, { "SpidersAttack", 0 } }));

            // Testing doable that gives half an hour of passive generation.
            forest.Doables["Halfhour Offline"].Unlocked = true;

            // Think upgrades
            forest.Values["DruidcraftConsideredBonus"] = 0.01; //How much production boost in % each level of druidcraft gives.
            forest.Values["Handled You BeforeBonus"] = 0.01; //How much attack boost in % each level of Animal Handling gives.
            forest.Values["Soothing ThoughtsBonus"] = 0.01; //How much attack boost in % each level of Animal Handling gives.

        }
    }
}
