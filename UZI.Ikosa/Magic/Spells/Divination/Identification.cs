using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Time;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using System.Linq;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Identification : SpellDef, ISpellMode
    {
        public override MagicStyle MagicStyle { get { return new Divination(Divination.SubDivination.Illumination); } }
        public override string DisplayName { get { return @"Identification"; } }
        public override string Description { get { return @"Provides detailed information about a standard magic item"; } }
        public override ActionTime ActionTime { get { return new ActionTime(Hour.UnitFactor); } }

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new GemMaterialComponent<PearlMaterial>(100m);
                yield return new MaterialComponent();
                yield break;
            }
        }
        #endregion

        #region public override IEnumerable<SpellComponent> DivineComponents { get; }
        public override IEnumerable<SpellComponent> DivineComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new DivineFocusComponent();
                yield break;
            }
        }
        #endregion

        #region public override IEnumerable<ISpellMode> SpellModes { get; }
        public override IEnumerable<ISpellMode> SpellModes
        {
            get
            {
                yield return this;
                yield break;
            }
        }
        #endregion

        #region ISpellMode Members

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new TouchAim(@"Object", @"Object", Lethality.AlwaysNonLethal,
                20, this, FixedRange.One, FixedRange.One, new MeleeRange(), new ObjectTargetType());
            yield break;
        }

        public bool AllowsSpellResistance { get { return false; } }
        public bool IsHarmless { get { return true; } }

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverSpell(deliver, 0, deliver.TargetingProcess.Targets[0]);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            var _target = apply.TargetingProcess.Targets[0].Target as ICoreObject;
            if (!(_target is ISuperiorRelic))
            {
                var _identity = Identity.CreateIdentity(apply.Actor, _target, typeof(Identification));

                // add creatures
                _identity.CreatureIDs.Add(apply.Actor?.ID ?? Guid.Empty, apply.Actor?.Name);
                // TODO: add friendly creatures (within communication range?)

                // add identity to the target for future reference
                _target.AddAdjunct(_identity);

                // provide immediate info
                if (apply.TargetingProcess is CoreActivity _activity)
                {
                    apply.AppendFollowing(_activity.GetActivityResultNotifyStep(_identity.Infos.ToArray()));
                }
            }
            else
            {
                // enqueue information the indicates the item is too powerful to be identified
                if (apply.TargetingProcess is CoreActivity _activity)
                {
                    apply.AppendFollowing(_activity.GetActivityResultNotifyStep(@"Item is too powerful for this spell"));
                }
            }
        }

        #endregion
    }
}