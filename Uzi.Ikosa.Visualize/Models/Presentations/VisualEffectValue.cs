using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Visualize
{
    public class VisualEffectValue
    {
        public VisualEffectValue(Type type, VisualEffect effect)
        {
            this.Type = type;
            this.Effect = effect;
            this.HashVal = string.Format(@"{0}|{1}", this.Type.AssemblyQualifiedName, (byte)effect);
        }

        public Type Type { get; private set; }
        public VisualEffect Effect { get; private set; }

        public string HashVal { get; private set; }
    }
}
