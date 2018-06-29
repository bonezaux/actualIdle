using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {
    /// <summary>
    /// A branch in the path, where you can pick your next path.
    /// </summary>
    public class Branch {
        /// <summary>
        /// Paths to continue on.
        /// </summary>
        public List<Path> Paths { get; private set; }
        /// <summary>
        /// Name of the branch.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Description of the branch.
        /// </summary>
        public string DescText { get; private set; }

        public Branch(string name, string descText) {
            Paths = new List<Path>();
            Name = name;
            DescText = descText;
        }

        /// <summary>
        /// Writes out the path's information.
        /// </summary>
        public void Echo() {
            Console.WriteLine(DescText);
            foreach(Path path in Paths) {
                if(path.Unlocked) {
                    path.Echo();
                }
            }
        }

        public Path PickPath() {
            if (Paths.Count == 0) {
                Echo();
                Console.WriteLine("You have nowhere to go from here currently.");
            }
            bool picked = false;
            while(!picked) {
                Echo();
                string l = Console.ReadLine();
                foreach(Path path in Paths) {
                    if(l.Trim().Equals(path.Name)) {
                        return path;
                    }
                }
            }
            return null;
            
        }
    }
}
