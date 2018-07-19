using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ActualIdle {

    /// <summary>
    /// Will become a trophy somehow
    /// </summary>
    public class Trophy : IEntity {
        public string Name { get; private set; }
        public codeInject[] Requirements { get; set; }
        public string Text { get; set; }
        public bool Unlocked { get; set; }
        public Forest forest { get; private set; }

        public Trophy(Forest forest, string name, codeInject[] requirements, string text) {
            Name = name;
            Requirements = requirements;
            Text = text;
            Unlocked = false;
            this.forest = forest;
        }

        public void Loop() {
            if (!Unlocked) {
                bool result = true;
                foreach (codeInject requirement in Requirements)
                    if (!requirement(forest, this, null).GetBool())
                        result = false;
                if (result) {
                    Console.WriteLine(Text);
                    Unlocked = true;
                    forest.Values["Trophy" + Name] = 1;
                    ///TODO: add trohphy code.
                }
            }
        }

        public void Save(XElement trophyElement) {
            XMLUtils.CreateElement(trophyElement, "Unlocked", Unlocked);
        }

        public void Load(XElement trophyElement) {
            Unlocked = XMLUtils.GetBool(trophyElement, "Unlocked");
        }
    }
}
