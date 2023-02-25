using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class EnlargeSpellMode : MetaMagicSpellMode
    {
        public EnlargeSpellMode(ISpellMode wrapped)
            : base(wrapped)
        {
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            foreach (var _aim in Wrapped.AimingMode(actor, mode))
            {
                if (_aim is RangedAim _ranged)
                {
                    // double valid approriate ranges
                    var _rType = _ranged.Range.GetType();
                    if (_rType == typeof(NearRange))
                    {
                        _ranged.Range = new NearRange(true);
                    }
                    else if (_rType == typeof(MediumRange))
                    {
                        _ranged.Range = new MediumRange(true);
                    }
                    else if (_rType == typeof(FarRange))
                    {
                        _ranged.Range = new FarRange(true);
                    }
                    yield return _ranged;
                }
                else
                {
                    yield return _aim;
                }
            }
            yield break;
        }
    }
}
