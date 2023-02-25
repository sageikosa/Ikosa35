using System;
using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Creatures.BodyType;

namespace Uzi.Ikosa.Creatures.Types
{
    /// <summary>
    /// Grants ExtraHealthPoints based on body size
    /// </summary>
    [Serializable]
    public class ConstructType : CreatureType, IMonitorChange<Body>, IMonitorChange<Size>
    {
        public override string Name { get { return @"Construct"; } }
        public override bool IsLiving { get { return false; } }

        public IEnumerable<Type> BlockedAdjuncts()
        {
            yield return typeof(Poisoned);
            yield return typeof(UnconsciousEffect);
            yield return typeof(SleepEffect);
            yield return typeof(StunnedEffect);
            yield return typeof(ParalyzedEffect);
            yield return typeof(Diseased);
            yield return typeof(Fatigued);
            yield return typeof(Exhausted);
            // TODO: death effects
            // TODO: necromancy
            yield break;
        }

        protected override void OnBind()
        {
            base.OnBind();
            Creature.BodyDock.AddChangeMonitor(this);

            // creatures start with Medium NoBody
            Creature.HealthPoints.ExtraHealthPoints += ExpectedHealthPoints(Creature.Body.Sizer.Size);
            Creature.Body?.Sizer.AddChangeMonitor(this);
        }

        protected override void OnUnBind()
        {
            Creature.Body?.Sizer.RemoveChangeMonitor(this);
            Creature.BodyDock.RemoveChangeMonitor(this);
            base.OnUnBind();
        }

        #region private int ExpectedHealthPoints(Size size)
        private int ExpectedHealthPoints(Size size)
        {
            switch (size.Order)
            {
                case -1: return 10;
                case 0: return 20;
                case 1: return 30;
                case 2: return 40;
                case 3: return 60;
                case 4: return 80;
                default: return 0;
            }
        }
        #endregion

        #region IMonitorChange<Body>
        public void PreTestChange(object sender, AbortableChangeEventArgs<Body> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<Body> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<Body> args)
        {
            var _oldEHP = ExpectedHealthPoints(args.OldValue?.Sizer.Size ?? Size.Fine);
            var _newEHP = ExpectedHealthPoints(args.NewValue?.Sizer.Size ?? Size.Fine);
            args.OldValue?.Sizer.RemoveChangeMonitor(this);
            args.NewValue?.Sizer.AddChangeMonitor(this);
            Creature.HealthPoints.ExtraHealthPoints += _newEHP - _oldEHP;
        }
        #endregion

        #region IMonitorChange<Size>

        public void PreTestChange(object sender, AbortableChangeEventArgs<Size> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<Size> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<Size> args)
        {
            var _oldEHP = ExpectedHealthPoints(args?.OldValue ?? Size.Fine);
            var _newEHP = ExpectedHealthPoints(args?.NewValue ?? Size.Fine);
            Creature.HealthPoints.ExtraHealthPoints += _newEHP - _oldEHP;
        }
        #endregion
    }
}
