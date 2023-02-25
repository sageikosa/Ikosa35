using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class SummonMonsterII : SummonMonsterI
    {
        public override IEnumerable<ISpellMode> SpellModes
            => base.SpellModes.Union(new SummonMonsterMode(this, SummonSubMode.FewLesser).ToEnumerable());

        public override string DisplayName => @"Summon Monster II";
        public override string Description => @"Summon creature to fight for you, or multiple lesser creatures";

        public override Creature GetCreature(CoreActor actor, OptionAimOption option, string prefix, int index)
        {
            // TODO: options from Monster II list, otherwise, pass to Monster I
            return base.GetCreature(actor, option, prefix, index);
        }

        protected override IPowerDef NewForPowerSource()
            => new SummonMonsterII();

        public override IEnumerable<OptionAimOption> GetCreatures(CoreActor actor, SummonMonsterMode monsterMode)
            => monsterMode.SubMode switch
            {
                SummonSubMode.SeveralLeast => GetMonsterList1(actor),
                SummonSubMode.FewLesser => GetMonsterList1(actor),
                _ => GetMonsterList2(actor)
            };

        protected IEnumerable<OptionAimOption> GetMonsterList2(CoreActor coreActor)
        {
            var _creature = coreActor as Creature;
            var _casterClasses = _creature.Classes.OfType<ICasterClass>().ToList();
            var _law = new Lawful();
            var _chaos = new Chaotic();
            var _good = new Good();
            var _evil = new Evil();
            yield break;
        }
    }
}
