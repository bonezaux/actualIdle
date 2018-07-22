using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle.Entity_Extensions {
    public class EExtBuyable : EExt {
        public Resources Price { get; set; }
        public override string Name => E.EEXT_BUYABLE;
        public List<codeInject> injects;

        public EExtBuyable(Resources price) {
            Price = price;
            injects = new List<codeInject>();
        }

        public bool Create(int amount, bool percentage) {
            if (percentage) {
                int resA = Price.GetBuys(Entity.forest, amount);
                Console.WriteLine("Buys : " + resA);
                if (resA > 0) {
                    Create(resA, false);
                }
                return true;
            }
            if (Price.CanAfford(Entity.forest, amount)) {
                foreach (codeInject gci in injects)
                    gci(Entity.forest, Entity, new RuntimeValue[] { new RuntimeValue(2, amount) });
                Console.WriteLine("You bought " + amount + " " + Name + " for ");
                Price.Print(Entity.forest, amount);
                Price.Apply(Entity.forest, amount);
                Entity.Amount += amount;
                Entity.OnAdd(amount);
                return true;
            }
            else {
                Console.WriteLine("You don't have enough to buy " + amount + " " + Name + "! You need");
                Price.Print(Entity.forest, amount);
            }
            return false;
        }

        public void EchoPrice() {
            string result = Entity.Name + ": ";
            result += Price.Text(Entity.forest, 1);
            Console.WriteLine(result);
        }
    }
}
