using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class MagicNaturalWeaponsTrait : TraitEffect, IProcessFeedback
    {
        public MagicNaturalWeaponsTrait(ITraitSource traitSource, IMagicPowerSource powerSource, int amount)
            : base(traitSource)
        {
            _PowerSource = powerSource;
            _Seed = 0;
            _NatrlWpnEnh = [];
        }

        #region data
        private IMagicPowerSource _PowerSource;
        private int _Seed;
        private List<Enhanced> _NatrlWpnEnh;
        #endregion

        public IMagicPowerSource MagicPowerSource => _PowerSource;
        public int Amount => _Seed;

        protected override void OnActivate(object source)
        {
            _NatrlWpnEnh.Clear();

            // only use natural weapons that have natural weapon traits associated with them
            foreach (var _natrl in Creature.Adjuncts.OfType<NaturalWeaponTrait>())
            {
                var _enh = new Enhanced(MagicPowerSource, Amount);
                _NatrlWpnEnh.Add(_enh);
                _natrl.Weapon.MainHead.AddAdjunct(_enh);
            }

            // need to check future adds/removes
            Creature.AddIInteractHandler(this);
            base.OnActivate(source);
        }

        public static SuperNaturalTrait GetMagicNaturalWeaponsTrait(ITraitSource traitSource, int amount, IPowerClass powerClass)
        {
            // damage Reduction
            var _def = new SuperNaturalPowerDef(@"Magically Enhanced Natural Weapons", @"Natural weapons treated as magic to overcome damage reduction", new Evocation());
            var _src = new SuperNaturalPowerSource(powerClass, 1, _def);
            var _dr = new MagicNaturalWeaponsTrait(traitSource, _src, amount);
            return new SuperNaturalTrait(traitSource, _src, TraitCategory.Quality, _dr);
        }

        protected override void OnDeactivate(object source)
        {
            // de-enhance natural weapons
            foreach (var _enh in _NatrlWpnEnh)
            {
                _enh.Eject();
            }
            _NatrlWpnEnh.Clear();

            // no longer checking for future adds/removes
            Creature.RemoveIInteractHandler(this);
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new MagicNaturalWeaponsTrait(TraitSource, MagicPowerSource, Amount);

        public override TraitEffect Clone(ITraitSource species)
            => new MagicNaturalWeaponsTrait(TraitSource, MagicPowerSource, Amount);

        #region IProcessFeedback/IInteractHandler
        void IProcessFeedback.ProcessFeedback(Interaction workSet)
        {
            switch (workSet?.InteractData)
            {
                case AddAdjunctData _add:
                    if (_add.Adjunct is NaturalWeaponTrait _addWpn)
                    {
                        // enhance new natural weapon
                        var _enh = new Enhanced(MagicPowerSource, Amount);
                        _NatrlWpnEnh.Add(_enh);
                        _addWpn.Weapon.MainHead.AddAdjunct(_enh);
                    }
                    break;

                case RemoveAdjunctData _rmv:
                    if (_rmv.Adjunct is NaturalWeaponTrait _rmvWpn)
                    {
                        // de-enhance natural weapon
                        var _remover = _NatrlWpnEnh.FirstOrDefault(_e => _e.Anchor == _rmvWpn.Weapon.MainHead);
                        if (_remover != null)
                        {
                            _remover.Eject();
                            _NatrlWpnEnh.Remove(_remover);
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        void IInteractHandler.HandleInteraction(Interaction workSet)
        {
        }

        IEnumerable<Type> IInteractHandler.GetInteractionTypes()
        {
            yield return typeof(AddAdjunctData);
            yield return typeof(RemoveAdjunctData);
            yield break;
        }

        bool IInteractHandler.LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return true;
        }
        #endregion
    }
}
