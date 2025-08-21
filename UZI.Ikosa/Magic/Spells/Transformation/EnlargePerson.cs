using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Creatures.Types;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Visualize;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class EnlargePerson : SpellDef, ISpellMode, IDurableCapable, ISaveCapable, ICounterDispelCapable
    {
        public override string DisplayName => @"Enlarge Person";
        public override string Description => @"Humanoid size increases";
        public override MagicStyle MagicStyle => new Transformation();

        public override ActionTime ActionTime => new ActionTime(Round.UnitFactor);

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new MaterialComponent();
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
            yield return new AwarenessAim(@"Creature", @"Creature", FixedRange.One, FixedRange.One, new NearRange(), new CreatureTypeTargetType<HumanoidType>());
            yield break;
        }

        public bool AllowsSpellResistance { get { return true; } }
        public bool IsHarmless { get { return false; } }

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            if ((deliver.TargetingProcess.Targets[0].Target is Creature _critter) && typeof(HumanoidType).IsAssignableFrom(_critter.CreatureType.GetType()))
            {
                SpellDef.DeliverDurable(deliver, deliver.TargetingProcess.Targets[0], 0);
            }
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }
        #endregion

        #region IDurableMode Members
        public IEnumerable<int> DurableSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            var _enlarge = new EnlargeEffect(source);
            target.AddAdjunct(_enlarge);
            return _enlarge;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            if (source.ActiveAdjunctObject is EnlargeEffect _enlarge)
            {
                target.RemoveAdjunct(_enlarge);
            }
        }

        public bool IsDismissable(int subMode) { return true; }
        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode) { return @"Save.Fortitude"; }
        public DurationRule DurationRule(int subMode) { return new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute(), 1)); }
        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            yield break;
        }
        #endregion

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource,Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Fortitude, SaveEffect.Negates, SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion

        #region ICounterDispelMode Members
        public IEnumerable<Type> CounterableSpells
        {
            get
            {
                yield return typeof(ReducePerson);
                yield break;
            }
        }

        public IEnumerable<Type> DescriptorTypes { get { yield break; } }
        #endregion
    }

    [Serializable]
    public class EnlargeEffect : Adjunct, IMonitorChange<ICoreObject>
    {
        #region Construction
        public EnlargeEffect(object source)
            : base(source)
        {
            _Order = new Delta(1, typeof(Uzi.Ikosa.Size));
            _Str = new Delta(2, typeof(Uzi.Ikosa.Deltas.Size));
            _Dex = null;
            _Reach = null;
            _Adjustable = [];
        }
        #endregion

        #region Private Data
        private Delta _Str;
        private Delta _Dex;
        private Delta _Order;
        private Delta _Reach;
        #endregion

        /// <summary>Inventory of manufactured weapons in creatures possession when enlarged</summary>
        private Collection<Guid> _Adjustable;

        #region protected override void OnAnchorSet(IAdjunctable oldAnchor)
        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            if (oldAnchor == null)
            {
                // was just added
                var _critter = Anchor as Creature;

                // get items that can be adjusted (in object load at anchorage of effect)
                _Adjustable.Clear();
                foreach (ISizable _item in from _iCore in _critter.ObjectLoad.AllLoadedObjects()
                                         let _type = _iCore.GetType()
                                         where typeof(ISizable).IsAssignableFrom(_type)
                                         select _iCore as ISizable)
                {
                    _Adjustable.Add(_item.ID);
                }

                // monitor object load changes (for dropped weapons)
                _critter.ObjectLoad.AddChangeMonitor(this);
            }
            else
            {
                // was just removed
                var _critter = oldAnchor as Creature;
                _critter.ObjectLoad.RemoveChangeMonitor(this);
            }
            base.OnAnchorSet(oldAnchor, oldSetting);
        }
        #endregion

        protected override void OnActivate(object source)
        {
            var _critter = Anchor as Creature;

            // body size
            _critter.Body.Sizer.SizeOffset.Deltas.Add(_Order);
            _critter.Weight *= 8;
            _critter.Height *= 2;
            _critter.Width *= 2;
            _critter.Length *= 2;
            if ((Locator.FindFirstLocator(_critter) is ObjectPresenter _loc)
                && (_loc.Chief.ID == _critter.ID))
            {
                _loc.Relocate(new Cubic(_loc.Location, new GeometricSize(_critter.GeometricSize)), _loc.PlanarPresence);
            }

            // size deltas (Dex never falls below 1)
            _critter.Abilities.Strength.Deltas.Add(_Str);
            if (_critter.Abilities.Dexterity.EffectiveValue >= 3)
            {
                _Dex = new Delta(-2, typeof(Uzi.Ikosa.Deltas.Size));
                _critter.Abilities.Dexterity.Deltas.Add(_Dex);
            }
            else if (_critter.Abilities.Dexterity.EffectiveValue > 1)
            {
                _Dex = new Delta(-1, typeof(Uzi.Ikosa.Deltas.Size));
                _critter.Abilities.Dexterity.Deltas.Add(_Dex);
            }
            else
            {
                _Dex = null;
            }

            // reach
            if (_critter.Body.Sizer.Size.Order > 0)
            {
                _Reach = new Delta(1, typeof(Uzi.Ikosa.Deltas.Size));
                _critter.Body.ReachSquares.Deltas.Add(_Reach);
            }

            // adjust all items in adjust items, revert when unloaded
            foreach (ICoreObject _obj in _critter.ObjectLoad.AllLoadedObjects())
            {
                if (_Adjustable.Contains(_obj.ID))
                {
                    if (_obj is ISizable _sizer)
                    {
                        // TODO: adjust weight of adjusted objects
                        _sizer.Sizer.SizeOffset.Deltas.Add(_Order);
                    }
                }
            }
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            var _critter = Anchor as Creature;

            // body size
            _Order.DoTerminate();
            _critter.Weight /= 8;
            _critter.Height /= 2;
            _critter.Width /= 2;
            _critter.Length /= 2;
            if ((Locator.FindFirstLocator(_critter) is ObjectPresenter _loc)
                && (_loc.Chief.ID == _critter.ID))
            {
                _loc.Relocate(new Cubic(_loc.Location, new GeometricSize(_critter.GeometricSize)), _loc.PlanarPresence);
            }

            // terminate size deltas
            _Str.DoTerminate();
            if (_Dex != null)
            {
                _Dex.DoTerminate();
            }

            if (_Reach != null)
            {
                _Reach.DoTerminate();
            }

            // not enlarged...
            base.OnDeactivate(source);
        }

        #region IMonitorChange<ICoreObject> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<ICoreObject> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<ICoreObject> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<ICoreObject> args)
        {
            if (args.Action.Equals(@"Remove", StringComparison.OrdinalIgnoreCase))
            {
                // an adjustable object that leaves the object load is reverted to normal size
                if (args.NewValue is ISizable _sizer)
                {
                    _sizer.Sizer.SizeOffset.Deltas.Remove(_Order);
                }

                // an adjustable object that leaves the object load is removed from the adjustable list
                if (_Adjustable.Contains(args.NewValue.ID))
                {
                    _Adjustable.Remove(args.NewValue.ID);
                }
            }
        }
        #endregion

        public override object Clone()
        {
            return new EnlargeEffect(Source);
        }
    }
}
