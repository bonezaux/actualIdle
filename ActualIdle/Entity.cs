using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ActualIdle.Entity_Extensions;

namespace ActualIdle  {
    /// <summary>
    /// An object in the forest, like oaks or bushes. Produce other things by a formula based on how many of them there are, and give stats.
    /// </summary>


    /// <summary>
    /// An object in the forest, like oaks or bushes. Produce other things by a formula based on how many of them there are, and give stats.
    /// </summary>
    public class Entity : IPerformer {
        private bool _unlocked;

        public string Name { get; set; }
        public Forest Forest { get; set; }
        public string Description { get; set; }
        public string Group { get; set; }
        /// <summary>
        /// Which strength of reset is required to reset the amount of this entity.
        /// 0 = soft
        /// 1 = medium
        /// 2 = hard
        /// 3 = immutible
        /// </summary>
        public int Strength { get; set; }

        // THESE ARE RUNTIME VARIABLES
        /// <summary>
        /// Whether the growth is unlocked currently.
        /// </summary>
        public bool Unlocked { get => _unlocked; set {
                if(value != _unlocked) {
                    if (value)
                        OnEnable();
                    else
                        OnDisable();
                }
                _unlocked = value;
            }
        }
        /// <summary>
        /// How many of the growth are currently owned.
        /// </summary>
        public double Amount { get; set; }


        /// <summary>
        /// Code injects:
        ///  loop
        ///  ownedLoop - loop, but only run when at least one is owned.
        ///  create (called when the Growth is created, args[0] = amount)
        ///  INJ_RESET (Called when a reset is made, args[0] = strength)
        /// </summary>
        public Dictionary<string, List<codeInject>> Injects { get; private set; }

        public Dictionary<string, EExt> Extensions { get; private set; }

        public Entity(Forest forest, string name, string group = E.GRP_FOREST, int strength = 0) {
            Name = name;
            this.Forest = forest;
            Unlocked = false;
            Injects = new Dictionary<string, List<codeInject>> {
                ["loop"] = new List<codeInject>(),
                ["ownedLoop"] = new List<codeInject>(),
                [E.INJ_CREATE] = new List<codeInject>(),
                [E.INJ_RESET] = new List<codeInject>(),
                [E.INJ_REQUIREMENTS] = new List<codeInject>()
        };
            foreach (string stat in Statics.statList) {
                if (!forest.Values.ContainsKey(name + stat)) {
                    forest.Values[name + stat] = 0;
                }
            }
            Amount = 0;
            Description = "";
            Group = group;
            Extensions = new Dictionary<string, EExt>();
            Strength = strength;
        }

        public void Trigger(string trigger, params RuntimeValue[] arguments) {
            if(Injects.ContainsKey(trigger)) {
                foreach(codeInject inj in Injects[trigger]) {
                    inj(Forest, this, arguments);
                }
            }
            if(HasExtension(E.EEXT_REQUIREMENTS)) {
                Unlocked = ((EExtRequirements)Extensions[E.EEXT_REQUIREMENTS]).Evaluate();
            }
        }

        /// <summary>
        /// Adds a trigger to the given trigger key
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="inject"></param>
        public void AddTrigger(string trigger, codeInject inject) {
            if (!Injects.ContainsKey(trigger))
                Injects[trigger] = new List<codeInject>();
            Injects[trigger].Add(inject);
        }

        public Entity Add(EExt extension) {
            extension.Entity = this;
            Extensions.Add(extension.Name, extension);
            return this;
        }

        public bool HasExtension(string extension) => Extensions.ContainsKey(extension);

        public virtual void Loop() {
            foreach(EExt e in Extensions.Values) {
                e.Loop();
            }
            foreach (codeInject i in Injects["loop"])
                i(Forest, this, null);
            if (Unlocked && Amount > 0) {
                foreach (codeInject i in Injects["ownedLoop"])
                    i(Forest, this, null);
            }
        }

        /// <summary>
        /// If percentage is used, amount is the percentage of total resources to use.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="percentage"></param>
        /// <returns></returns>
        public virtual bool Create(int amount, bool percentage = false) {
            if (!Extensions.ContainsKey(E.EEXT_BUYABLE))
                return false;
            else {
                if (((EExtBuyable)Extensions[E.EEXT_BUYABLE]).Create(amount, percentage)) {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Called when a number of these are added to the forest; could even be zero. 
        /// Calls all extensions' onAdd function
        /// </summary>
        /// <param name="amount"></param>
        public void OnAdd(int amount) {
            foreach (EExt ext in Extensions.Values) {
                ext.OnAdd(amount);
            }
            Forest.Trigger(E.TRG_ENTITY_ADDED, Name, amount);
        }

        public void OnReset(int resetStrength) {
            if(resetStrength == 1) {
                Forest.SoftValues[E.SV_COUNT + Name] = Amount;
                if (Amount > Forest.SoftValues[E.SV_MAX_COUNT + Name])
                    Forest.SoftValues[E.SV_MAX_COUNT + Name] = Amount;
            }
            foreach(codeInject c in Injects[E.INJ_RESET]) {
                c(Forest, this, new RuntimeValue(2, resetStrength));
            }
            if (Strength < resetStrength) {
                Unlocked = false;
                Amount = 0;
            }
        }

        public void OnEnable() {
            foreach (EExt ext in Extensions.Values) {
                ext.OnEnable();
            }
        }

        public void OnDisable() {
            foreach (EExt ext in Extensions.Values) {
                ext.OnDisable();
            }
        }

        public virtual void Echo(bool writeDescription = false) {
            string result = Statics.GetDisplayNumber(Amount) + " " + Name + "";
            foreach (EExt extension in Extensions.Values) {
                result += extension.ShortDescription;
            }
            Console.WriteLine(result);
            if (writeDescription)
                Console.WriteLine(Description);
        }

        public void EchoPrice() {
            if (!Extensions.ContainsKey(E.EEXT_BUYABLE))
                return;
            else
                ((EExtBuyable)Extensions[E.EEXT_BUYABLE]).EchoPrice();
        }

        public Dictionary<string, double> GetStats() {
            Dictionary<string, double> result = new Dictionary<string, double>();
            foreach (string stat in Statics.statList) {
                result[stat] = Forest.GetValue(Name + stat) * Amount;
            }
            return result;
        }

        public virtual void Save(XElement growthElement) {
            XMLUtils.CreateElement(growthElement, "Amount", Math.Round(Amount, 3));
            XMLUtils.CreateElement(growthElement, "Unlocked", Unlocked);
        }

        public virtual void Load(XElement growthElement) {
            Amount = XMLUtils.GetDouble(growthElement, "Amount");
            Unlocked = XMLUtils.GetBool(growthElement, "Unlocked");
        }

        /// <summary>
        /// Called when thinking happens for strength 1 or above entities to reapply their bonuses.
        /// </summary>
        public void ReApply() {
            if (Extensions.ContainsKey(E.EEXT_MODIFIER)) {
                ((EExtModifier)Extensions[E.EEXT_MODIFIER]).OnAdd(Amount);
            }
        }
    }
}