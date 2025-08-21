using System;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Items.Weapons;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class AlignedItem : Aligned
    {
        public AlignedItem(object source, Alignment alignment, int powerLevel)
            : base(source, alignment)
        {
            _Power = powerLevel;
        }

        public override AuraStrength AlignmentStrength
        {
            get
            {
                if (_Power <= 2)
                {
                    return AuraStrength.Faint;
                }

                if (_Power <= 8)
                {
                    return AuraStrength.Moderate;
                }

                if (_Power <= 20)
                {
                    return AuraStrength.Strong;
                }

                return AuraStrength.Overwhelming;
            }
        }

        private int _Power;
        public override int PowerLevel => _Power;

        public override object Clone()
            => new AlignedItem(Source, Alignment, PowerLevel);
    }
}
