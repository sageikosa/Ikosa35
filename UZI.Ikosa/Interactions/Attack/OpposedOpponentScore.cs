using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Shields;
using Uzi.Ikosa.Items.Weapons;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class OpposedOpponentScore : ISupplyQualifyDelta
    {
        public OpposedOpponentScore(int score, Creature critter, ICoreObject coreObj, bool isSunder)
        {
            _Score = new ConstDeltable(score);
            Score.Deltas.Add(new SoftQualifiedDelta(this));
            _Critter = critter;
            _CoreObj = coreObj;
            _IsSunder = isSunder;
        }

        #region state
        private ConstDeltable _Score;
        private Creature _Critter;
        private ICoreObject _CoreObj;
        private bool _IsSunder;
        #endregion

        public IDeltable Score => _Score;

        private int GetHoldingDelta()
        {
            // TODO: item connected to locked gauntlets...+10
            // TODO: poorly-secured must be an adjunct...otherwise assume properly secured...

            if (_CoreObj is ISlottedItem)
            {
                // melee weapon? --> wield template
                if ((_CoreObj is IMeleeWeapon _melee) && _melee.IsActive)
                {
                    return _melee.GetWieldTemplate().OpposedDelta();
                }
                if (_IsSunder && (_CoreObj is IShield _shield))
                {
                    return _shield.OpposedDelta;
                }
            }
            else
            {
                // TODO: spiked gauntlets are part of armor and cannot be disarmed
            }

            // if all else fails, -4
            return _IsSunder ? 0 : -4;
        }

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            foreach (var _delta in _Critter.OpposedDeltable.QualifiedDeltas(qualify, this, @"Opposed"))
            {
                yield return _delta;
            }

            var _holdDelta = GetHoldingDelta();
            if (_holdDelta != 0)
            {
                yield return new QualifyingDelta(_holdDelta, typeof(WieldTemplate), @"Object Attendence");
            }
            yield break;
        }
    }
}
