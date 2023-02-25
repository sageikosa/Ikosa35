using System;
using Uzi.Core;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Removes self and other adjunct(s) upon expiration</summary>
    [Serializable]
    public class Expiry : Adjunct, ITrackTime
    {
        #region construction
        public Expiry(Adjunct adjunct, double endTime, TimeValTransition direction, double resolution)
            : base(new Adjunct[] { adjunct })
        {
            _EndTime = endTime;
            _Resolution = resolution;
            _Direction = direction;
        }

        public Expiry(Adjunct[] adjunct, double endTime, TimeValTransition direction, double resolution)
            : base(adjunct)
        {
            _EndTime = endTime;
            _Resolution = resolution;
            _Direction = direction;
        }
        #endregion

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);

            // add all controlled adjuncts
            if (ExpirableAdjuncts != null)
                foreach (var _adj in ExpirableAdjuncts)
                {
                    if (!Anchor.Adjuncts.Contains(_adj))
                        Anchor.AddAdjunct(_adj);
                }
        }

        protected override void OnDeactivate(object source)
        {
            // eject all controlled adjuncts
            if (ExpirableAdjuncts != null)
            {
                foreach (var _adj in ExpirableAdjuncts)
                    _adj.Eject();
            }
            base.OnDeactivate(source);
        }

        #region private data
        private double _EndTime;
        private double _Resolution;
        private TimeValTransition _Direction;
        #endregion

        public override bool IsProtected => true;

        public double EndTime => _EndTime;
        public TimeValTransition Direction => _Direction;
        public Adjunct[] ExpirableAdjuncts => Source as Adjunct[];
        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if ((timeVal >= EndTime) && (direction == _Direction))
            {
                Eject();
            }
        }
        public double Resolution => _Resolution;

        public override object Clone()
            => new Expiry(ExpirableAdjuncts, _EndTime, _Direction, Resolution);
    }
}
