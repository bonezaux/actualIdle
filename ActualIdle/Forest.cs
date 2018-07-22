﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Xml.Linq;

namespace ActualIdle {

    public class Forest : Fighter {
        
        /// <summary>
        /// The Growths available to the druid.
        /// </summary>
        public Dictionary<string, Entity> Entities { get; private set; }
        public Dictionary<string, Doable> Doables { get; private set; }
        public Dictionary<string, Trophy> Trophies { get; private set; }
        public Dictionary<string, Upgrade> Upgrades { get; private set; }


        /// <summary>
        /// No Values can start with 'sv', then they are known as soft values.
        /// </summary>
        public Dictionary<string, double> Values { get; private set; }
        /// <summary>
        /// Values that are kept over a soft reset; Standard values are just discarded. Must all be prefixed with 'sv';
        /// </summary>
        public Dictionary<string, double> SoftValues { get; private set; }
        /// <summary>
        /// The Druids xp in all skills specified in the Statics statlist. Levels are at specific intervals, which will probably be changed at some point.
        /// </summary>
        public new Dictionary<string, double> Xp { get; private set; }
        public Dictionary<string, Modifier> Modifiers { get; private set; }
        public Fighter Boss { get; private set; }
        public bool Running { get; private set; }
        /// <summary>
        /// The Forest's current path.
        /// </summary>
        public Path CurPath { get; private set; }
        /// <summary>
        /// The current boss number on the path. Starts at zero
        /// </summary>
        public int CurBoss { get; private set; }

        /// <summary>
        /// Controls whether the Druid is currently fighting a boss.
        /// </summary>
        public bool Fighting { get; set; }
        /// <summary>
        /// How much the druid has soothed in the current battle.
        /// </summary>
        public double Soothe { get; set; }
        /// <summary>
        /// The Hp in the last tick of combat. If this is lower than current health three combat ticks in a row, the fight is won.
        /// </summary>
        public double LastHp { get; set; }


        public int HpIncreaseRounds { get; set; }
        /// <summary>
        /// What calculation step we're currently at. Every 5th is a second
        /// </summary>
        public int Count { get; private set; }
        /// <summary>
        /// How many offline ticks have gone by.
        /// </summary>
        public int OfflineTicks { get; private set; }
        public double Income { get; set; }
        public double Mana;

        public Forest() : base(0, 0, 0, "Druid", null, null, "!Btrue") {
            Entities = new Dictionary<string, Entity>();
            Doables = new Dictionary<string, Doable>();
            Trophies = new Dictionary<string, Trophy>();
            Upgrades = new Dictionary<string, Upgrade>();
            Values = new Dictionary<string, double>();
            SoftValues = new Dictionary<string, double>();
            Xp = new Dictionary<string, double>();
            Modifiers = new Dictionary<string, Modifier>();
            Modifiers.Add(E.XP+E.GAIN, new Modifier(E.XP+E.GAIN, modifiersF: new Dictionary<string, double>() { { E.XP + E.GAIN, 1 } }));
            foreach (string skill in Statics.skills) {
                Xp[skill] = 100;
                Values[skill+E.XP+E.GAIN] = 1;
                Modifiers[E.XP+E.GAIN].ModifiersF[skill+ E.XP + E.GAIN] = 0;
            }

            Running = true;
            Fighting = false;
            Values[E.DEFEATED_BOSSES] = 0;
        }

        /// <summary>
        /// Returns a dictionary of the Druids stats in all the stats specified in Statics.statList
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, double> GetStats() {
            Dictionary<string, double> result = new Dictionary<string, double>();
            foreach (string stat in Statics.statList) {
                result[stat] = 0;
            }

            foreach (KeyValuePair<string, Entity> entry in Entities) {
                Dictionary<string, double> thingStats = Entities[entry.Key].GetStats();
                result = Statics.AddDictionaries(result, thingStats);
            }
            result = (from e in result
                      select new { Name = e.Key, Count = Modifier.Modify(Modifiers.Values, e.Key, e.Value) }).ToDictionary(item => item.Name, item => item.Count);
            return result;
        }

