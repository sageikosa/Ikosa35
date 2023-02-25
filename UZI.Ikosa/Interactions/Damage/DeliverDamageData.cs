using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// Set of damage-values to deliver as a unit, ensures a minimum of 1.
    /// </summary>
    [Serializable]
    public class DeliverDamageData : InteractData, IDeliverDamage
    {
        public DeliverDamageData(CoreActor actor, IEnumerable<DamageData> damages, bool isContinuous, bool isCriticalHit)
            : base(actor)
        {
            Damages = damages.ToList();
            this.EnsureMinimum();
            IsContinuous = isContinuous;
            IsCriticalHit = isCriticalHit;
            Secondaries = new List<ISecondaryAttackResult>();
        }

        public List<DamageData> Damages { get; private set; }
        public bool IsContinuous { get; private set; }
        public bool IsCriticalHit { get; private set; }
        public List<ISecondaryAttackResult> Secondaries { get; private set; }

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            if (target is Creature)
            {
                yield return CreatureDamageHandler.Static;
            }
            else if ((target is IAmmunitionBundle) && !(target is IAmmunitionContainer))
            {
                yield return AmmunitionBundleDamageHandler.Static;
            }
            else if (target is Trove)
            {
                yield return TroveDamageHandler.Static;
            }
            else if (target is IStructureDamage)
            {
                yield return ObjectDamageHandler.Static;
            }
            yield break;
        }

        public InteractData GetClone()
            => new DeliverDamageData(null, Damages.Select(_d => _d.Clone() as DamageData), IsContinuous, IsCriticalHit);
    }
}
