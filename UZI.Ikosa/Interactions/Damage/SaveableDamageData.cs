using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// One source, one save potentially multiple damages.
    /// </summary>
    public class SaveableDamageData : SavingThrowData, IDeliverDamage
    {
        #region ctor()
        public SaveableDamageData(Creature critter, IEnumerable<DamageData> damages,
            SaveMode saveMode, double saveFactor, Deltable saveRoll, bool isContinuous, bool criticalFailDamagesItems)
            : base(critter, saveMode, saveRoll)
        {
            Damages = damages.ToList();
            SaveFactor = saveFactor;
            IsContinuous = isContinuous;
            Secondaries = [];
            CriticalFailDamagesItems = criticalFailDamagesItems;
        }

        public SaveableDamageData(Creature critter, DamageData damage,
            SaveMode saveMode, double saveFactor, Deltable saveRoll, bool isContinuous, bool criticalFailDamagesItems)
            : base(critter, saveMode, saveRoll)
        {
            Damages =
            [
                damage
            ];
            SaveFactor = saveFactor;
            IsContinuous = isContinuous;
            Secondaries = [];
            CriticalFailDamagesItems = criticalFailDamagesItems;
        }
        #endregion

        /// <summary>If a save is made, this is the factor applied to the damage</summary>
        public double SaveFactor { get; set; }

        public List<DamageData> Damages { get; private set; }
        public bool IsContinuous { get; private set; }
        public bool IsCriticalHit => false;
        public List<ISecondaryAttackResult> Secondaries { get; private set; }
        public bool CriticalFailDamagesItems { get; private set; }

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            if (target is Creature)
            {
                yield return CreatureDamageHandler.Static;
            }
            else if ((target is IAmmunitionBundle) && !(target is IAmmunitionContainer))
            {
                yield return ObjectSaveHandler.Static;
                yield return AmmunitionBundleDamageHandler.Static;
            }
            else if (target is Trove)
            {
                yield return ObjectSaveHandler.Static;
                yield return TroveDamageHandler.Static;
            }
            else if (target is IStructureDamage)
            {
                yield return ObjectSaveHandler.Static;
                yield return ObjectDamageHandler.Static;
            }
            yield break;
        }

        public InteractData GetClone()
            => new SaveableDamageData(null, Damages.Select(_d => _d.Clone() as DamageData),
                SaveMode, SaveFactor, new Deltable(SaveRoll.BaseValue), IsContinuous, false);
    }
}
