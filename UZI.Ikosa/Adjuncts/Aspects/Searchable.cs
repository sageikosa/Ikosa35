using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Indicates that the object is not immediately observable and must be searched to be found.</summary>
    [Serializable]
    public class Searchable : Adjunct, ICore, IProcessFeedback
    {
        public Searchable(Deltable difficulty, bool isProtected)
            : base(difficulty)
        {
            _IsProtected = isProtected;
        }

        public Deltable Difficulty => Source as Deltable;

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            (Anchor as CoreObject)?.AddIInteractHandler(this);
            base.OnActivate(source);
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            (Anchor as CoreObject)?.RemoveIInteractHandler(this);
            base.OnDeactivate(source);
        }
        #endregion

        #region data
        private HashSet<Guid> _Found;
        private Dictionary<Guid, double> _AutoTime;
        private bool _IsProtected;
        #endregion

        #region FoundSet
        private HashSet<Guid> GetFoundSet()
            => _Found ??= new HashSet<Guid>();

        public void ClearFound()
            => GetFoundSet().Clear();

        public bool HasFound(Guid critterID)
            => GetFoundSet().Contains(critterID);

        public void SetFound(Guid critterID, bool foundState)
        {
            var _set = GetFoundSet();
            if (foundState)
            {
                if (!_set.Contains(critterID))
                    _set.Add(critterID);
            }
            else if (_set.Contains(critterID))
                _set.Remove(critterID);
        }
        #endregion

        #region AutoCheck
        protected double CurrentTime
            => Anchor?.GetCurrentTime() ?? double.MaxValue;

        private Dictionary<Guid, double> GetAutoTimeSet()
            => _AutoTime ??= new Dictionary<Guid, double>();

        public void ClearAutoTime()
            => GetAutoTimeSet().Clear();

        public double? LastAutoCheck(Guid critterID)
        {
            var _set = GetAutoTimeSet();
            if (_set.TryGetValue(critterID, out var _auto))
                return _auto;
            return null;
        }

        public void SetAutoChecked(Guid critterID, bool checkSet)
        {
            var _set = GetAutoTimeSet();
            if (checkSet)
            {
                _set[critterID] = CurrentTime;
            }
            else if (_set.ContainsKey(critterID))
            {
                _set.Remove(critterID);
            }
        }
        #endregion

        public override bool IsProtected => _IsProtected;

        public override object Clone()
            => new Searchable(Difficulty, IsProtected);

        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet.InteractData is Observe _obs)
            {
                var _viewer = _obs.Viewer as ISensorHost;
                if (workSet.Target is ICoreObject _target)
                {
                    // an open (non-mechanism) searchable can skip this ???
                    if ((_target is IOpenable _open)
                        && !(_target is Mechanism)
                        && !_open.OpenState.IsClosed)
                        return;

                    // if searchable already found, we can skip this stuff
                    if (!HasFound(_viewer.ID))
                    {
                        void _fail()
                        {
                            // search failed, not found... :-(
                            workSet.Feedback.Add(
                                new ObserveFeedback(this, new KeyValuePair<Guid, AwarenessLevel>[]
                                {
                                    new KeyValuePair<Guid, AwarenessLevel>(_target.ID, AwarenessLevel.None)
                                }));
                        }

                        // actively searching?
                        if (_obs is SearchData _search)
                        {
                            if (_search.IsAutoCheck && LastAutoCheck(_viewer.ID).HasValue)
                            {
                                // already auto-checked :-(
                                _fail();
                            }
                            else
                            {
                                #region searching
                                var _distance = _obs.GetDistance(_target);
                                if (_distance <= 5)
                                {
                                    var _check = Deltable.GetCheckNotify(_viewer.ID, @"Search Check", Anchor.ID, @"Difficulty");
                                    var _difficulty = Difficulty.QualifiedValue(workSet, _check.OpposedInfo);
                                    if ((_search.CheckValue.QualifiedValue(workSet, Deltable.GetDeltaCalcNotify(_viewer.ID, @"Search Check").DeltaCalc)
                                        >= _difficulty))
                                    {
                                        // successful search allows observation attempt
                                        if ((_target.HasAdjunct<TrapPart>())
                                            && (_difficulty > 20)
                                            && !_viewer.Adjuncts.OfType<ITrapFinding>().Any(_tf => _tf.CanFindTrap(_target)))
                                        {
                                            // however, if it is a trap, must be able to find it...
                                            _fail();
                                        }
                                    }
                                    else
                                    {
                                        // search failed, not found... :-(
                                        _fail();
                                    }
                                }
                                else
                                {
                                    // search failed, not found... :-(
                                    _fail();
                                }
                                #endregion
                            }
                        }
                        else
                        {
                            // if viewer hasn't already found it, a simple observe won't work
                            _fail();
                        }
                    }
                }
            }

        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(Observe);
            yield return typeof(ObserveDetails);
            yield return typeof(SearchData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            if (interactType == typeof(ObserveDetails))
                return true;
            if (typeof(ObserveHandler).IsAssignableFrom(existingHandler.GetType()))
                return true;
            return existingHandler is Hiding;
        }

        #endregion

        #region IProcessFeedback Members

        public void ProcessFeedback(Interaction workSet)
        {
            // process feedback from observe handler
            if (workSet.InteractData is SearchData _obs)
            {
                // if currently no awareness of the searchable target...
                if ((workSet.Target is CoreObject _target)
                    && (_obs.Viewer is ISensorHost _viewer)
                    && _viewer.IsSensorHostActive
                    && ((_viewer?.Awarenesses?.GetAwarenessLevel(_target.ID) ?? AwarenessLevel.None) == AwarenessLevel.None))
                {
                    // would observe feedback would expose the searchable target?
                    var _obsBack = workSet.Feedback
                        .OfType<ObserveFeedback>()
                        .FirstOrDefault(_of =>
                            _of.Levels.TryGetValue(_target.ID, out var _aware) &&
                            (_aware == AwarenessLevel.Aware));
                    if (_obsBack != null)
                    {
                        // then we found it (since we must have passed the search check on the way in)
                        SetFound(_viewer.ID, true);
                        SetAutoChecked(_viewer.ID, false);
                    }
                    else if (_obs.IsAutoCheck)
                    {
                        // auto-check failed
                        SetAutoChecked(_viewer.ID, true);
                    }
                }
            }
        }

        #endregion
    }
}
