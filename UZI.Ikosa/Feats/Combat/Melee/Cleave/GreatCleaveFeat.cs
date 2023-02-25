using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Feats
{
    [Serializable]
    [FighterBonusFeat]
    [AbilityRequirement(MnemonicCode.Str, 13)]
    [FeatChainRequirement(typeof(PowerAttackFeat))]
    [FeatChainRequirement(typeof(CleaveFeat))]
    [BaseAttackRequirement(4)]
    [FeatInfo(@"Great Cleave")]
    public class GreatCleaveFeat : FeatBase
    {
        public GreatCleaveFeat(object source, int powerLevel) 
            : base(source, powerLevel)
        {
            _Boost = new ConstDelta(99, this);
        }

        #region data
        private ConstDelta _Boost;
        #endregion

        public ConstDelta CleaveBoost => _Boost;

        public override string Benefit => @"Effectively unlimited cleave attacks in a round";

        protected override void OnActivate()
        {
            Creature?.Feats.OfType<CleaveFeat>().FirstOrDefault()?.Capacity.Deltas.Add(_Boost);
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            _Boost.DoTerminate();
            base.OnDeactivate();
        }
    }
}