        public override void Lose() {
            Console.WriteLine("You were defeated! Boss hp: " + Statics.GetDisplayNumber(Boss.Hp) + "/" + Statics.GetDisplayNumber(Boss.Stats[E.HEALTH]));
            Boss.Hp = Boss.Stats[E.HEALTH];
            Fighting = false;
            Hp = 0;
            Soothe = 0;
        }

        public void SpendMana(double mana) {
            Mana -= mana;
        }

        public bool HasModifier(Modifier modifier) => Modifiers.ContainsKey(modifier.Name);

        public override double takeDamage(double damage, Fighter attacker, bool armor=true) {
            if(Soothe > 0) {
                damage -= (Soothe / 30);
            }
            return base.takeDamage(damage, attacker, armor);
        }

        public void Loop() {
            double preGold = Entities["Organic Material"].Amount;
            foreach (KeyValuePair<string, Entity> entry in Entities) {
                entry.Value.Loop();
            }
            foreach (KeyValuePair<string, Trophy> entry in Trophies) {
                entry.Value.Loop();
            }
            foreach (KeyValuePair<string, Upgrade> entry in Upgrades) {
                entry.Value.Loop();
            }
            foreach (KeyValuePair<string, Doable> entry in Doables) {
                entry.Value.Loop();
            }
            Income = Entities["Organic Material"].Amount - preGold;
            AddXp("Druidcraft", Income);

            if (Fighting && Count % 5 == 0) {
                FightTick();
            }
            
            Stats = GetStats();
            Hp += Stats[E.HEALTHREGEN] / 5.0;
            Mana += Stats[E.MANAREGEN] / 5.0;
            if (Hp > Stats[E.HEALTH]) {
                Hp = Stats[E.HEALTH];
            }
            if(Mana > Stats[E.MAXMANA]) {
                Mana = Stats[E.MAXMANA];
            }
        }

        /// <summary>
        /// Called when the battle against the current boss is won. Can be used to insta-win a boss battle too, if necessary.
        /// winType 0 = defeated
        /// winType 1 = soothed
        /// </summary>
        public void WinBattle(int winType = 0) {
            Boss.Lose();
            if (winType == 0)
                Console.WriteLine("You defeated " + Boss.Name);
            else if(winType == 1) {
                Console.WriteLine(Boss.Name + " realized that this is not a world for fighting, but for loving.");
            }
            Soothe = 0;
            Values["Defeated" + Boss.Name] = 1;
            Values["DefeatedBosses"] += 1;
            Values["allowedGrowths"] += Boss.AddedGrowths;
            Boss = null;
            Fighting = false;
            CurBoss++;
            Console.WriteLine("CB:" + CurBoss);
            if (CurBoss >= CurPath.Length()) {
                Console.WriteLine("You're through this path now..");
            } else {
                Boss = CurPath.Bosses[CurBoss].Clone();
                Console.WriteLine("Changing boss:" + Boss);
            }
        }

        public void FightTick(bool forestAttack=true, bool bossAttack=true) {
            if(bossAttack) {
                if (Boss.Hesitation <= 0)
                    Boss.FightLoop(this);
                else
                    Boss.Hesitation--;
            }
            if (LastHp < Hp && LastHp > -1) {
                HpIncreaseRounds++;
                if (HpIncreaseRounds >= 3) {
                    WinBattle(1);
                }
            } else {
                HpIncreaseRounds = 0;
            }
            if (Fighting && forestAttack) { // Only damage the boss if the boss didn't kill the Druid first.
                if(Hesitation <= 0) {
                    double damage = (Stats[E.ATTACK]);
                    if (damage > 0) {
                        damage = Boss.takeDamage(damage, this);
                        AddXp(E.ANIMAL_HANDLING, damage);
                    }
                    if (OwnsUpgrade("Become Soother")) {
                        Soothe += Stats[E.SOOTHING];
                        AddXp("Soothing", Stats[E.SOOTHING]);
                    } else {
                        Soothe += Stats[E.SOOTHING] / 20;
                    }
                    if (Boss.Hp <= 0) {
                        WinBattle();
                    }
                } else {
                    Hesitation--;
                }
            }
            LastHp = Hp;
        }

