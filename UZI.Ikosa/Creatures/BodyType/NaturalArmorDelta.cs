using System;
using Uzi.Core;
using Uzi.Ikosa.Deltas;

namespace Uzi.Ikosa.Creatures.BodyType
{
    [Serializable]
    public class NaturalArmorDelta : Deltable, IModifier
    {
        public NaturalArmorDelta(Body body)
            : base(0)
        {
            _Terminator = new TerminateController(this);
        }

        public IDelta IDelta => (IDelta)this;

        public int Value => EffectiveValue;
        public object Source => typeof(NaturalArmor);
        public string Name => @"Natural Armor";

        public bool Enabled
        {
            get => true;
            set { }
        }

        #region IControlTerminate Members
        /// <summary>
        /// Tells all modifiable values using this modifier to release it.  Note: this does not destroy the modifier and it can be re-used.
        /// </summary>
        public void DoTerminate()
        {
            _Terminator.DoTerminate();
        }

        #region IControlTerminate Members
        private readonly TerminateController _Terminator;
        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Terminator.TerminateSubscriberCount;
        #endregion
        #endregion
    }
}
