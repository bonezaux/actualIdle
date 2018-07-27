using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle.Entity_Extensions {
    public class EExtModifier : EExt
    {
        public override string Name => E.EEXT_MODIFIER;

        public Modifier Modifier { get; set; }
        public bool Scale { get; set; }
        public bool ReduceMultipliers { get; set; }

        public EExtModifier(Modifier modifier, bool scale = false, bool reduceMultipliers=true) {
            Modifier = modifier;
            Scale = scale;
            ReduceMultipliers = reduceMultipliers;
        }

        public override void OnAdd(double amount) {
            if (Entity.Forest.HasModifier(Modifier)) {
                System.Diagnostics.Debug.Assert(Modifier != null);
                if (Scale)
                    Entity.Forest.GetModifier(Modifier.Name).AddModifier(Modifier, amount, ReduceMultipliers);
            } else {
                Console.WriteLine(Entity.Name + " adding modifier!");
                Entity.Forest.AddModifier(Modifier);
            }
        }
        
        public override void OnEnable() {
        }
        /// <summary>
        /// Deactivates the modifier
        /// </summary>
        public override void OnDisable() {
            Entity.Forest.RemoveModifier(Modifier.Name);
        }
    }
}
