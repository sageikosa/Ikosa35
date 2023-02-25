using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// The Poisonous aspect is meant to indicate to Detect Poison that a creature, object or area is poisonous.
    /// </summary>
    [Serializable]
    public class Poisonous : Adjunct
    {
        public Poisonous(IPoisonProvider source)
            : base(source)
        {
        }

        public Poison Poison { get { return (Source as IPoisonProvider).GetPoison(); } }
        public static bool IsPoisonous(CoreObject holder)
        {
            return holder.Adjuncts.Where(e => e.IsActive && e is Poisonous).Count() == 0;
        }
        public static Poison GetPoison(CoreObject holder)
        {
            Adjunct _effect = holder.Adjuncts.Where(e => e.IsActive && e is Poisonous).FirstOrDefault();
            if (_effect != null)
            {
                Poisonous _psn = _effect as Poisonous;
                return _psn.Poison;
            }
            return null;
        }

        public override object Clone()
        {
            return new Poisonous(Source as IPoisonProvider);
        }
    }
}
