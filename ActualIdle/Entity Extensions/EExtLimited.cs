using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle.Entity_Extensions {
    public class EExtLimited : EExt {
        public override string Name => E.EEXT_LIMITED;

        public EExtLimited() : base() { }

        /// <summary>
        /// Whether a given amount of limited entites are allowed to be made.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool IsAllowed(int amount) {
            if (Entity.Forest.Values[E.BOUGHT_THINGS] + amount > Entity.Forest.GetValue(E.ALLOWED_GROWTHS)) {
                Console.WriteLine("You cannot have more than " + Entity.Forest.GetValue(E.ALLOWED_GROWTHS) + " growths!");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Called when a given amount of limited entities are made.
        /// </summary>
        /// <param name="amount"></param>
        public override void OnAdd(double amount) {
            Entity.Forest.Values[E.BOUGHT_THINGS] += amount;
        }
    }
}
