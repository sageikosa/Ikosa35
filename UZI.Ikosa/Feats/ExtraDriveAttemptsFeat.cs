using System;
using Uzi.Core;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    FeatInfo(@"Extra Drive Attempts")
    ]
    public class ExtraDriveAttemptsFeat : FeatBase
    {
        public ExtraDriveAttemptsFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Benefit
        {
            get { return @"Make 4 more drive creatures attempts per day, for each driving power."; }
        }

        protected Delta _Extra;

        protected override void OnAdd()
        {
            base.OnAdd();
            _Extra = new Delta(4, this, @"Extra Drive Attempts");
            Creature.ExtraDrivingBattery.Deltas.Add(_Extra);
        }

        protected override void OnRemove()
        {
            base.OnRemove();
            _Extra.DoTerminate();
        }
    }
}
