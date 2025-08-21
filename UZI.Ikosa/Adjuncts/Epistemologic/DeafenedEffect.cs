using System;
using System.Linq;
using Uzi.Core;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class DeafenedEffect : Adjunct, ITrackTime, IMonitorChange<Activation>
    {
        #region Construction
        public DeafenedEffect(object source, double endTime, double resolution)
            : base(source)
        {
            _Senses = [];
            _EndTime = endTime;
            _TimeRes = resolution;
        }
        #endregion

        // senses that need to be reactivated...
        private Collection<SensoryBase> _Senses;

        protected override void OnActivate(object source)
        {
            var _critter = Anchor as Creature;
            if (_critter != null)
            {
                _critter.Conditions.Add(new Condition(Condition.Deafened, this));
                foreach (var _sense in from _s in _critter.Senses.AllSenses
                                       where _s.UsesHearing && _s.IsActive
                                       select _s)
                {
                    _sense.IsActive = false;
                    if (!_sense.IsActive)
                    {
                        _sense.AddChangeMonitor(this);
                        _Senses.Add(_sense);
                    }
                }

                _critter.Awarenesses.RecalculateAwareness(_critter);
            }
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            Creature _critter = Anchor as Creature;
            if (_critter != null)
            {
                _critter.Conditions.Remove(_critter.Conditions[Condition.Deafened, this]);
                foreach (SensoryBase _sense in _Senses)
                {
                    _sense.RemoveChangeMonitor(this);
                    _sense.IsActive = true;
                }
                _Senses.Clear();

                _critter.Awarenesses.RecalculateAwareness(_critter);
            }
            base.OnDeactivate(source);
        }

        #region ITrackTime Members
        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if ((timeVal >= _EndTime) && (direction == TimeValTransition.Entering))
            {
                Anchor.RemoveAdjunct(this);
            }
        }

        private double _EndTime;
        public double EndTime { get { return _EndTime; } }

        private double _TimeRes;
        public double Resolution { get { return _TimeRes; } }
        #endregion

        #region IMonitorChange<Activation> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<Activation> args)
        {
            if (args.NewValue.IsActive)
            {
                args.DoAbort();
            }
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<Activation> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<Activation> args)
        {
        }
        #endregion

        public override object Clone()
        {
            return new DeafenedEffect(Source, this.EndTime, this.Resolution);
        }
    }
}
