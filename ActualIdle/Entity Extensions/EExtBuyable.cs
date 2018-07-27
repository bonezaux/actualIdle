using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle.Entity_Extensions {
    public class EExtBuyable : EExt {
        public Resources Price { get; set; }
        public override string Name => E.EEXT_BUYABLE;
        /// <summary>
        /// Whether it is possible to buy more than one.
        /// </summary>
        public bool BuySeveral { get; set; }

        public EExtBuyable(Resources price, bool buySeveral = true) {
            Price = price;
            BuySeveral = buySeveral;
        }

        public bool Create(int amount, bool percentage) {
            if(!BuySeveral && (amount > 1 || Entity.Amount > 0)) {
                Console.WriteLine("You can only buy one " + Entity.Name);
                return false;
            }
            if (percentage) {
                int resA = Price.GetBuys(Entity.Forest, amount);
                Console.WriteLine("Buys : " + resA);
                if (resA > 0) {
                    Create(resA, false);
                }
                return true;
            }
            if(Entity.HasExtension(E.EEXT_LIMITED)) {
                if (!((EExtLimited)Entity.Extensions[E.EEXT_LIMITED]).IsAllowed(amount))
                    return false;
            }
            if (Price.CanAfford(Entity.Forest, amount)) {
                foreach (codeInject gci in Entity.Injects[E.INJ_CREATE])
                    gci(Entity.Forest, Entity, new RuntimeValue[] { new RuntimeValue(2, amount) });
                Console.WriteLine("You bought " + (BuySeveral ? amount+" " : "")  + Entity.Name + " for ");
                Price.Print(Entity.Forest, amount);
                Price.Apply(Entity.Forest, amount);
                Entity.Amount += amount;
                Entity.OnAdd(amount);
                return true;
            }
            else {
                Console.WriteLine("You don't have enough to buy " + amount + " " + Entity.Name + "! You need");
                Price.Print(Entity.Forest, amount);
            }
            return false;
        }

        public void EchoPrice() {
            string result = Entity.Name + ": ";
            result += Price.Text(Entity.Forest, 1);
            Console.WriteLine(result);
        }

        public override void Echo() {
            Console.WriteLine(Price.Text(Entity.Forest, 1));
        }
    }
}
