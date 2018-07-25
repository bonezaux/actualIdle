using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle.Entity_Extensions {
    /// <summary>
    /// For evaluating whether 
    /// </summary>
    public class EExtRequirements : EExt {
        public string Requirements { get; set; }
        public override string Name => E.EEXT_REQUIREMENTS;

        public EExtRequirements(string requirements) {
            Requirements = requirements;
        }

        public bool Evaluate() {
            if (!Entity.Forest.TestRequirements(Requirements))
                return false;
            foreach (codeInject i in Entity.Injects[E.INJ_REQUIREMENTS])
                if (!(bool)i(Entity.Forest, Entity))
                    return false;
            return true;
        }
    }
}