        /// <summary>
        /// Sets the currently traveled path.
        /// </summary>
        /// <param name="path"></param>
        public void SetPath(Path path) {
            CurPath = path;
            CurBoss = 0;
            Boss = CurPath.Bosses[CurBoss].Clone();
        }

        public void StartFighting() {
            if (Fighting)
                return;
            if(Boss == null) {
                Console.WriteLine("You don't have a next boss to fight right now.");
                return;
            }
            Console.WriteLine();
            Console.WriteLine("You are now fighting " + Boss.Name + "!");
            Soothe = 0;
            HpIncreaseRounds = 0;
            LastHp = -1;
            double fSpdWin = Stats[E.SPEED] - Boss.Stats[E.STALL];
            double bSpdWin = Boss.Stats[E.SPEED] - Stats[E.STALL];
            if(fSpdWin >= 1) {
                int win = (int)fSpdWin;
                for(int loop=0;loop<win;loop++) {
                    FightTick(true, false);
                }
            } else if(fSpdWin <= -1) {
                int loss = (int)-fSpdWin;
                Hesitation = loss;
            }
            if (bSpdWin >= 1) {
                int win = (int)bSpdWin;
                for (int loop = 0; loop < win; loop++) {
                    FightTick(false, true);
                }
            } else if (bSpdWin <= -1) {
                int loss = (int)-bSpdWin;
                Boss.Hesitation = loss;
            }

            Fighting = true;
        }

        public void AddObject(Entity obj) {
            Entities.Add(obj.Name, obj);
        }

        public void AddDoable(Doable doable) {
            Doables.Add(doable.Name, doable);
        }

        public void AddTrophy(Trophy trophy) {
            Trophies.Add(trophy.Name, trophy);
        }

        public void AddUpgrade(Upgrade upgrade) {
            Upgrades.Add(upgrade.Name, upgrade);
        }

        /// <summary>
        /// Adds xp based on a change in the value that the skill uses for xp.
        /// change is this change (The change in the virtual total that the xp is equal to a sixth of the magnitude times 100 to.)
        /// </summary>
        /// <param name="skillName"></param>
        /// <param name="change"></param>
        public void AddXp(string skillName, double change) {
            double xp = Statics.XpGain(Xp[skillName], change, skillName) * GetValue(skillName+"XpGain");
            if (Statics.skills.Contains(skillName)) {
                int preLevel = (int)GetValue("lvl" + skillName);
                Xp[skillName] += xp;
                int postLevel = (int)GetValue("lvl" + skillName);
                if(postLevel > preLevel) {
                    Console.WriteLine("Level up! " + skillName + " " + preLevel + "->" + postLevel);
                }
            }
        }

        public void AddModifier(Modifier modifier) {
            if (Modifiers.ContainsKey(modifier.Name))
                Modifiers[modifier.Name] = modifier;
            else
                Modifiers.Add(modifier.Name, modifier);
        }

        public Modifier GetModifier(string name) {
            if (Modifiers.ContainsKey(name))
                return Modifiers[name];
            return null;
        }

        public void RemoveModifier(string modifier) {
            Modifiers.Remove(modifier);
        }

        public void DealDamage(double damage, bool armor = true) {
            damage = Boss.takeDamage(damage, this, armor);
            AddXp("Animal Handling", damage);
        }

        /// <summary>
        /// Buys a number of ForestObjects. 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="amount"></param>
        public void BuyObject(string obj, int amount, bool percentage=false) {
            Entities[obj].Create(amount, percentage);
        }

