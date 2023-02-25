using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class Poison : ItemBase, IActionProvider, IConsumableItem, IAppliableItem, IFeedableItem
    {
        #region construction
        public Poison(string name, PoisonDamage primaryDamage, PoisonDamage secondaryDamage,
            ActivationMethod method, MaterialForm form, IVolatileValue difficulty,
            Guid? sourceID = null, double? saveImmuneSpan = null)
            : base(form == MaterialForm.Powder
            ? @"Packet of Powder"
            : (form == MaterialForm.Pellet
            ? @"Pellet"
            : @"Vial of Liquid"), Size.Miniature)
        {
            Name = name;

            // setup damages
            PrimaryDamage = primaryDamage;
            SecondaryDamage = secondaryDamage;

            Activation = method;
            Form = form;
            Difficulty = difficulty;

            SourceID = sourceID;
            SaveImmuneSpan = saveImmuneSpan;
        }
        #endregion

        public enum ActivationMethod { Contact, Inhaled, Ingestion, Injury };
        public enum MaterialForm { Powder, Liquid, Pellet };

        public PoisonDamage PrimaryDamage { get; private set; }
        public PoisonDamage SecondaryDamage { get; private set; }

        public MaterialForm Form { get; private set; }
        public ActivationMethod Activation { get; private set; }
        public IVolatileValue Difficulty { get; private set; }

        public Guid? SourceID { get; private set; }
        public double? SaveImmuneSpan { get; private set; }

        // TODO: actions for liquid, powder, or pellet

        #region IActionProvider Members

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // TODO: liquid action similar to potions
            // TODO: powder is a useful component for traps and food, drink additive
            // TODO: pellet is useful for food or water pollution
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
        {
            return GetInfoData.GetInfoFeedback(this, budget.Actor);
        }

        #endregion

        #region IConsumableItem Members

        public CoreStep DoConsume(CoreActivity activity)
        {
            // TODO: poison self
            return null;
        }

        public IEnumerable<AimingMode> ConsumptionAimingMode(CoreActivity activity)
        {
            // TODO: self-target...needed?
            yield break;
        }

        #endregion

        #region IAppliableItem Members

        public CoreStep DoApply(CoreActivity activity)
        {
            // TODO: apply poison to a weapon (if liquid)
            return null;
        }

        public IEnumerable<AimingMode> ApplicationAimingMode(CoreActivity activity)
        {
            // TODO: weapon head target
            yield break;
        }
        #endregion

        #region IFeedableItem Members

        public CoreStep DoFeed(CoreActivity activity)
        {
            // TODO: poison a creature
            return null;
        }

        public IEnumerable<AimingMode> FeedableAimingMode(CoreActivity activity)
        {
            // TODO: melee attack
            yield break;
        }

        #endregion

        #region IActionSource Members
        public int ActionLevel
            => 1;
        #endregion

        protected override string ClassIconKey
            => @"potion";

        public IEnumerable<PoisonRoll> GetPrimaryRollers()
            => PrimaryDamage.GetRollers();

        public IEnumerable<PoisonRoll> GetSecondaryRollers()
            => SecondaryDamage.GetRollers();

        public IEnumerable<DamageInfo> ApplyPrimary(CoreStep step, Creature critter)
            => PrimaryDamage.ApplyDamage(this, step, critter);

        public IEnumerable<DamageInfo> ApplyPrimary(CoreStep step, Creature critter, int[] rollValue)
            => PrimaryDamage.ApplyDamage(this, step, critter, rollValue);

        public IEnumerable<DamageInfo> ApplySecondary(CoreStep step, Creature critter)
            => SecondaryDamage.ApplyDamage(this, step, critter);

        public IEnumerable<DamageInfo> ApplySecondary(CoreStep step, Creature critter, int[] rollValue)
            => SecondaryDamage.ApplyDamage(this, step, critter, rollValue);
    }

    public interface IPoisonProvider
    {
        Poison GetPoison();
        string Name { get; }
    }
}