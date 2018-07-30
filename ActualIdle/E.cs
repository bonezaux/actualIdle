using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {
    public static class E {
        //ILLEGAL CHARACTERS:
        // Starting with numbers
        // Parentheses
        public const string ORGANIC_MATERIAL = "Organic Material";
        /// <summary>
        /// Key for how many of the limited growths are owned.
        /// </summary>
        public const string BOUGHT_THINGS = "boughtThings";
        /// <summary>
        /// Key for how many of the limitied growths are allowed currently.
        /// </summary>
        public const string ALLOWED_GROWTHS = "allowedGrowths";
        public const string GAIN = "Gain";
        public const string ATTACK = "Attack";
        public const string HEALTH = "Health";
        public const string HEALTHREGEN = "HealthRegen";
        public const string DEFENSE = "Defense";
        /// <summary>
        /// Skill in soothing.
        /// </summary>
        public const string SOOTHING = "Soothing";
        /// <summary>
        /// Postfix for how much the price of an Entity is multiplied by for each bought
        /// </summary>
        public const string INC = "Inc";
        public const string MOD = "Mod";
        public const string COOLDOWN = "Cooldown";
        /// <summary>
        /// Used for durations too
        /// </summary>
        public const string TIME = "Time";
        public const string DAMAGE = "Damage";
        public const string DAMAGE_TAKEN = "Damage Taken";
        public const string INCOME = "Income";
        public const string MAXMANA = "Max Mana";
        public const string MANAREGEN = "Mana Regen";
        public const string MANA = "Mana";
        public const string SPEED = "Speed";
        public const string STALL = "Stall";
        public const string DRUIDCRAFT = "Druidcraft";
        public const string ANIMAL_HANDLING = "Animal Handling";
        /// <summary>
        /// Key to amount of defeated bosses.
        /// </summary>
        public const string DEFEATED_BOSSES = "DefeatedBosses";
        public const string ACTIVE = "Active";
        /// <summary>
        /// Prefix for getting the count of things in GetValue.
        /// </summary>
        public const string COUNT = "count";
        public const string SOOTHER = "Soother";
        /// <summary>
        /// [skill] + XP + GAIN is the modifier for xp gain.
        /// </summary>
        public const string XP = "Xp";
        public const string KILLS = "Kills";
        /// <summary>
        /// Prefix for getting level in a skill in GetValue. LEVEL + [skill name]
        /// </summary>
        public const string LEVEL = "lvl";

        // ABILITIES BELOW
        public const string ABIL_RAGEUVENATE = "Rageuvenate";
        public const string ABIL_REJUVENATE = "Rejuvenate";
        public const string ABIL_HARMONY = "Harmony";
        public const string ABIL_SURF_THE_WEB = "Surf the Web";

        // FOREST ENTITIES BELOW
        public const string ENTITY_BUSHES = "Bushes";
        public const string ENTITY_OAKS = "Oaks";
        public const string ENTITY_ANTS = "Ants";
        public const string ENTITY_BIRCHES = "Birches";
        public const string ENTITY_YEWS = "Yews";
        public const string ENTITY_FLOWERS = "Flowers";
        public const string ENTITY_SPIDERS = "Spiders";
        public const string ENTITY_WEBS = "Webs";

        /// <summary>
        /// Refers to Oaks, Birches and Yews.
        /// </summary>
        public const string TREES = "Trees";
        /// <summary>
        /// Refers to Ants and Spiders
        /// </summary>
        public const string ANIMALS = "Animals";

        // TRIGGERS BELOW
        /// <summary>
        /// Key called when a boss is defeated. args[0]=boss name
        /// </summary>
        public const string TRG_DEFEATED_BOSS = "Defeated Boss";
        /// <summary>
        /// Key called at the end of a think.
        /// </summary>
        public const string TRG_THINK_COMPLETED = "Think Completed";
        /// <summary>
        /// Key called when OnAdd of an Entity is called. args[0] = name, args[1] = amount added.
        /// </summary>
        public const string TRG_ENTITY_ADDED = "Entity Added";
        /// <summary>
        /// Key called when the druid levels up. args[0] = skill, args[1] = level before level up, args[2] = level after level up.
        /// </summary>
        public const string TRG_LEVEL_UP = "Level Up";

        // ENTITY EXTENSIONS BELOW
        public const string EEXT_BUYABLE = "Buyable";
        public const string EEXT_GENERATE = "Generate";
        public const string EEXT_MODIFIER = "Modifier";
        public const string EEXT_XPMOD = "XpMod";
        public const string EEXT_LIMITED = "Limited";
        public const string EEXT_REQUIREMENTS = "Requirements";

        // UPGRADES BELOW
        public const string UPG_WEB_SITE = "Web Site";
        public const string UPG_WEB_DEVELOPMENT = "Web Development";
        public const string UPG_DRUIDCRAFT_CONSIDERED = "Druidcraft Considered";
        public const string UPG_HANDLED_YOU_BEFORE = "Handled You Before";
        public const string UPG_SOOTHING_THOUGHTS = "Soothing Thoughts";
        public const string UPG_BIG_BIRCHES = "Big Birches";
        public const string UPG_SPIDERFRIENDS = "Spiderfriends";
        public const string UPG_SOOTHING_REJUVENATION = "Soothing Rejuvenation";
        public const string UPG_FREE_HEALTHCARE = "Free Healthcare";
        public const string UPG_UNLOCK_HARMONY = "Unlock Harmony";
        public const string UPG_TRANSMOGRIFY_RAGEUVENATE = "Transmogrify Rageuvenate";
        public const string UPG_BUSH_GROWTH = "Bush Growth";
        public const string UPG_OAK_GROWTH = "Oak Growth";
        public const string UPG_BIRCH_GROWTH = "Growing Birches";
        public const string UPG_ANT_GROWTH = "Larger Ants";
        public const string UPG_YEW_GROWTH = "Gnarly Yews";
        public const string UPG_SPIDER_GROWTH = "Spiderbefriending";
        public const string UPG_FLOWER_GROWTH = "Blooming Flowers";
        public const string UPG_BECOME_SOOTHER = "Become Soother";
        public const string UPG_DENSE_FOREST = "Dense Forest";
        public const string UPG_UNLOCK_SURF_THE_WEB = "Unlock Surf the Web";
        public const string UPG_SPIDER_PRESTIGE = "Spider Prestige";
        /// <summary>
        /// Postfix for an upgrade, with prefix UPGRADE together key for whether the upgrade is bought.
        /// > 0 = bought, 0 = not bought.
        /// </summary>
        public const string BOUGHT = "Bought";
        /// <summary>
        /// See BOUGHT
        /// </summary>
        public const string UPGRADE = "Upgrade";

        // ENTITY GROUPS BELOW
        public const string GRP_FOREST = "Forest";
        public const string GRP_ITEMS = "Items";
        public const string GRP_UPGRADES = "Upgrades";
        public const string GRP_TROPHIES = "Trophies";
        public const string GRP_TALENTS = "Talents";

        // TROPHIES BELOW
        public const string TROPHY_SOOTHED_20TH_BOSS = "Soothed 20th Boss";
        public const string TROPHY_WEBBED_20TH_BOSS = "Webbed 20th Boss";
        public const string TROPHY_250_BUSHES = "Acquired 250 Bushes";

        //SOFT VALUES BELOW
        /// <summary>
        /// Amount of soft resets, soft value.
        /// </summary>
        public const string SV_THINKS = "svThinks";
        /// <summary>
        /// The prefix for the value that gives the highest level that a skill has been
        /// </summary>
        public const string SV_LEVEL = "svlvl";
        /// <summary>
        /// The prefix for the value that gives the amount of a growth at last think.
        /// </summary>
        public const string SV_COUNT = "svcount";
        /// <summary>
        /// The prefix for the value that gives the highest SV_COUNT has been. 
        /// </summary>
        public const string SV_MAX_COUNT = "svmaxcount";
        /// <summary>
        /// Prefix for soft values for other things that happen to 
        /// </summary>
        public const string SV = "sv";

        // INJECT KEYS BELOW
        /// <summary>
        /// Called in OnAdd. args[0] = amount
        /// Also called in Reapply for Trophies.
        /// </summary>
        public const string INJ_ACQUIRE = "Acquire";
        public const string INJ_REQUIREMENTS = "Requirements";
        public const string INJ_TOOLTIP = "Tooltip";
        public const string INJ_RESET = "Reset";
        public const string INJ_CREATE = "Create";

        // FIGHTING STYLES BELOW
        public const string STYLE_FIGHT = "Fight";
        public const string STYLE_SOOTHE = "Soothe";
        /// <summary>
        /// Key for the value that says whether fighting style has been changed yet this think.
        /// </summary>
        public const string CHANGED_STYLE = "ChangedStyle";

        // ITEMS BELOW
        /// <summary>
        /// Item found on the avg hp path, final boss. Used by Soothers later to create a Sapling of Life.
        /// </summary>
        public const string ITEM_CORRUPT_LIFE_SEED = "Corrupt Seed of Life";
        /// <summary>
        /// A heavy rock. Gives +10% health, +25% defense.
        /// </summary>
        public const string ITEM_HEAVY_ROCK = "Heavy Rock";
        /// <summary>
        /// Tasty seeds. +50% Animal Handling xp gain, +20% bird gain.
        /// </summary>
        public const string ITEM_TASTY_SEEDS = "Tasty Seeds";
        /// <summary>
        /// A torn journal. +40% Druidcraft xp, +20% soothing.
        /// </summary>
        public const string ITEM_TORN_JOURNAl = "Torn Journal";

        // TALENTS BELOW
        public const string TALENT_DC_CLEAR_SKIES = "Clear Skies";
        public const string TALENT_DC_IRONBARK = "Ironbark";
        public const string TALENT_AH_WEBS = "Recreate Webs";

        // LOOT TABLES BELOW
        public const string LOOT_RANDOM = "Random Loot";
    }
}
