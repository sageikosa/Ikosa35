using System;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Alignment attached to the core object.</summary>
    [Serializable]
    public abstract class Aligned : Adjunct, IAlignmentAura
    {
        protected Aligned(object source, Alignment alignment)
            :base(source)
        {
            _Align = alignment;
        }

        private Alignment _Align;
        public Alignment Alignment { get { return _Align; } }

        public abstract AuraStrength AlignmentStrength { get; }
        public abstract int PowerLevel { get; }
        public AuraStrength AuraStrength { get { return AlignmentStrength; } }
    }

    public static class AlignedExtension
    {
        public static Alignment GetAlignment(this IAdjunctable self)
        {
            var _order = LawChaosAxis.Neutral;
            var _ethic = GoodEvilAxis.Neutral;
            var _alignments = self.Adjuncts.OfType<Aligned>();
            foreach (var _align in _alignments)
            {
                if (_order == LawChaosAxis.Neutral)
                {
                    _order = _align.Alignment.Orderliness;
                }
                if (_ethic == GoodEvilAxis.Neutral)
                {
                    _ethic = _align.Alignment.Ethicality;
                }
            }
            return new Alignment(_order, _ethic);
        }
    }
}
