using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ActualIdle {
    public class XMLUtils {

        /// <summary>
        /// Creates a new XElement with the specified owner, specified name and specified contents.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="name"></param>
        /// <param name="contents"></param>
        public static XElement CreateElement(XElement owner, string name, object contents=null) {
            XElement element = null;
            name = name.Replace(' ', '_');
            if (contents != null)
                element = new XElement(name, contents);
            else
                element = new XElement(name);
            owner.Add(element);
            return element;
        }
    }
}
