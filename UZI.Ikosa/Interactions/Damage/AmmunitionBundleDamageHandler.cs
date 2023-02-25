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

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class AmmunitionBundleDamageHandler : IInteractHandler
    {
        public static readonly AmmunitionBundleDamageHandler Static = new AmmunitionBundleDamageHandler();

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(DeliverDamageData);
            yield return typeof(SaveableDamageData);
            yield break;
        }

        public void HandleInteraction(Interaction workSet)
        {
            var _rangedAttack = (((workSet as StepInteraction)?.Step is AttackResultStep _atkRslt)
                    && _atkRslt.IsRangedAttack
                    && ((_atkRslt.AttackSource as IWeaponHead)?.ContainingWeapon is IProjectileWeapon));
            void _applyDamage(List<ObjectDamageHardnessAlteration> hardAlters, AmmoEditSet set, IDeliverDamage deliver)
            {
                // min of 1 structure point per ammo
                var _pts = set.Ammunition.StructurePoints;
                if (_pts < 1)
                    _pts = 1;

                // hardness calculator
                var _hardness = set.Ammunition.GetHardness();
                int _effectiveHardness(EnergyType? energy)
                    => (from _a in hardAlters
                        let _h = _a.GetHardness(_hardness, energy)
                        where _h != null
                        select _h).FirstOrDefault() ?? _hardness;

                // effective energy filtering (default handler if nothing else)
                var _workSet = new Interaction(null, workSet.Source, set.Ammunition, deliver as InteractData);
                int _energyDamage(EnergyType energy, int damage)
                     => (new ObjectEffectiveEnergyDamage(energy, damage)).GetEffectiveEnergyDamage(_workSet);

                // handle non-energy
                var _neDmg = (deliver.GetLethalNonEnergy() / (_rangedAttack ? 2 : 1)) - _effectiveHardness(null);
                if (_neDmg > 0)
                {
                    // how many ammo units affected (rounding up)
                    var _qty = _neDmg / _pts;
                    if ((_neDmg % _pts) > 0)
                        _qty++;

                    if (_qty > set.Count)
                    {
                        // set is exhausted (some damage still left?)
                        deliver.Damages.Add(new DamageData(_pts * set.Count * -1, false, @"Hardness", -1));
                        _qty -= set.Count;
                        set.Bundle.ExtractAmmo(set.Ammunition);
                        return;
                    }
                    else
                    {
                        // some still left in this set (damage is exhausted)
                        deliver.Damages.Add(new DamageData(_neDmg * -1, false, @"Hardness", -1));
                        set.Count -= _qty;
                    }
                }

                // handle all energies
                foreach (var _energy in deliver.GetEnergyTypes())
                {
                    // effective energy damage less effective energy hardness
                    var _eDmg = _energyDamage(_energy, deliver.GetEnergy(_energy)) - _effectiveHardness(_energy);
                    if (_eDmg > 0)
                    {
                        // how many ammo units affected (rounding up)
                        var _qty = _eDmg / _pts;
                        if ((_eDmg % _pts) > 0)
                            _qty++;

                        if (_qty > set.Count)
                        {
                            // set is exhausted
                            deliver.Damages.Add(new EnergyDamageData(_pts * set.Count * -1, _energy, @"Hardness", -1));
                            _qty -= set.Count;
                            set.Bundle.ExtractAmmo(set.Ammunition);
                            return;
                        }
                        else
                        {
                            // some still left in this set
                            deliver.Damages.Add(new EnergyDamageData(_neDmg * -1, _energy, @"Hardness", -1));
                            set.Count -= _qty;
                        }
                    }
                }
            }

            // collect from damage
            if ((workSet?.InteractData is IDeliverDamage _damage)
                && (workSet.Target is IAmmunitionBundle _bundle))
            {
                // effective hardness adjustments (opt-in to use anything but standard hardness)
                var _alters = workSet.InteractData.Alterations.OfType<ObjectDamageHardnessAlteration>().ToList();
                if (_damage is SaveableDamageData _saveDamage)
                {
                    // bundle should have already made a save roll...
                    var _bSave = workSet.Target as IProvideSaves;
                    if (_saveDamage.Success(workSet))
                    {
                        // bundle save succeeded, huzzah...
                        foreach (var _dmg in _saveDamage.Damages)
                        {
                            // subtract from each damage
                            _dmg.Amount -= Convert.ToInt32(_dmg.Amount * _saveDamage.SaveFactor);
                        }
                        foreach (var _set in _bundle.AmmoSets.OrderBy(_as => _as.Price).ToList())
                        {
                            _applyDamage(_alters, _set, _saveDamage);

                            // if all damage is exhausted, break
                            if (_saveDamage.GetTotal() <= 0)
                                break;
                        }
                    }
                    else
                    {
                        // bundle save failed, find AmmoSets that failed
                        // any ammoset ammunition that always fails, will fail
                        var _fails = _bundle.AmmoSets.Where(_s => _s.Ammunition.AlwaysFailsSave).ToList();

                        // any ammoset that doesn't always fail tries its own SavingThrow delta
                        var _succeeds = _bundle.AmmoSets.Where(_s => !_s.Ammunition.AlwaysFailsSave).ToList();
                        foreach (var _set in _succeeds.ToList())
                        {
                            var _workset = new Interaction(null, workSet.Source, _set.Ammunition, _saveDamage);
                            if (!_saveDamage.Success(_workset))
                            {
                                // safe to remove since _trySave isn't being iterated over
                                _fails.Add(_set);
                                _succeeds.Remove(_set);
                            }
                        }

                        foreach (var _set in _fails.OrderBy(_as => _as.Price))
                        {
                            _applyDamage(_alters, _set, _saveDamage);

                            // if all damage is exhausted, break
                            if (_saveDamage.GetTotal() <= 0)
                                break;
                        }

                        // only if there are things to do and damage left
                        if (_succeeds.Any() && (_saveDamage.GetTotal() > 0))
                        {
                            foreach (var _dmg in _saveDamage.Damages)
                            {
                                // subtract from each damage
                                _dmg.Amount -= Convert.ToInt32(_dmg.Amount * _saveDamage.SaveFactor);
                            }
                            foreach (var _set in _succeeds.OrderBy(_as => _as.Price))
                            {
                                _applyDamage(_alters, _set, _saveDamage);

                                // if all damage is exhausted, break
                                if (_saveDamage.GetTotal() <= 0)
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    // non-saveable damage...
                    foreach (var _set in _bundle.AmmoSets.OrderBy(_as => _as.Price).ToList())
                    {
                        _applyDamage(_alters, _set, _damage);

                        // if all damage is exhausted, break
                        if (_damage.GetTotal() <= 0)
                            break;
                    }
                }

                // TODO: damage type immunities...
                workSet.Feedback.Add(new UnderstoodFeedback(this));
            }
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return false;
        }
    }
}
