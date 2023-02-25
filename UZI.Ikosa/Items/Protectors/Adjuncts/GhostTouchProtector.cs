using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class GhostTouchProtector : ProtectorAdjunct
    {
        public GhostTouchProtector(object source)
            : base(source, 3, 0m)
        {
            _Ptr = null;
        }

        #region state
        private DeltaPtr _Ptr;
        #endregion

        public override IEnumerable<Info> IdentificationInfos
            => (new Description(@"Ghost Touch",
                    new string[]
                    {
                        @"Effective against incorporeal attacks",
                        @"Incorporeal actors may wear"
                    })).ToEnumerable();

        public override object Clone()
            => new GhostTouchProtector(Source);

        protected override void OnActivate(object source)
        {
            // get protector delta
            _Ptr = new DeltaPtr(Protector);

            // then complete the activation
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            base.OnDeactivate(source);

            // tear down and destroy
            _Ptr?.DoTerminate();
            _Ptr = null;
        }

        protected override void OnSlottedActivate()
        {
            // associate with incorporeal armor rating
            Protector.CreaturePossessor.IncorporealArmorRating.Deltas.Add(Protector);
        }

        protected override void OnSlottedDeActivate()
        {
            // tear it down
            _Ptr?.DoTerminate();
        }
    }
}
