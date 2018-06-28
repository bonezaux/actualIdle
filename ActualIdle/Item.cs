using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {
    /// <summary>
    /// An inventory Item. You can have as many as you want.
    /// </summary>
    public class Item {
        public static List<Item> itemList = new List<Item>();

        /// <summary>
        /// The way values are changed when this is equipped. Will probably be changed into modifiers or something.
        /// Keys: 
        /// </summary>
        public Modifier Modifier;
        public string Name { get; private set; }
        public string Text { get; private set; }

        public Item(string name, Modifier modifier, string text) {
            Name = name;
            Modifier = modifier;
            Text = text;
        }

        public virtual void Loop(Forest forest) {

        }

        /// <summary>
        /// Called when the Druid attains the Item.
        /// </summary>
        /// <param name="forest"></param>
        public virtual void Get(Forest forest) {
            //TODO: Figure out modifiers
        }

        /// <summary>
        /// Called when the Druid loses the Item.
        /// </summary>
        /// <param name="forest"></param>
        public virtual void Lose(Forest forest) {
            //TODO: Figure out modifiers
        }

        public void Echo(Forest forest) {
            Console.WriteLine(Text);
        }
    }
}
