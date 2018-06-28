using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {

    /// <summary>
    /// Will become a trophy somehow
    /// </summary>
    public class Trophy {
        public string Name { get; private set; }
        public string[] Requirements { get; set; }
        public string Text { get; set; }
        public bool Unlocked { get; set; }
        public Forest forest { get; private set; }

        public Trophy(Forest forest, string name, string[] requirements, string text) {
            Name = name;
            Requirements = requirements;
            Text = text;
            Unlocked = false;
            this.forest = forest;
        }

        public void Loop() {
            if (!Unlocked) {
                bool result = true;
                foreach (string requirement in Requirements)
                    if (!forest.TestRequirement(requirement))
                        result = false;
                if (result) {
                    Console.WriteLine(Text);
                    Unlocked = true;
                    ///TODO: add trohphy code.
                }
            }
        }
    }
}
