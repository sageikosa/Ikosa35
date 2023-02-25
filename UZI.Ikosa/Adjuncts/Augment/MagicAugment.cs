using System;
using Uzi.Ikosa.Magic;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// Used by magical effects infused into an item as an augmentation.
    /// Expresses an aura, helps anchor an augmentation, and controls augmentation activation.
    /// </summary>
    [Serializable]
    public class MagicAugment : MagicSourceAuraAdjunct
    {
        public MagicAugment(MagicPowerActionSource source, Adjunct augmentation)
            : base(source)
        {
            _Augment = augmentation;
        }

        #region data
        private Adjunct _Augment;
        #endregion

        public Adjunct Augmentation => _Augment;

        #region public override bool CanAnchor(IAdjunctable newAnchor)
        public override bool CanAnchor(IAdjunctable newAnchor)
        {
            if (!Augmentation.CanAnchor(newAnchor))
                return false;

            if (Augmentation is IRequiresEnhancement)
            {
                if ((Augmentation as IRequiresEnhancement).RequiresEnhancement && !newAnchor.IsEnhanced())
                {
                    // augmentation requires enhancment, but the item isn't enhanced
                    return false;
                }
            }

            // enhancement total tracking
            var _delta = (Augmentation as IEnhancementCost)?.TrackedDelta;
            if ((_delta?.Value ?? 0) > 0)
            {
                if (newAnchor is IEnhancementTracker _tracker)
                {
                    try
                    {
                        // temporarily add delta in
                        _tracker.TotalEnhancement.Deltas.Add(_delta);
                        if (_tracker.TotalEnhancement.EffectiveValue > 10)
                        {
                            // cannot exceed 10
                            return false;
                        }
                    }
                    finally
                    {
                        // remove temporary addition
                        _tracker.TotalEnhancement.Deltas.Remove(_delta);
                    }
                }
            }

            // anchor must be masterwork for enhancement
            if (newAnchor is IItemBase _iItem)
            {
                // if no explicit requirements, assume masterwork needed
                if ((Augmentation as IItemRequirements)?.RequiresMasterwork ?? true)
                    return _iItem.IsMasterwork();

                // didn't require masterwork, so good to go!
                return true;
            }
            else
            {
                // weaponhead is not an Item
                if (newAnchor is IWeaponHead _head)
                {
                    // natural weapons can be enhanced (usually +0)
                    return (_head.ContainingWeapon is NaturalWeapon) || _head.ContainingWeapon.IsMasterwork();
                }
            }

            // cannot augment
            return false;
        }
        #endregion

        #region public override bool CanUnAnchor()
        public override bool CanUnAnchor()
        {
            // only have to check if the augmentation is enhanced?
            return Augmentation.CanUnAnchor();
        }
        #endregion

        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            // enhancement total tracking
            var _delta = (Augmentation as IEnhancementCost)?.TrackedDelta ?? null;

            base.OnAnchorSet(oldAnchor, oldSetting);

            #region remove old anchor
            if (oldAnchor != null)
            {
                oldAnchor.RemoveAdjunct(Augmentation);

                // delta cost
                if ((_delta?.Value ?? 0) > 0)
                {
                    (oldAnchor as IEnhancementTracker)?.TotalEnhancement.Deltas.Remove(_delta);
                }
            }
            #endregion

            #region add new anchor
            if (Anchor != null)
            {
                Augmentation.InitialActive = false;
                base.Anchor.AddAdjunct(Augmentation);
                if ((_delta?.Value ?? 0) > 0)
                {
                    // track enhancement when anchor added
                    (Anchor as IEnhancementTracker)?.TotalEnhancement.Deltas.Add(_delta);
                }
            }
            #endregion
        }

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            Augmentation.IsActive = true;
        }

        protected override void OnDeactivate(object source)
        {
            base.OnDeactivate(source);
            Augmentation.IsActive = false;
        }

        public override object Clone()
            => new MagicAugment(MagicPowerActionSource, Augmentation.Clone() as Adjunct);

        public override bool Equals(Adjunct other)
        {
            if (other is MagicAugment _augment)
            {
                return Augmentation.Equals(_augment.Augmentation);
            }
            return false;
        }
    }

    /// <summary>Used to explicitly define item requirements, or override default behavior</summary>
    public interface IItemRequirements
    {
        /// <summary>Usually masterwork is required, implement and override to alter this requirement</summary>
        bool RequiresMasterwork { get; }
    }

    public interface IAugmentationCost
    {
        /// <summary>standard cost</summary>
        decimal StandardCost { get; }

        /// <summary>true if this has affinity with its item's slot</summary>
        bool Affinity { get; }

        /// <summary>non-null if this belongs to a similarty group, in which case it is the key for similar augmentations</summary>
        string SimilarityKey { get; }
    }
}
