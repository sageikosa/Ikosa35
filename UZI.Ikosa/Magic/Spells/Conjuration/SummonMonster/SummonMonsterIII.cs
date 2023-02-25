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
    public class SummonMonsterIII : SummonMonsterII
    {
        public override IEnumerable<ISpellMode> SpellModes
            => base.SpellModes.Union(new SummonMonsterMode(this, SummonSubMode.SeveralLeast).ToEnumerable());

        public override string DisplayName => @"Summon Monster III";

        public override Creature GetCreature(CoreActor actor, OptionAimOption option, string prefix, int index)
        {
            // TODO: options from Monster III list, otherwise, pass to Monster II
            return base.GetCreature(actor, option, prefix, index);
        }

        protected override IPowerDef NewForPowerSource()
            => new SummonMonsterIII();

        public override IEnumerable<OptionAimOption> GetCreatures(CoreActor actor, SummonMonsterMode monsterMode)
            => monsterMode.SubMode switch
            {
                SummonSubMode.SeveralLeast => GetMonsterList1(actor),
                SummonSubMode.FewLesser => GetMonsterList2(actor),
                _ => GetMonsterList3(actor)
            };

        protected IEnumerable<OptionAimOption> GetMonsterList3(CoreActor coreActor)
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
