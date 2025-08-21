using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class PoisonMultiDamage : PoisonDamage
    {
        public PoisonMultiDamage(params PoisonDamage[] damages)
            : base()
        {
            Damages = damages;
        }

        public PoisonDamage[] Damages { get; private set; }
        public override string Name => string.Join(@" + ", (from _dmg in Damages select _dmg.Name).ToArray());

        public override IEnumerable<DamageInfo> ApplyDamage(object source, CoreStep step, Creature critter)
        {
            var _build = new StringBuilder();
            foreach (var _dmg in Damages)
            {
                foreach (var _info in _dmg.ApplyDamage(source, step, critter))
                {
                    yield return _info;
                }
            }
            yield break;
        }

        public override IEnumerable<DamageInfo> ApplyDamage(object source, CoreStep step, Creature critter, int[] rollValue)
        {
            var _build = new StringBuilder();
            for (var _px = 0; _px < Damages.Length; _px++)
            {
                foreach (var _info in Damages[_px].ApplyDamage(source, step, critter, new int[] { rollValue[_px] }))
                {
                    yield return _info;
                }
            }
            yield break;
        }

        public override IEnumerable<PoisonRoll> GetRollers()
        {
            foreach (var _pRoll in from _dmg in Damages
                                   from _roll in _dmg.GetRollers()
                                   select _roll)
            {
                yield return _pRoll;
            }
            yield break;
        }
    }
}