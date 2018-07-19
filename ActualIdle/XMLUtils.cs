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

        /// <summary>
        /// Gets an XElement from the specified owner by the specified name. Returns the first child element of that name.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static XElement GetElement(XElement owner, string name) {
            name = name.Replace(' ', '_');
            foreach(XElement possible in owner.Elements()) {
                if (possible.Name.LocalName.Equals(name))
                    return possible;
            }
            return null;
        }

        /// <summary>
        /// Returns the double value of the child of the element of name.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static double GetDouble(XElement element, string name) {
            return double.Parse(GetElement(element, name).Value);
        }

        /// <summary>
        /// Returns the string value of the child of the element of name.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetString(XElement element, string name) {
            return (GetElement(element, name).Value);
        }

        /// <summary>
        /// Returns the bool value of the child of the element of name.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool GetBool(XElement element, string name) {
            return bool.Parse(GetElement(element, name).Value);
        }

        /// <summary>
        /// Returns the name of the given element, with underscores replaced with _
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static string GetName(XElement element) {
            return element.Name.LocalName.Trim().Replace('_', ' ');
        }
    }
}
