using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {
    public class Runtime {

        public string parse(string code) {
            foreach( string word in code.Split(' ')) {

            }
            return "";
        }

        //1 = double literal, = int literal, 3= bool literal

        public RuntimeValue execute(string code) {
            Stack<RuntimeValue> stack = new Stack<RuntimeValue>();
            for(int loop=0;loop<code.Length;loop++) {
                char c = code[loop];
                if(c == 1) {
                    long data = (long)code[loop + 1] + code[loop + 2] * 65536 + code[loop + 3] * 4294967296 + code[loop + 4] * 281474976710656;
                    double result = BitConverter.Int64BitsToDouble(data);
                }
            }
            if (stack.Count > 0)
                return stack.Pop();
            return null;
        }
    }

    /// <summary>
    /// A runtime value. 
    /// </summary>
    public class RuntimeValue {

        /// <summary>
        /// 1 == double
        /// 2 == int
        /// 3 == bool
        /// 4 == string
        /// </summary>
        public int Type { get; private set; }
        public Object Value { get; private set; }

        public RuntimeValue(int type, Object value) {
            Type = type;
            Value = value;
        }

        public static explicit operator int (RuntimeValue rv) {
            if (rv.Type != 1 && rv.Type != 2)
                throw new InvalidCastException("RuntimeValue of type " + rv.Type + " cannot be cast to int");
            else
                return (int)rv.Value;
        }
        public static explicit operator double(RuntimeValue rv) {
            if (rv.Type != 1 && rv.Type != 2)
                throw new InvalidCastException("RuntimeValue of type " + rv.Type + " cannot be cast to int");
            else
                return (double)rv.Value;
        }
        public static explicit operator bool(RuntimeValue rv) {
            if (rv.Type != 3)
                throw new InvalidCastException("RuntimeValue of type " + rv.Type + " cannot be cast to int");
            else
                return (bool)rv.Value;
        }
        public static explicit operator string(RuntimeValue rv) {
            if (rv.Type != 4)
                throw new InvalidCastException("RuntimeValue of type " + rv.Type + " cannot be cast to int");
            else
                return (string)rv.Value;
        }
    }
}
