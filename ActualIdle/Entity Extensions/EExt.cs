using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActualIdle.Entity_Extensions {
    /// <summary>
    /// Extensions for Entities, not IEntities.
    /// </summary>
    public abstract class EExt
    {
        public abstract string Name { get; }
        public Entity Entity { get; set; }
        public virtual string ShortDescription => "";
        
        /// <summary>
        /// Called when a number of the entity is created / added.
        /// </summary>
        /// <param name="amount"></param>
        public virtual void OnAdd(double amount) { }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
        public virtual void Echo() { }
        public virtual void Loop() { }
        public virtual void Trigger(string trigger, params RuntimeValue[] arguments) { }
    }
}
