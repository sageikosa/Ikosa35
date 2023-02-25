using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Creatures.BodyType;

namespace Uzi.Ikosa.Creatures.Templates
{
    [Serializable]
    public class HalfCelestial : IReplaceCreature
    {
        public HalfCelestial(Creature original)
        {
            _Original = original;
        }

        public string TemplateName => nameof(HalfCelestial);

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
                    // non-evil, living, and smart-enough
                    if ((Original.Alignment.Ethicality != GoodEvilAxis.Evil)
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