        /// <summary>
        /// Echoes the stats of the next boss. Maybe this should also echo like boss options or something like that.
        /// </summary>
        public void EchoBoss() {
            Console.WriteLine("Boss:");
            if(Boss == null) {
                Console.WriteLine("There is currently no boss.");
                return;
            }
            Console.WriteLine(Boss.Name);
            Console.WriteLine("Hp: " + Math.Round(Boss.Hp, 2) + ", defense: " + Math.Round(Boss.Stats[E.DEFENSE], 2));
            Console.WriteLine("Attack: " + Math.Round(Boss.Stats[E.ATTACK]-Soothe/75, 2));
            int stall = (int)Boss.Stats[E.STALL];
            int speed = (int)Boss.Stats[E.SPEED];
            if(stall != 0)
                Console.WriteLine("Stall: " + stall);
            if (speed != 0)
                Console.WriteLine("Speed: " + speed);
            if (Program.debug)
                Console.WriteLine("DEBUG VALUE (hp*(attack-f.def)): " + Statics.GetDisplayNumber(Boss.Hp * (Boss.Stats[E.ATTACK] - Boss.Stats[E.DEFENSE])));
            if(Boss.Description != null)
                Console.WriteLine(Boss.Description);
        }

        public void TickOffline(int ticks) {
            for(int i=0;i<ticks;i++)
                Loop();
            OfflineTicks += ticks;
        }

        public void ListGrowths(string group=E.GRP_FOREST) {
            int i = 0;
            Console.WriteLine();
            foreach (KeyValuePair<string, Entity> entry in Entities) {
                if (entry.Value.Unlocked && entry.Value.Group == group) {
                    entry.Value.Echo();
                    i++;
                    if (i == 4) {
                        i = 0;
                        Console.WriteLine();
                    }
                }
            }
        }

        public void ListPrices() {
            foreach (KeyValuePair<string, Entity> entry in Entities) {
                if (Entities[entry.Key].Unlocked) {
                    Entities[entry.Key].EchoPrice();
                }
            }
        }

        public void ListDoables() {
            Console.WriteLine("You can: ");
            foreach (KeyValuePair<string, Doable> entry in Doables) {
                if (entry.Value.Unlocked)
                    Console.WriteLine("\t" + entry.Value.GetTooltip());
            }
        }

        public void ListSkills() {
            foreach (string skill in Statics.skills) {
                if (Xp[skill] < 101)
                    continue;
                int lvl = (int)GetValue("lvl" + skill);
                double nextXp = Math.Pow(1.2, lvl + 1) * 100;
                Console.WriteLine(skill + "\tlvl " + GetValue("lvl" + skill) + "\t" + Math.Round(Xp[skill], 2) + "/ " + nextXp + " xp");
            }
        }

        public void ListAvailableUpgrades() {
            string owned = "";
            string unowned = "";
            foreach (KeyValuePair<string, Upgrade> entry in Upgrades) {
                if(entry.Value.Unlocked) {
                    if (entry.Value.Owned)
                        owned += ", " + entry.Key;
                    else
                        unowned += ", " + entry.Key;
                }
            }
            
            Console.WriteLine(" --- Available Upgrades --- ");
            if(unowned.Length > 0) {
                Console.WriteLine(unowned.Substring(2));
            } else {
                Console.WriteLine("N/A");
            }
                Console.WriteLine(" ----- Owned Upgrades ----- ");
            if (owned.Length > 0) {
                Console.WriteLine(owned.Substring(2));
            } else {
                Console.WriteLine("N/A");
            }
        }

        /// <summary>
        /// Performs a doable, if it exists and is unlocked.
        /// </summary>
        /// <param name="doable"></param>
        public void PerformDoable(string doable) {
            if (Doables.ContainsKey(doable) && Doables[doable].Unlocked)
                Doables[doable].Perform();
            else
                Console.WriteLine("No doable with the name " + doable + " is unlocked");
        }

