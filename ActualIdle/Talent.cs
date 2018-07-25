using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {
    public class Talent : Entity {
        public Talent(Forest forest, string name) :base(forest, name, E.GRP_TALENTS) {

        }
    }
}
