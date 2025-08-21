using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa
{
    [Serializable]
    public class TempHPAdjunct : Adjunct, ITrackTime
    {
        public TempHPAdjunct(object source, int amount, double endTime)
            : base(source)
        {
            _Amount = amount;
            _EndTime = endTime;
        }

        #region data
        private double _EndTime;
        private int _Amount;
        private TempHPChunk _Chunk;
        #endregion

        public int OriginalAmount => _Amount;
        public double EndTime => _EndTime;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (Anchor is Creature _critter)
            {
                _Chunk = new TempHPChunk(_critter.TempHealthPoints, new Delta(OriginalAmount, this));
            }
        }

        protected override void OnDeactivate(object source)
        {
            if ((Anchor is Creature _critter) && (_Chunk != null))
            {
                _critter.TempHealthPoints.Remove(_Chunk);
            }
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new TempHPAdjunct(Source, OriginalAmount, EndTime);

        public double Resolution => Hour.UnitFactor;

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if (timeVal >= EndTime)
            {
                Eject();
            }
        }
    }
}
