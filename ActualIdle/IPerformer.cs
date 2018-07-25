using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle {
    public interface IPerformer {
        bool Unlocked { get; set; }
        string Name { get; }
        void Trigger(string trigger, params RuntimeValue[] arguments);
    }
}
