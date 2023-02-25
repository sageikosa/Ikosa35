using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Interactions
{
    public interface IDeliverDamage
    {
        List<DamageData> Damages { get; }
        bool IsContinuous { get; }
        bool IsCriticalHit { get; }
        List<ISecondaryAttackResult> Secondaries { get; }
        InteractData GetClone();
    }

    [Serializable]
    public struct IncidentalDamage
    {
        public int Damage { get; set; }
        public Interaction Incident { get; set; }
    }

    public static class IDeliverDamageHelper
    {
        /// <summary>True if there is incidental damage associated with this activity</summary>
        public static IList<ValueTarget<IncidentalDamage>> GetIncidentalDamages(this CoreActivity activity)
            => activity.Targets.OfType<ValueTarget<IncidentalDamage>>()
                .Where(_vt => _vt.Key == @"Damage.Incidental")
                .ToList();

        public static void EnsureMinimum(this IDeliverDamage self)
        {
            self.CompensateLethalEnergy();
            self.CompensateLethalNonEnergy();
            self.CompensateNonLethal();
        }

        public static int GetLethal(this IDeliverDamage self)
            => Math.Max(0,
                self.Damages.Where(_d => !_d.IsNonLethal)
                .Sum(_d => _d.Amount));

        public static int GetNonLethal(this IDeliverDamage self)
            => Math.Max(0,
                self.Damages.Where(_d => _d.IsNonLethal)
                .Sum(_d => _d.Amount));

        /// <summary>When collecting damages, make sure we get a positive balance</summary>
        private static void CompensateNonLethal(this IDeliverDamage self)
        {
            if (self.Damages.Any(_d => _d.IsNonLethal))
            {
                // due to penalties, we delivered no, or negative damage in a min-group
                foreach (var (_key, _sum) in (from _nld in self.Damages
                                              where _nld.IsNonLethal
                                              group _nld.Amount by _nld.MinGroup
                                              into _mg
                                              select (_mg.Key, Sum: _mg.Sum()))
                                              .Where(_g => _g.Sum <= 0)
                                              .ToList())
                {
                    // add in just enough to reverse the trend and ensure 1 unit per min-group
                    self.Damages.Add(new DamageData((0 - _sum) + 1, true, @"Minimum", _key));
                }
            }
        }

        /// <summary>Total, regardless of type (indicates some effective damage)</summary>
        public static int GetTotal(this IDeliverDamage self)
            => self.GetLethal() + self.GetNonLethal();

        /// <summary>List all energy types in this damage set</summary>
        public static IEnumerable<EnergyType> GetEnergyTypes(this IDeliverDamage self)
            => self.Damages.OfType<EnergyDamageData>().Select(_ed => _ed.Energy).Distinct();

        public static int GetEnergy(this IDeliverDamage self, EnergyType energy)
            => Math.Max(0,
                self.Damages.OfType<EnergyDamageData>()
                .Where(_ed => _ed.Energy == energy)
                .Sum(_ed => _ed.Amount));

        /// <summary>When collecting damages, make sure we get a positive balance</summary>
        private static void CompensateLethalEnergy(this IDeliverDamage self)
        {
            foreach (var _energy in self.GetEnergyTypes())
            {
                if (self.Damages.OfType<EnergyDamageData>().Any(_ed => _ed.Energy == _energy))
                {
                    // due to penalties, we delivered no, or negative damage in a min-group
                    foreach (var (_key, _sum) in (from _ed in self.Damages.OfType<EnergyDamageData>()
                                                  where _ed.Energy == _energy
                                                  group _ed.Amount by _ed.MinGroup
                                                  into _mg
                                                  select (_mg.Key, Sum: _mg.Sum()))
                                                  .Where(_g => _g.Sum <= 0)
                                                  .ToList())
                    {
                        // add in just enough to reverse the trend and ensure 1 unit per min-group
                        self.Damages.Add(new EnergyDamageData((0 - _sum) + 1, _energy, @"Minimum", _key));
                    }
                }
            }
        }

        public static int GetLethalNonEnergy(this IDeliverDamage self)
            => Math.Max(0,
                self.Damages
                .Where(_d => !(_d is EnergyDamageData) && !_d.IsNonLethal)
                .Sum(_d => _d.Amount));

        /// <summary>When collecting damages, make sure we get a positive balance</summary>
        private static void CompensateLethalNonEnergy(this IDeliverDamage self)
        {
            if (self.Damages.Any(_d => !(_d is EnergyDamageData) && !_d.IsNonLethal))
            {
                // due to penalties, we delivered no, or negative damage in a min-group
                foreach (var (_key, _sum) in (from _d in self.Damages
                                              where !(_d is EnergyDamageData) && !_d.IsNonLethal
                                              group _d.Amount by _d.MinGroup
                                              into _mg
                                              select (_mg.Key, Sum: _mg.Sum()))
                                              .Where(_g => _g.Sum <= 0)
                                              .ToList())
                {
                    // add in just enough to reverse the trend and ensure 1 unit per min-group
                    self.Damages.Add(new DamageData((0 - _sum) + 1, false, @"Minimum", _key));
                }
            }
        }
    }
}
