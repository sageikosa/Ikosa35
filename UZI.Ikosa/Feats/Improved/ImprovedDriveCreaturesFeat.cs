using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo(@"Improved Drive Creatures", true)
    ]
    public class ImprovedDriveCreaturesFeat : FeatBase, IQualifyDelta
    {
        public ImprovedDriveCreaturesFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
            _TermCtrl = new TerminateController(this);
            _Delta = new QualifyingDelta(1, typeof(ImprovedDriveCreaturesFeat), @"Improved Drive Creatures");
        }

        #region data
        private IDelta _Delta;
        private readonly TerminateController _TermCtrl;
        #endregion

        public override string Benefit
            => @"Class Power Level +1 for all creature driving powers.";

        protected override void OnActivate()
        {
            base.OnActivate();
            if (Creature != null)
            {
                Creature.ExtraClassPowerLevel.Deltas.Add(this);
            }
        }

        protected override void OnDeactivate()
        {
            DoTerminate();
            base.OnDeactivate();
        }

        // ISupplyQualifyDelta Members
        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
            => (IsActive && (qualify as Interaction)?.InteractData is DriveCreatureData
            ? _Delta
            : null).ToEnumerable().Where(_d => _d != null);

        #region IControlTerminate Members

        public void DoTerminate()
        {
            _TermCtrl.DoTerminate();
        }

        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _TermCtrl.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _TermCtrl.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _TermCtrl.TerminateSubscriberCount;

        #endregion
    }
}