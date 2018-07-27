using ActualIdle.Entity_Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {
    public class Talent : Entity {

        /// <summary>
        /// Add requirements with AddRequirements
        /// </summary>
        /// <param name="forest"></param>
        /// <param name="name"></param>
        public Talent(Forest forest, string name, string description, Modifier modifier=null) :base(forest, name, E.GRP_TALENTS) {
            Description = description;
            if(modifier != null) {
                Add(new EExtModifier(modifier));
            }
        }

    }
}
