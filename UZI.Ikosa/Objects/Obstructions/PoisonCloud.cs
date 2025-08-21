using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Adjuncts;
using Uzi.Visualize;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Objects
{
    /// <summary>
    /// Used as a source for a poison cloud LocatorCapture
    /// </summary>
    [Serializable]
    public class PoisonCloud : LocatableObject, ILocatorZone, IPoisonProvider
    {
        // TODO: look at Ghoulish stench...
        // TODO: really not ready for cloud management, but this is needed for Detect Poison...
        public PoisonCloud(string name, Poison poison, bool isVisible)
            : base(name, isVisible)
        {
            var _psn = new Poisonous(this);
            AddAdjunct(_psn);
        }

        #region public Poison Poison { get; }
        [NonSerialized, JsonIgnore]
        private Poison _Poison = null;
        public Poison Poison
        {
            get
            {
                _Poison ??= Poisonous.GetPoison(this);
                return _Poison;
            }
        }
        #endregion

        #region ILocatorZone Members
        public void Start(Locator locator)
        {
            // attempt to poison any creature within the zone
            throw new NotImplementedException();
        }

        public void Enter(Locator locator)
        {
            // attempt to poison any creature entering the zone
            throw new NotImplementedException();
        }

        public void Capture(Locator locator)
        {
            // attempt to poison any creature captured when the zone moves
            throw new NotImplementedException();
        }

        public void MoveInArea(Locator locator, bool followOn)
        {
            // attempt to poison any creature still moving in the zone
            throw new NotImplementedException();
        }

        public void End(Locator locator) { }
        public void Exit(Locator locator) { }
        public void Release(Locator locator) { }

        /// <summary>
        /// Always active
        /// </summary>
        public bool IsActive { get { return true; } }
        #endregion

        #region IControlChange<Activation> Members
        public void AddChangeMonitor(IMonitorChange<Activation> monitor) { }
        public void RemoveChangeMonitor(IMonitorChange<Activation> monitor) { }
        #endregion

        public override Info GetInfo(CoreActor actor, bool baseValues)
            => null;

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
            => fetchedInfo;

        public Poison GetPoison()
        {
            return Poison;
        }

        public override Sizer Sizer
            => new ObjectSizer(Size.Medium, this);

        public override IGeometricSize GeometricSize
            => new GeometricSize(1, 1, 1);

        protected override string ClassIconKey
            => nameof(PoisonCloud);

        public override bool IsTargetable => false;
    }
}
