using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {
    public class Chest : Fighter{

        public int Items { get; set; }
        public Chest(string name, string lootTable, int items, string description = null) :base(1, 0, 0, name, null, null, null, description, 0, false) {
            Debug.Assert(Initializer.lootTables.ContainsKey(lootTable));
            LootTable = lootTable;
            Items = items;
        }

        public override void StartFight(Fighter fighter) {
            base.StartFight(fighter);
            string[] picks = new string[Items];
            for(int loop=0;loop<Items;loop++) {
                picks[loop] = Initializer.GetLoot(LootTable);
            }

            Console.WriteLine("You are presented with two items: ");
            int i = 1;
            foreach (string pick in picks) {
                Console.WriteLine(i + ": " + pick);
                i++;
            }
            while(true) {
                Console.Write("Which do you want? ");
                string s = Console.ReadLine();
                if(int.TryParse(s, out int result) && result <= Items && result > 0) {
                    ((Forest)fighter).AddItem(picks[result-1], 1);
                    break;
                }
            }
            
        }

        public override Fighter Clone() {
            return new Chest(Name, LootTable, Items, Description);
        }
    }
}
