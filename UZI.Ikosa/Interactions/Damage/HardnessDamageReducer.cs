using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>May be used by constructs (especially animated objects) to reduce damage</summary>
    [Serializable]
    public class HardnessDamageReducer : IInteractHandler
    {
        public void HandleInteraction(Interaction workSet)
        {
            // figure out where we get the hardness
            var _hardness = 0;
            if (workSet.Target is Creature)
            {
                _hardness = (workSet.Target as Creature).Body.BodyMaterial.Hardness;
            }

            // alright, got something
            if (_hardness > 0)
            {
                // alter damage by subtracting hardness from each "unitary" damage set
                if (workSet.InteractData is IDeliverDamage _damage)
                {
                    // and if anything is left, compensate for instantaneous damage
                    var _instant = _damage.GetLethal();
                    if (_instant > 0)
                    {
                        _damage.Damages.Add(new DamageData(0 - Math.Min(_hardness, _instant), false, @"Hardness", -1));
                    }
                }
            }
        }

        #region public IEnumerable<Type> GetInteractionTypes()
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(DeliverDamageData);
            yield return typeof(SaveableDamageData);
            yield break;
        }
        #endregion

        #region public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // this should be before the standard creature damage handler
            if (existingHandler is TempHPDamageHandler)
                return true;

            return false;
        }
        #endregion
    }
}
