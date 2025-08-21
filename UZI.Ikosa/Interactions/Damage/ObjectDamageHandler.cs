using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class ObjectDamageHandler : IInteractHandler
    {
        public static readonly ObjectDamageHandler Static = new ObjectDamageHandler();

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(DeliverDamageData);
            yield return typeof(SaveableDamageData);
            yield break;
        }

        public void HandleInteraction(Interaction workSet)
        {
            // collect from damage
            if ((workSet?.Target is IStructureDamage _structure)
                && (workSet?.InteractData is IDeliverDamage _damage))
            {
                // effective hardness adjustments (opt-in to use anything but standard hardness)
                var _alters = workSet.InteractData.Alterations.OfType<ObjectDamageHardnessAlteration>().ToList();
                var _hardness = _structure.GetHardness();
                int _effectiveHardness(EnergyType? energy)
                    => (from _a in _alters
                        let _h = _a.GetHardness(_hardness, energy)
                        where _h != null
                        select _h).FirstOrDefault() ?? _hardness;

                // effective energy filtering (default handler if nothing else)
                int _energyDamage(EnergyType energy, int damage)
                    => (new ObjectEffectiveEnergyDamage(energy, damage)).GetEffectiveEnergyDamage(workSet);

                // handle non-energy
                var _neDmg = _damage.GetLethalNonEnergy();
                // TODO: general falling damage resistance?

                // ranged attacks from projectile weapons take half damage
                if (((workSet as StepInteraction)?.Step is AttackResultStep _atkRslt)
                    && _atkRslt.IsRangedAttack
                    && ((_atkRslt.AttackSource as IWeaponHead)?.ContainingWeapon is IProjectileWeapon))
                {
                    _neDmg /= 2;
                }

                _neDmg -= _effectiveHardness(null);
                if (_neDmg > 0)
                {
                    _structure.StructurePoints -= _neDmg;
                }

                // handle all energies
                foreach (var _energy in _damage.GetEnergyTypes())
                {
                    // effective energy damage less effective energy hardness
                    var _eDmg = _energyDamage(_energy, _damage.GetEnergy(_energy)) - _effectiveHardness(_energy);
                    if (_eDmg > 0)
                    {
                        _structure.StructurePoints -= _eDmg;
                    }
                }

                workSet.Feedback.Add(new UnderstoodFeedback(this));
            }
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // this should be before the standard creature damage handler (if added for a creature)
            if (existingHandler is CreatureDamageHandler)
            {
                return true;
            }

            return false;
        }
    }
}