        public void PickPath() {
            if(CurBoss < CurPath.Bosses.Count) {
                Console.WriteLine("You are not at the end of a path right now.");
                return;
            } else if(CurPath.EndBranch == null) {
                Console.WriteLine("There's no branch from here..");
                return;
            }
            Path nextPath = CurPath.EndBranch.PickPath();
            if (nextPath != null) {
                SetPath(nextPath);
            }
        }

        public void EchoPath() {
            if(CurPath != null) {
                CurPath.Echo();
            } else {
                Console.WriteLine("You're not currently on a path.");
            }
        }

        public void ListModifiers() {
            Console.WriteLine(" --- Modifiers ---");
            foreach(Modifier modifier in Modifiers.Values) {
                modifier.Echo();
            }
        }

        /// <summary>
        /// Returns a value after being modified. This may be the start of some kind of mini bytecode.
        /// The given value string may sometime become the basis of this bytecode.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public double GetValue(string value) {
            if (value.StartsWith("lvl")) {
                if (Statics.skills.Contains(value.Substring(3)))
                    return (int)Math.Log(Xp[value.Substring(3)] / 100, 1.2);
            } else if (value.StartsWith("count")) {
                if (Entities.ContainsKey(value.Substring(5)))
                    return Entities[value.Substring(5)].Amount;
            } else if (value.StartsWith("stat")) {
                if (Stats.ContainsKey(value.Substring(4)))
                    return Stats[value.Substring(4)];
            } else if (value.Equals("mana")) {
                return Mana;
            } else if (value.Equals("hp")) {
                return Hp;
            } else if (value.StartsWith("!I")) {
                return int.Parse(value.Substring(2));
            } else if (value.StartsWith("!F")) {
                return float.Parse(value.Substring(2));
            } else if (value.StartsWith("!B")) {
                return bool.Parse(value.Substring(2)) ? 1 : 0;
            } else if (value.StartsWith("sv")) {
                if(SoftValues.ContainsKey(value))
                    return Modifier.Modify(Modifiers.Values, value, SoftValues[value]);
            } else if (Values.ContainsKey(value)) {
                return Modifier.Modify(Modifiers.Values, value, Values[value]);
            }

            return 0;

        }

        /// <summary>
        /// Changes a value by the given amount, or creates that value, if it doesn't exist already.
        /// If prefixed with 'sv', the value change happens in the soft values.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="change"></param>
        public void ChangeValue(string value, double change) {
            if(value.StartsWith("sv")) {
                if (SoftValues.ContainsKey(value))
                    SoftValues[value] += change;
                else
                    SoftValues[value] = change;
            } else if (Values.ContainsKey(value))
                Values[value] += change;
            else
                Values[value] = change;
        }

        /// <summary>
        /// Tests whether a giiven string requirement holds true. Used when Lambda expressions are overkill.
        /// Returns the answer.
        /// </summary>
        /// <param name="requirement"></param>
        public bool TestRequirement(string requirement) {
            double value1 = GetValue(requirement.Split('_')[0]);
            string compare = requirement.Split('_')[1];
            double value2 = GetValue(requirement.Split('_')[2]);

            if (compare == ">=")
                return value1 >= value2;
            else if (compare == "<=")
                return value1 <= value2;
            else if (compare == ">")
                return value1 > value2;
            else if (compare == "<")
                return value1 < value2;
            else if (compare == "==")
                return value1 == value2;
            else if (compare == "!=")
                return value1 != value2;
            else {
                Console.WriteLine("Illegal comparator " + compare);
                return false;
            }
        }

        public bool OwnsUpgrade(string upgrade) {
            return GetValue("Upgrade" + upgrade + "Bought") > 0;
        }

