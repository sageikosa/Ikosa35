using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Skills;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class FreezeTrait : TraitEffect, IProcessFeedback, IMonitorChange<CoreActivity>
    {
        public FreezeTrait(ITraitSource traitSource, Deltable spotDifficulty, ObjectInfo altInfo)
            : base(traitSource)
        {
            _Difficulty = spotDifficulty;
            _Detected = new Dictionary<Guid, bool>();
            _AltInfo = altInfo;
        }

        // TODO: observer observations or actions that can force more spot-checks
        // TODO: observer knowledge sharing...(identity GUIDs?)

        #region data
        private Deltable _Difficulty;
        private Dictionary<Guid, bool> _Detected;
        private ObjectInfo _AltInfo;
        #endregion

        public override string ToString()
            => $@"Spot Difficulty: {SpotDifficulty}";

        public Deltable SpotDifficulty => _Difficulty;
        public ObjectInfo AlternateInfo => _AltInfo; 

        // cloning
        public override TraitEffect Clone(ITraitSource traitSource)
            => new FreezeTrait(traitSource, SpotDifficulty, AlternateInfo);

        public override object Clone()
            => new FreezeTrait(TraitSource, SpotDifficulty, AlternateInfo);

        #region OnActivate()
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            Creature?.AddIInteractHandler(this);
            Creature?.AddChangeMonitor(this);
        }
        #endregion

        #region OnDeactivate()
        protected override void OnDeactivate(object source)
        {
            Creature?.RemoveChangeMonitor(this);
            Creature?.RemoveIInteractHandler(this);
            base.OnDeactivate(source);
        }
        #endregion

        private bool HasDetected(Creature observer)
        {
            if (observer == null)
                return false;

            if (!_Detected.ContainsKey(observer.ID))
            {
                // TODO: distance penalties... +1 per 10ft
                _Detected[observer.ID] = observer.Skills.Skill<SpotSkill>().AutoCheck(SpotDifficulty.EffectiveValue, Creature);
            }
            return _Detected[observer.ID];
        }

        // IInteractHandler
        public void HandleInteraction(Interaction workSet)
        {
            if (workSet?.InteractData is GetInfoData)
            {
                if (!HasDetected(workSet?.Actor as Creature))
                {
                    // fixup size to current size...
                    _AltInfo.Size = Creature?.Sizer.Size.ToSizeInfo();
                    workSet.Feedback.Add(new InfoFeedback(this, _AltInfo));
                }
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(GetInfoData);
            yield return typeof(VisualPresentationData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // last feedback processor of visual presntation data
            if (interactType == typeof(VisualPresentationData))
                return true;

            // first strike at get info data
            if (interactType == typeof(GetInfoData))
                return true;
            return false;
        }

        public void ProcessFeedback(Interaction workSet)
        {
            if (workSet?.InteractData is VisualPresentationData)
            {
                if (!HasDetected(workSet?.Actor as Creature))
                {
                    var _fb = workSet?.Feedback.OfType<VisualModelFeedback>().FirstOrDefault();
                    if (_fb != null)
                    {
                        // no character pogs for successfully fooled observers
                        foreach (var _pog in _fb.ModelPresentation.Adornments.OfType<CharacterPogAdornment>().ToList())
                        {
                            _fb.ModelPresentation.Adornments.Remove(_pog);
                        }
                    }
                }
            }
        }

        // IMonitorChange<CoreActivity>
        public void PreTestChange(object sender, AbortableChangeEventArgs<CoreActivity> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<CoreActivity> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<CoreActivity> args)
        {
            var _critter = Creature;
            if ((args != null) && (args.NewValue != null)
                && (_critter != null) && (args.NewValue.Actor == _critter))
            {
                if ((args.NewValue.Action is ActionBase _action) && !_action.IsMental)
                {
                    _Detected.Clear();
                }
            }

            // TODO: any activities that should definitely cause awareness shifts (or always true detect)...?
        }
    }
}
