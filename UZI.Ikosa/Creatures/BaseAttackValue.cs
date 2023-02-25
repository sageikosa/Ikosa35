using System;
using Uzi.Core;

namespace Uzi.Ikosa
{
    [Serializable]
    public class BaseAttackValue : ConstDeltable, IModifier
    {
        public BaseAttackValue()
            : base(0)
        {
            _Terminator = new TerminateController(this);
        }

        /// <summary>Number of primary attacks available on a full attack</summary>
        public int FullAttackCount 
            => (int)Math.Min(Math.Floor(Math.Max(0d, EffectiveValue - 1d) / 5d) + 1d, 4);

        int IDelta.Value => EffectiveValue;
        object ISourcedObject.Source => this;
        string IDelta.Name => @"Base Attack";
        bool IDelta.Enabled { get => true; set { /*ignore*/ } }

        public override string ToString()
            => ((IDelta)this).Name;

        #region ITerminating Members
        /// <summary>
        /// Tells all modifiable values using this modifier to release it.  
        /// Note: this does not destroy the modifier and it can be re-used.
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
