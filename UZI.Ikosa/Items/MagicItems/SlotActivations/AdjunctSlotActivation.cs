using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class AdjunctSlotActivation : SlotActivation, IItemRequirements, IAugmentationCost
    {
        public AdjunctSlotActivation(Adjunct adjunct, Info info, 
            decimal cost, bool affinity, bool needsMasterwork)
            : base(adjunct, affinity)
        {
            _Info = info;
            _Cost = cost;
            _Masterwork = needsMasterwork;
        }

        #region state
        private readonly Info _Info;
        private readonly decimal _Cost;
        private readonly bool _Masterwork;
        #endregion

        public Info Info => _Info;

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return _Info;
                yield break;
            }
        }

        public bool RequiresMasterwork
            => _Masterwork;

        public decimal StandardCost
            => _Cost;

        protected override void OnSlottedActivate()
        {
            SlottedItem?.CreaturePossessor?.AddAdjunct(Source as Adjunct);
        }

        protected override void OnSlottedDeActivate()
        {
            (Source as Adjunct)?.Eject();
        }

        public override object Clone()
            => new AdjunctSlotActivation((Source as Adjunct).Clone() as Adjunct, 
                _Info.Clone() as Info, _Cost, Affinity, RequiresMasterwork);

        public static MagicAugment CreateAdjunctAugment(ICasterClass caster, ISpellDef spellDef,
            int spellLevel, int casterLevel, Adjunct adjunct, Info info,
            decimal cost, bool affinity, bool needsMasterWork)
            => new(
                new SpellSource(caster, spellLevel, casterLevel, false, spellDef),
                new AdjunctSlotActivation(adjunct, info, cost, affinity, needsMasterWork));

    }
}
