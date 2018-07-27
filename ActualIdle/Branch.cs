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
        public Forest Forest { get; private set; }

        public Branch(Forest forest, string name, string descText) {
            this.Forest = forest;
            Paths = new List<Path>();
            Name = name;
            DescText = descText;
        }

        public void AddPath(Path path) {
            Paths.Add(path);
        }

        /// <summary>
        /// Writes out the path's information.
        /// </summary>
        public void Echo() {
            Console.WriteLine(DescText);
            foreach(Path path in Paths) {
                if(path.Show) {
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
                bool allLocked = true;
                foreach(Path path in Paths) {
                    if (path.Unlocked)
                        allLocked = false;
                }
                if(allLocked) {
                    Console.WriteLine("You can't go anywhere from here.");
                    return null;
                }
                string l = Console.ReadLine();
                foreach(Path path in Paths) {
                    if (l.Trim().ToLower().Equals("exit"))
                        return null;
                    if(l.Trim().Equals(path.Name) && path.Unlocked) {
                        return path;
                    }
                }
            }
            return null;
            
        }
    }
}
