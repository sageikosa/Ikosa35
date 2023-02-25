using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public abstract class MetaMagicSpellMode : ISpellMode
    {
        #region Construction
        protected MetaMagicSpellMode(ISpellMode wrapped)
        {
            _Wrapped = wrapped;
        }
        #endregion

        private ISpellMode _Wrapped;
        public ISpellMode Wrapped => _Wrapped;

        /// <summary>True if any wrapper from this to the underlying ISpellMode matches the supplied metamagicwrapper</summary>
        public bool HasWrapper<MetaWrap>() where MetaWrap : MetaMagicSpellMode
        {
            // if this wrapper is the correct type, return true
            if (GetType() == typeof(MetaWrap))
                return true;

            // if this is wrapping a metamagic wrapper itself, dig further in
            MetaMagicSpellMode _wrapper = _Wrapped as MetaMagicSpellMode;
            return ((_wrapper != null) && _wrapper.HasWrapper<MetaWrap>());
        }

        public string DisplayName => _Wrapped.DisplayName;
        public string Description => _Wrapped.Description;
        public virtual IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode) => _Wrapped.AimingMode(actor, mode);
        public virtual bool AllowsSpellResistance => _Wrapped.AllowsSpellResistance;
        public bool IsHarmless => _Wrapped.IsHarmless;

        public virtual void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            _Wrapped.ActivateSpell(deliver);
        }

        public virtual void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            _Wrapped.ApplySpell(apply);
        }

        public virtual IMode GetCapability<IMode>() where IMode : class, ICapability
            => _Wrapped.GetCapability<IMode>();
    }
}
