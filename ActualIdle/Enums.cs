using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {
    public static class E {
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
        public const string XP = "Xp";

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

        // ENTITY EXTENSIONS BELOW
        public const string EEXT_BUYABLE = "Buyable";
        public const string EEXT_GENERATE = "Generate";
        public const string EEXT_MODIFIER = "Modifier";
        public const string EEXT_XPMOD = "XpMod";
        public const string EEXT_LIMITED = "Limited";

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

        // GROUPS BELOW
        public const string GRP_FOREST = "Forest";
        public const string GRP_ITEMS = "Items";
        
        /// <summary>
        /// Amount of soft resets, soft value.
        /// </summary>
        public const string THINKS = "svThinks";

        // INJECT KEYS BELOW
        public const string INJ_ACQUIRE = "Acquire";
        public const string INJ_REQUIREMENTS = "Requirements";
        public const string INJ_TOOLTIP = "Tooltip";
    }
}
