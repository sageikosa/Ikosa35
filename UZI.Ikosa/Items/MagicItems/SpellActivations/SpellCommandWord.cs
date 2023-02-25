using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class SpellCommandWord : SpellActivation, IProtectable, IIdentification
    {
        public SpellCommandWord(SpellSource source, ISpellMode mode, IPowerCapacity powerCapacity, 
            int chargeCost, SpellActivationCost cost, bool affinity, string similarity)
            : base(source, powerCapacity, cost, affinity, similarity)
        {
            _SpellMode = mode;
            _ChargeCost = chargeCost;
        }

        #region data
        private ISpellMode _SpellMode;
        private int _ChargeCost;
        #endregion

        public ISpellMode SpellMode => _SpellMode;
        public int ChargeCost => _ChargeCost;

        public override decimal LevelUnitPrice
            => 1800m;

        public override object Clone()
            => new SpellCommandWord(SpellSource, SpellMode, PowerCapacity, ChargeCost, _Cost, Affinity, SimilarityKey);

        public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (IsActive)
            {
                if ((budget.Actor is Creature _critter) 
                    && IsExposedTo(_critter) 
                    && PowerCapacity.CanUseCharges(ChargeCost))
                {
                    yield return new CommandWordSpell(this, new ActionTime(TimeType.Regular), @"101");
                }
            }
            yield break;
        }

        public override Info GetProviderInfo(CoreActionBudget budget)
            => new AdjunctInfo(@"Command Word Activation", ID);

        public bool IsExposedTo(Creature critter)
            => this.HasExposureTo(critter);

        public virtual IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return SpellSource.ToSpellSourceInfo();
                //yield return new Info { Title = string.Format(@"Charges/Use: {0}", ChargePerUse) };
                //yield return new Info { Title = string.Format(@"Charges(ID): {0}", Battery.AvailableCharges) };
                yield break;
            }
        }
    }
}