        /// <summary>
        /// Runs TestRequirement for every line in the string. Returns true if they're all true, otherwise false.
        /// </summary>
        /// <param name="requirements"></param>
        /// <returns></returns>
        public bool TestRequirements(string requirements) {
            if (requirements == "")
                return true;
            string[] requirementList = requirements.Split('\n');
            foreach(string requirement in requirementList) {
                if (!TestRequirement(requirement)) {
                    return false;
                }
            }
            return true;
        }

        public void Calculation() {
            Count = 0;
            while (Running) {
                Thread.Sleep((Program.debug ? Program.debugCountTime : 200));
                Count++;
                Loop();
            }
            Console.WriteLine("Terminating");
        }

        /// <summary>
        /// Saves the forest to an XML file. 
        /// </summary>
        public void Save(string filename = null) {
            Console.WriteLine("Saving...");
            XElement element = new XElement("Forest");

            // Saves values 
            XElement valuesElement = XMLUtils.CreateElement(element, "Values");
            foreach (KeyValuePair<string, double> entry in Values) {
                if(entry.Value != 0)
                    XMLUtils.CreateElement(valuesElement, entry.Key, entry.Value);
            }
            // Saves soft values
            foreach (KeyValuePair<string, double> entry in SoftValues) {
                if (entry.Value != 0)
                    XMLUtils.CreateElement(valuesElement, entry.Key, entry.Value);
            }

            // Saves skills
            XElement skillsElement = XMLUtils.CreateElement(element, "skills");
            foreach(string skill in Statics.skills) {
                XMLUtils.CreateElement(skillsElement, skill, Xp[skill]);
            }

            // Saves growths 
            XElement growthsElement = XMLUtils.CreateElement(element, "Growths");
            foreach(Entity g in Entities.Values) {
                XElement growthElement = XMLUtils.CreateElement(growthsElement, g.Name);
                g.Save(growthElement);
            }

            // Saves trophies
            XElement trophiesElement = XMLUtils.CreateElement(element, "Trophies");
            foreach(Trophy t in Trophies.Values) {
                XElement trophyElement = XMLUtils.CreateElement(trophiesElement, t.Name);
                t.Save(trophyElement);
            }

            // Saves doables
            XElement doablesElement = XMLUtils.CreateElement(element, "Doables");
            foreach (Doable d in Doables.Values) {
                XElement doableElement = XMLUtils.CreateElement(doablesElement, d.Name);
                d.Save(doableElement);
            }

            XElement modifiersElement = XMLUtils.CreateElement(element, "Modifiers");
            foreach(Modifier m in Modifiers.Values) {
                XElement modifierElement = XMLUtils.CreateElement(modifiersElement, m.Name);
                m.Save(modifierElement);
            }

            XElement upgradeElement = XMLUtils.CreateElement(element, "Upgrades");
            foreach(Upgrade u in Upgrades.Values) {
                XMLUtils.CreateElement(upgradeElement, u.Name, u.Owned);
            }
            
            XMLUtils.CreateElement(element, "Path", CurPath.Name);
            XMLUtils.CreateElement(element, "PathBoss", CurBoss);
            XMLUtils.CreateElement(element, "Count", Count);
            XMLUtils.CreateElement(element, "OfflineTicks", OfflineTicks);
            XMLUtils.CreateElement(element, "Hp", Hp);
            XMLUtils.CreateElement(element, "Mana", Mana);

            System.IO.Directory.CreateDirectory("saves");
            XDocument xd = new XDocument();
            xd.Add(element);
            if(filename == null) {
                if (System.IO.File.Exists("saves/save.xml")) {
                    for (int loop = 10; loop > 2; loop--) {
                        if (System.IO.File.Exists("saves/save" + (loop - 1) + ".xml")) {
                            System.IO.File.Copy("saves/save" + (loop - 1) + ".xml", "saves/save" + loop + ".xml", true);
                        }
                    }
                    System.IO.File.Copy("saves/save.xml", "saves/save2.xml", true);
                }
                xd.Save("saves/save.xml");
            } else {
                xd.Save("saves/"+filename + ".xml");
            }
        }

