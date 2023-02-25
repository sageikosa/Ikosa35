using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class AidSpell : SpellDef, IDurableCapable, ISpellMode, IDurableAnchorCapable
    {
        public override string DisplayName => @"Aid";
        public override string Description => @"+1 Moral on attack and saves versus fear.  1d8+level temporary health";
        public override MagicStyle MagicStyle => new Enchantment(Enchantment.SubEnchantment.Compulsion);

        public override IEnumerable<Descriptor> Descriptors => (new MindAffecting()).ToEnumerable();
        public override IEnumerable<ISpellMode> SpellModes => this.ToEnumerable();

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

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new FocusComponent();
                yield break;
            }
        }
        #endregion

        #region IDurableAnchorMode Members
        public object OnAnchor(IAdjunctTracker source, IAdjunctable target)
        {
            if ((target is Creature _critter)
                && (source is MagicPowerEffect _power))
            {
                // calculate hp value from roll and caster level
                var _hpVal = Math.Min(_power.CasterLevel, 10) + _power.GetTargetValue<int>(@"TempHP", 1);

                // create, attach and return aid
                var _aid = new AidSpellEffect(source, _hpVal)
                {
                    InitialActive = false
                };
                _critter.AddAdjunct(_aid);
                return _aid;
            }
            return null;
        }

        public void OnEndAnchor(IAdjunctTracker source, IAdjunctable target)
        {
            (source.AnchoredAdjunctObject as AidSpellEffect)?.Eject();
        }
        #endregion

        #region IDurableMode Members
        public IEnumerable<int> DurableSubModes => 0.ToEnumerable();

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            // enable anchored aid
            if (source.AnchoredAdjunctObject is AidSpellEffect _aid)
                _aid.IsActive = true;
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            // disable anchored aid
            if (source.AnchoredAdjunctObject is AidSpellEffect _aid)
                _aid.IsActive = false;
        }

        public bool IsDismissable(int subMode) => false;
        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode) => string.Empty;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact) { yield break; }
        #endregion

        #region ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new TouchAim(@"Creature", @"Living Creature", Lethality.AlwaysNonLethal,
                20, this, new FixedRange(1), new FixedRange(1), new MeleeRange(), new LivingCreatureTargetType());
            yield return new RollAim(@"TempHP", @"Temporary Health", DieRoller.CreateRoller(StandardDieType.d8));
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurableToTouch(deliver, @"Creature", 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }
        #endregion
    }

    [Serializable]
    public class AidSpellEffect : BlessEffect
    {
        public AidSpellEffect(object source, int tempHP)
            : base(source)
        {
            _Amount = new Delta(tempHP, this, @"Aid");
        }

        #region state
        protected Delta _Amount;
        protected TempHPChunk _Chunk;
        #endregion

        #region temp HP maintenance
        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            // temp HP amount survives activation/deactivation
            if (Anchor != null)
            {
                if (Anchor is Creature _critter)
                {
                    _Chunk = new TempHPChunk(_critter.TempHealthPoints, _Amount);
                    _critter.TempHealthPoints.Add(_Chunk);
                }
            }
            else
            {
                if (oldAnchor is Creature _critter)
                {
                    _critter.TempHealthPoints.Remove(_Chunk);
                }
            }
            base.OnAnchorSet(oldAnchor, oldSetting);
        }
        #endregion

        public override object Clone()
            => new AidSpellEffect(Source, _Amount.Value);
    }
}
