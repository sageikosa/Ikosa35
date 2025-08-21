using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Magic;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Spells and supernatural powers typically need this to transit their activity</summary>
    [Serializable]
    public class MagicPowerEffect : MagicSourceAuraAdjunct, IAdjunctTracker
    {
        #region Construction
        /// <summary>Spells and supernatural powers typically need this to transit their activity</summary>
        public MagicPowerEffect(MagicPowerActionSource magicSource, ICapabilityRoot root, PowerAffectTracker tracker, int subMode)
            : base(magicSource)
        {
            AnchoredAdjunctObject = null;
            ActiveAdjunctObject = null;
            _SubMode = subMode;
            _Root = root;
            _Tracker = tracker;
            _AllTargets = [];
        }
        #endregion

        #region data
        private int _SubMode;
        private ICapabilityRoot _Root;
        private Collection<AimTarget> _AllTargets;
        private PowerAffectTracker _Tracker;
        #endregion

        public int SubMode => _SubMode;
        public ICapabilityRoot CapabilityRoot
        {
            get => _Root;
            set => _Root = value ?? _Root;
        }

        public PowerAffectTracker PowerTracker
            => _Tracker ??= new PowerAffectTracker();

        public bool IsDismissable
            => CapabilityRoot?.GetCapability<IDurableCapable>()?.IsDismissable(SubMode) ?? false;

        /// <summary>Add multiple targets to the spell effect (such as when porting activity targets)</summary>
        public void AddTargets(IEnumerable<AimTarget> targets)
        {
            foreach (AimTarget _target in targets)
            {
                _AllTargets.Add(_target);
            }
        }

        /// <summary>
        /// Sometimes the adjunct needs additional information specified as targets.
        /// Activity targets are not automatically copied.
        /// </summary>
        public Collection<AimTarget> AllTargets => _AllTargets;

        /// <summary>
        /// Gets the value of the ValueTarget&lt;<typeparamref name="RefType"/>&gt; with the given key
        /// </summary>
        public RefType GetTargetValue<RefType>(string key)
            where RefType : class
            => GetTarget<ValueTarget<RefType>>(key)?.Value;

        /// <summary>
        /// Gets the value of the first ValueTarget&lt;<typeparamref name="RefType"/>&gt;
        /// </summary>
        public RefType GetTargetValue<RefType>()
            where RefType : class
            => _AllTargets.OfType<ValueTarget<RefType>>()
            .FirstOrDefault()?.Value;

        /// <summary>
        /// Gets the value of the ValueTarget&lt;<typeparamref name="RefType"/>&gt; with the given key, where the type is a value-type
        /// </summary>
        public StrucType GetTargetValue<StrucType>(string key, StrucType notFound)
            where StrucType : struct
            => GetTarget<ValueTarget<StrucType>>(key)?.Value ?? notFound;

        /// <summary>
        /// Gets the first <typeparamref name="Target"/> with the given key
        /// </summary>
        public Target GetTarget<Target>(string key)
            where Target : AimTarget
            => _AllTargets.OfType<Target>()
            .FirstOrDefault(_vt => _vt.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

        /// <summary>object related to the spell adjunct (defined during activation, used when de-activating)</summary>
        public object ActiveAdjunctObject { get; private set; }

        /// <summary>Object related to the spell adjunt (defined during anchoring, used when de-anchoring)</summary>
        public object AnchoredAdjunctObject { get; private set; }

        internal void ClearAnchoredAdjunctObject()
        {
            AnchoredAdjunctObject = null;
        }

        #region protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            var _anchorMode = CapabilityRoot.GetCapability<IDurableAnchorCapable>();
            if (_anchorMode != null)
            {
                if (oldAnchor == null)
                {
                    AnchoredAdjunctObject = _anchorMode.OnAnchor(this, Anchor);
                }
                else
                {
                    _anchorMode.OnEndAnchor(this, oldAnchor);
                }
            }
        }
        #endregion

        protected override void OnActivate(object source)
        {
            ActiveAdjunctObject = CapabilityRoot.GetCapability<IDurableCapable>().Activate(this, Anchor, SubMode, source);
        }

        protected override void OnDeactivate(object source)
        {
            CapabilityRoot.GetCapability<IDurableCapable>().Deactivate(this, Anchor, SubMode, source);
        }

        /// <summary>
        /// Gets the first target in the target array specified by the key
        /// </summary>
        /// <param name="key">target key</param>
        /// <returns>AimTarget for the key</returns>
        public AimTarget FirstTarget(string key)
            => AllTargets.Where(t => t.Key == key).First();

        public override object Clone()
            => new MagicPowerEffect(MagicPowerActionSource, CapabilityRoot, PowerTracker, SubMode);

        public static IEnumerable<MagicPowerEffect> GetMagicPowerEffects<Target>(IAdjunctable anchor)
            => anchor.Adjuncts.OfType<MagicPowerEffect>().Where(_mpe => _mpe.ActiveAdjunctObject is Target);
    }
}
