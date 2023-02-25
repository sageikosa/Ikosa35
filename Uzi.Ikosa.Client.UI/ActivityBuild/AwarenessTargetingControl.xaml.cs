using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Proxy.ViewModel;
using Uzi.Visualize;
using Uzi.Visualize.Contracts;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for AwarenessTargetingControl.xaml
    /// </summary>
    public partial class AwarenessTargetingControl : UserControl
    {
        public AwarenessTargetingControl()
        {
            try { InitializeComponent(); } catch { }
            _Choices = new List<AwarenessChoice>();
        }

        #region state
        protected List<AwarenessChoice> _Choices;
        #endregion

        public IEnumerable<AwarenessChoice> AwarenessChoices { get { return _Choices.Select(_c => _c); } }

        #region Resync
        private static void DoResync(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        {
            (depObj as AwarenessTargetingControl)?.ResyncTargets();
        }

        private void ResyncTargets()
        {
            // clear old
            var _oldChoices = _Choices.ToList();
            _Choices.Clear();

            if ((QueuedAwarenesses != null)
                && (AwarenessTargeting != null))
            {
                // snapshot queued
                var _queued = QueuedAwarenesses
                    .Where(_qa => _qa.Awareness.IsTargetable)
                    .ToList();

                if (_queued.Any())
                {
                    // select from queued only
                    _Choices = GetAllChoices(_oldChoices, _queued).ToList();
                }
                else
                {
                    // make sure we don't try to auto-select ourselves on harmful actions
                    // NOTE: you can still explicitly target via GetAllChoices and the queueing system...
                    var _self = AwarenessTargeting.Builder.ActivityBuilderActor.ActorID;
                    var _harmless = AwarenessTargeting.Builder.Action.IsHarmless;

                    // get from general awareness
                    _Choices = (from _a in AwarenessTargeting.Builder.ActivityBuilderActor.Awarenesses
                                where _a.IsTargetable && (_harmless || (_a.ID != _self))
                                let _cube = GetCubicForAwareness(_a)
                                where _cube.AllCellLocations().Any(_cl => IsCellInRange(_cl))
                                select new AwarenessChoice(AwarenessTargeting)
                                {
                                    Awareness = _a,
                                    TargetRegion = GetCubicForAwareness(_a)
                                }).ToList();
                }
            }

            // notify
            DoPropertyChanged(@"AwarenessChoices");
            AwarenessTargeting?.SetAwarenessChoices(_Choices);
        }
        #endregion

        #region public AwarenessTargeting AwarenessTargeting { get; set; } DEPENDENCY
        public AwarenessTargeting AwarenessTargeting
        {
            get { return (AwarenessTargeting)GetValue(AwarenessTargetingProperty); }
            set { SetValue(AwarenessTargetingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AwarenessTargeting.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AwarenessTargetingProperty =
            DependencyProperty.Register(@"AwarenessTargeting", typeof(AwarenessTargeting), typeof(AwarenessTargetingControl),
            new UIPropertyMetadata(null, DoResync));
        #endregion

        #region public IEnumerable<QueuedAwareness> QueuedAwarenesses { get; set; } DEPENDENCY
        public IEnumerable<QueuedAwareness> QueuedAwarenesses
        {
            get { return (IEnumerable<QueuedAwareness>)GetValue(QueuedAwarenessesProperty); }
            set { SetValue(QueuedAwarenessesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for QueuedAwarenesses.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty QueuedAwarenessesProperty =
            DependencyProperty.Register(nameof(QueuedAwarenesses), typeof(IEnumerable<QueuedAwareness>), typeof(AwarenessTargetingControl),
            new UIPropertyMetadata(null, DoResync));
        #endregion

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void DoPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #endregion

        private AwarenessChoice FindOrCreateChoice(List<AwarenessChoice> previous, AwarenessChoice candidate)
        {
            if (candidate != null)
            {
                var _comparer = new AwarenessChoiceComparer();
                var _original = previous.FirstOrDefault(_ac => _comparer.Equals(_ac, candidate));
                if (_original != null)
                    return _original;
            }
            return candidate;
        }

        #region protected bool IsCellInRange(ICellLocation location)
        /// <summary>
        /// Melee and strike zone use cell indexer counts from SourceCell
        /// Ranged uses minimum distance from SourcePoint.
        /// </summary>
        protected bool IsCellInRange(ICellLocation location)
        {
            var _range = AwarenessTargeting.AimingMode.RangeInfo;
            var _cube = AwarenessTargeting.Builder.ActivityBuilderTacticalActor.LocaleViewModel.Sensors;
            bool _inReachCube(int reach)
                => ((Math.Abs(_cube.Z - location.Z) <= reach) || (Math.Abs(_cube.ZTop - location.Z) <= reach))
                && ((Math.Abs(_cube.Y - location.Y) <= reach) || (Math.Abs(_cube.YTop - location.Y) <= reach))
                && ((Math.Abs(_cube.X - location.X) <= reach) || (Math.Abs(_cube.XTop - location.X) <= reach));
            if (_range is MeleeRangeInfo _melee)
            {
                // none must go beyond the reach
                return _inReachCube(_melee.ReachSquares);
            }
            else if (_range is StrikeZoneRangeInfo _strike)
            {
                return (_strike.MinimumReach > 0)
                    // not beyond max reach, and none closer than minimum reach
                    ? _inReachCube(_strike.MaximumReach) && !_inReachCube(_strike.MinimumReach - 1)
                    // not beyond max reach
                    : _inReachCube(_strike.MaximumReach);
            }
            else
            {
                // in max range
                return (new CellPosition(location)).NearDistance(_cube) <= _range.Value;
            }
        }
        #endregion

        #region protected IEnumerable<AttackChoice> GetNextChoices(List<AwarenessChoice> previous,List<QueuedAwareness> potential)
        protected IEnumerable<AwarenessChoice> GetNextChoices(List<AwarenessChoice> previous, List<QueuedAwareness> potential)
        {
            // TODO: filter by target type as known by creature...?
            if (potential.Any())
            {
                // gather awareness choices first
                var _awareness = potential.FirstOrDefault();

                // in range?
                if (AwarenessTargeting.Builder.ActivityBuilderTacticalActor != null)
                {
                    var _cube = GetCubicForAwareness(_awareness.Awareness);
                    if (_cube != null)
                    {
                        if (_cube.AllCellLocations().Any(_cl => IsCellInRange(_cl)))
                        {
                            // simple awareness
                            yield return FindOrCreateChoice(
                                previous,
                                new AwarenessChoice(AwarenessTargeting)
                                {
                                    Awareness = _awareness.Awareness,
                                    TargetRegion = _cube
                                });
                        }
                    }
                }

                // remove queued awareness so we don't get stuck on it or re-process it
                potential.Remove(_awareness);
            }
            yield break;
        }
        #endregion

        #region protected IEnumerable<AwarenessChoice> GetAllChoices(List<AwarenessChoice> previous,List<QueuedAwareness> potential)
        protected IEnumerable<AwarenessChoice> GetAllChoices(List<AwarenessChoice> previous, List<QueuedAwareness> potential)
        {
            var _next = GetNextChoices(previous, potential).ToList();
            while (_next.Any())
            {
                // add choices that were produced
                foreach (var _choice in _next)
                    yield return _choice;

                // end of loop, seed next cycle
                _next = GetNextChoices(previous, potential).ToList();
            }
            yield break;
        }
        #endregion

        #region protected CubicInfo GetCubicForAwareness(AwarenessInfo awareness)
        /// <summary>
        /// Gets the cubic information for the awareness from the presentation
        /// </summary>
        protected CubicInfo GetCubicForAwareness(AwarenessInfo awareness)
        {
            if (AwarenessTargeting != null)
            {
                if (AwarenessTargeting.Builder.ActivityBuilderActor is IActivityBuilderTacticalActor _tactical)
                {
                    return _tactical.LocaleViewModel.Presentations
                        .FirstOrDefault(_p => _p.PresentingIDs.Contains(awareness.ID));
                }
            }
            return null;
        }
        #endregion
    }
}
