using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Creatures.BodyType;

namespace Uzi.Ikosa.Creatures.Templates
{
    [Serializable]
    public class HalfFiend : IReplaceCreature
    {
        public HalfFiend(Creature original)
        {
            _Original = original;
        }

        public string TemplateName => nameof(HalfFiend);

        private Creature _Original;
        public Creature Original => _Original;

        public bool IsAcquired => false;

        public bool CanGenerate
        {
            get
            {
                // corporeal
                if (!(Original?.Body is NoBody) && !(Original?.HasAdjunct<Incorporeal>() ?? true))
                {
                    // non-good, living, and smart-enough
                    if ((Original.Alignment.Ethicality != GoodEvilAxis.Good)
                        && (Original.CreatureType.IsLiving)
                        && (Original.Abilities.Intelligence.EffectiveValue >= 4))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
