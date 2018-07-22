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
            if (Entity.forest.HasModifier(Modifier)) {
                System.Diagnostics.Debug.Assert(Modifier != null);
                if (Scale)
                    Entity.forest.GetModifier(Modifier.Name).AddModifier(Modifier, amount, ReduceMultipliers);
            }
        }

        /// <summary>
        /// Activates  the modifier
        /// </summary>
        public override void OnEnable() {
            Entity.forest.AddModifier(Modifier);
        }
        /// <summary>
        /// Deactivates the modifier
        /// </summary>
        public override void OnDisable() {
            Entity.forest.RemoveModifier(Modifier.Name);
        }
    }
}