        public void Load(string filename = null) {
            Console.WriteLine("Loading...");
            XDocument xd = null;

            if (filename == null)
                xd = XDocument.Load(@"saves/save.xml");
            else {
                xd = XDocument.Load(@"saves/"+filename+".xml");
            }
            XElement element = xd.Element("Forest");
            
            // Loads values and soft values 
            XElement valuesElement = XMLUtils.GetElement(element, "Values");
            foreach (XElement valueElement in valuesElement.Elements()) {
                string name = XMLUtils.GetName(valueElement);
                if(name.StartsWith("sv")) //Soft value
                    SoftValues[name] = double.Parse(valueElement.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
                else
                    Values[XMLUtils.GetName(valueElement)] = double.Parse(valueElement.Value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }

            // Loads skills 
            XElement skillsElement = XMLUtils.GetElement(element, "skills");
            foreach (string skill in Statics.skills) {
                if (XMLUtils.HasChild(skillsElement, skill))
                    Xp[skill] = XMLUtils.GetDouble(skillsElement, skill);
            }

            // Loads growths 
            XElement growthsElement = XMLUtils.GetElement(element, "Growths");
            foreach (Entity g in Entities.Values) {
                XElement growthElement = XMLUtils.GetElement(growthsElement, g.Name);
                if(growthElement != null)
                    g.Load(growthElement);
            }

            // Loads trophies 
            XElement trophiesElement = XMLUtils.GetElement(element, "Trophies");
            foreach (Trophy t in Trophies.Values) {
                XElement trophyElement = XMLUtils.GetElement(trophiesElement, t.Name);
                if(trophyElement != null)
                    t.Load(trophyElement);
            }

            // Loads doables 
            XElement doablesElement = XMLUtils.GetElement(element, "Doables");
            foreach (Doable d in Doables.Values) {
                XElement doableElement = XMLUtils.GetElement(doablesElement, d.Name);
                if(doableElement != null)
                    d.Load(doableElement);
            }

            // Loads modifiers TODO: if anything needs modifiers to stay past loads, do something about it then
            Modifiers.Clear();
            XElement modifiersElement = XMLUtils.GetElement(element, "Modifiers");
            foreach (XElement modifierElement in modifiersElement.Elements()) {
                Modifier m = new Modifier(XMLUtils.GetName(modifierElement));
                m.Load(modifierElement);
                Modifiers[m.Name] = m;
            }

            // Loads upgrades
            XElement upgradesElement = XMLUtils.GetElement(element, "Upgrades");
            foreach (Upgrade u in Upgrades.Values) {
                XElement upgradeElement = XMLUtils.GetElement(upgradesElement, u.Name);
                if(upgradeElement != null) {
                    u.Owned = bool.Parse(upgradeElement.Value);
                    if(u.Modifier != null && u.Owned) {
                        AddModifier(u.Modifier);
                    }
                }
            }

            string path = element.GetString("Path");
            CurBoss = element.GetInt("PathBoss");
            foreach(Path p in Path.paths) {
                if(p.Name == path) {
                    CurPath = p;
                    Boss = p.Bosses[CurBoss];
                    break;
                }
            }

            Count = element.GetInt("Count");
            Hp = element.GetDouble("Hp");
            if(element.HasChild("Mana"))
                Mana = element.GetDouble("Mana");
            OfflineTicks = element.GetInt("OfflineTicks");

        }

        /// <summary>
        /// Adds an amount of an entity to the Forest, and unlocks it if it's not unlocked (You could just add zero if you just want to unlock).
        /// </summary>
        /// <param name="name">name of entity</param>
        /// <param name="amount">How many to add</param>
        public void AddItem(string name, int amount) {
            Entities[name].OnAdd(amount);
            Entities[name].Amount += amount;
            if (!Entities[name].Unlocked)
                Entities[name].Unlocked = true;
        }
    }
}
