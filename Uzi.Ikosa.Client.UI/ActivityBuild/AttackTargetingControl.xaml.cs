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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Proxy.ViewModel;
using Uzi.Visualize;
using Uzi.Visualize.Contracts;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for AttackTargetingControl.xaml
    /// </summary>
    public partial class AttackTargetingControl : UserControl
    {
        public AttackTargetingControl()
        {
            try { InitializeComponent(); } catch { }
            _Choices = new List<AttackChoice>();
            _MaxCellZone = null;
        }

        #region private data
        protected List<AttackChoice> _Choices;

        /// <summary>null if not capable of cell targeting</summary>
        private int? _MaxCellZone;

        private bool _ReachOne;
        #endregion

        public IEnumerable<AttackChoice> AttackChoices
            => _Choices.Select(_c => _c);

        #region Resync
        private static void DoResync(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        {
            if (depObj is AttackTargetingControl _control)
                _control.ResyncTargets();
        }

        private void ResyncTargets()
        {
            // clear old
            var _oldChoices = _Choices.ToList();
            _Choices.Clear();

            // reach one only applies to reach weapons that can target 1 cell unit away, but cannot use strike zone
            _ReachOne = false;

            if ((QueuedTargets != null)
                && (SourceCell != null)
                && (SourcePoint != null)
                && (AttackTargeting != null))
            {
                #region set cell zone rules
                // determine cell zone rules
                if (AttackTargeting.AimingMode.RangeInfo is MeleeRangeInfo)
                {
                    // melee cell zone extends no farther than 1 (but could be 0)
                    _MaxCellZone = Math.Min((AttackTargeting.AimingMode.RangeInfo as MeleeRangeInfo).ReachSquares, 1);
                }
                else if (AttackTargeting.AimingMode.RangeInfo is StrikeZoneRangeInfo)
                {
                    var _strike = AttackTargeting.AimingMode.RangeInfo as StrikeZoneRangeInfo;
                    if (_strike.MinimumReach > 1)
                    {
                        // if minimum reach is beyond 1, there is no cell zone targeting
                        _MaxCellZone = null;
                    }
                    else if (_strike.MinimumReach > 0)
                    {
                        // if minimum reach is beyond 0, there is no cell zone targeting
                        _MaxCellZone = null;

                        // but, we can make a reach attack 1 cell unit away
                        _ReachOne = true;
                    }
                    else
                    {
                        // otherwise, no more than 1 for maximum cell zone
                        _MaxCellZone = Math.Min(_strike.MaximumReach, 1);
                    }
                }
                #endregion

                // resync when ready
                _Choices = GetAllChoices(_oldChoices, QueuedTargets.ToList()).ToList();

                // if no choices yet, try some last ditch implied targets
                if (!_Choices.Any())
                {
                    if (AttackTargeting.Builder.ActivityBuilderActor is IActivityBuilderTacticalActor _tactical)
                    {
                        if (_tactical.AimPointActivation == AimPointActivation.TargetCell)
                        {
                            #region Unqueued TargetCell Aim
                            // NOTE: if target-cell aiming is on, will not try adjacent or same cell targeting
                            //  ...  even if target-cell is out of range
                            // TODO: TargetIntersection also?
                            var _cell = new CellPosition(_tactical.LocaleViewModel.TargetCell);
                            if (IsCellInRange(_cell))
                            {
                                _Choices.Add(new AttackChoice(AttackTargeting)
                                {
                                    Awareness = null,
                                    TargetCell = _cell
                                });
                            }
                            #endregion
                        }
                        else if (((_MaxCellZone ?? 0) == 1) || _ReachOne)
                        {
                            #region adjacent facing cell
                            // NOTE: can make melee-style attacks up to 1 cell away
                            var _adjacent = _tactical.LocaleViewModel.AdjacentFacingCell;
                            _Choices.Add(new AttackChoice(AttackTargeting)
                            {
                                Awareness = null,
                                TargetCell = _adjacent
                            });
                            #endregion
                        }
                        else if ((_MaxCellZone ?? 0) == 0)
                        {
                            #region Into Personal AimCell
                            // NOTE: attack into aimimg-cell, but will not target self...
                            // NOTE: it would be better for the actor to select an awareness, but this is OK
                            _Choices.Add(new AttackChoice(AttackTargeting)
                            {
                                Awareness = null,
                                TargetCell = new CellPosition(_tactical.LocaleViewModel.Sensors.AimCell)
                            });
                            #endregion
                        }
                    }
                }
            }

            // notify
            DoPropertyChanged(nameof(AttackChoices));
            AttackTargeting?.SetAttackChoices(_Choices);
        }
        #endregion

        #region public AttackTargeting AttackTargeting { get; set; } DEPENDENCY
        public AttackTargeting AttackTargeting
        {
            get { return (AttackTargeting)GetValue(AttackTargetingProperty); }
            set { SetValue(AttackTargetingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AttackTargeting.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AttackTargetingProperty =
            DependencyProperty.Register(@"AttackTargeting", typeof(AttackTargeting), typeof(AttackTargetingControl),
            new UIPropertyMetadata(null, DoResync));
        #endregion

        #region public IEnumerable<QueuedTargetItem> QueuedTargets { get; set; } DEPENDENCY
        public IEnumerable<QueuedTargetItem> QueuedTargets
        {
            get { return (IEnumerable<QueuedTargetItem>)GetValue(QueuedTargetsProperty); }
            set { SetValue(QueuedTargetsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for QueuedTargets.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty QueuedTargetsProperty =
            DependencyProperty.Register(@"QueuedTargets", typeof(IEnumerable<QueuedTargetItem>), typeof(AttackTargetingControl),
            new UIPropertyMetadata(null, DoResync));
        #endregion

        #region public ICellLocation SourceCell { get; set; } DEPENDENCY
        public ICellLocation SourceCell
        {
            get { return (ICellLocation)GetValue(SourceCellProperty); }
            set { SetValue(SourceCellProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SourceCell.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceCellProperty =
            DependencyProperty.Register(@"SourceCell", typeof(ICellLocation), typeof(AttackTargetingControl),
            new UIPropertyMetadata(null, DoResync));
        #endregion

        #region public Point3D? SourcePoint { get; set; } DEPENDENCY
        public Point3D? SourcePoint
        {
            get { return (Point3D?)GetValue(SourcePointProperty); }
            set { SetValue(SourcePointProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SourcePoint.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourcePointProperty =
            DependencyProperty.Register(@"SourcePoint", typeof(Point3D?), typeof(AttackTargetingControl),
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

        private AttackChoice FindOrCreateChoice(List<AttackChoice> previous, AttackChoice candidate)
        {
            if (candidate != null)
            {
                var _comparer = new AttackChoiceComparer();
                var _original = previous.FirstOrDefault(_ac => _comparer.Equals(_ac, candidate));
                if (_original != null)
                    return _original;
            }
            return candidate;
        }

        #region protected IEnumerable<AttackChoice> GetNextChoices(List<AttackChoice> previous,List<QueuedTargetItem> potential)
        protected IEnumerable<AttackChoice> GetNextChoices(List<AttackChoice> previous, List<QueuedTargetItem> potential)
        {
            if (potential.Any())
            {
                if (AttackTargeting.AimingMode.IsCellAimOnly)
                {
                    #region cell aim only
                    // cell aims only ever target cells
                    var _cell = potential.OfType<QueuedCell>().FirstOrDefault();
                    if ((_cell != null) && IsCellInRange(_cell.Location))
                    {
                        // make sure we don't target the cell again
                        potential.Remove(_cell);
                        yield return FindOrCreateChoice(
                            previous,
                            new AttackChoice(AttackTargeting)
                            {
                                TargetCell = _cell.Location,
                                Awareness = null
                            });
                    }
                    #endregion
                }
                else if (potential.OfType<QueuedAwareness>().Any())
                {
                    // gather awareness choices first
                    var _awareness = potential.OfType<QueuedAwareness>().FirstOrDefault();
                    if (_awareness.Awareness.IsTargetable)
                    {
                        // in range?
                        var _cube = GetCubicForAwareness(_awareness.Awareness);
                        if (_cube != null)
                        {
                            if (_cube.AllCellLocations().Any(_cell => IsCellInRange(_cell)))
                            {
                                // bias towards strike zone cells?
                                if (_MaxCellZone.HasValue)
                                {
                                    // any cells dip into strike zone?
                                    if (_cube.AllCellLocations().Any(_cell => IsCellInCellZone(_cell)))
                                    {
                                        #region specified awareness and adjacent cells
                                        // some in strike zone
                                        var _foundCells = false;
                                        foreach (var _queuedCell in (from _qc in potential.OfType<QueuedCell>()
                                                                     let _c = _qc.Location
                                                                     where _cube.ContainsCell(_c) && IsCellInCellZone(_c)
                                                                     select _qc).ToList())
                                        {
                                            // use any targetted adjacent cells
                                            yield return FindOrCreateChoice(
                                                previous,
                                                new AttackChoice(AttackTargeting)
                                                {
                                                    Awareness = _awareness.Awareness,
                                                    TargetCell = _queuedCell.Location
                                                });

                                            // we don't need to auto use adjacent cells
                                            _foundCells = true;

                                            // and we don't want to visit this cell again
                                            potential.Remove(_queuedCell);
                                        }

                                        if (!_foundCells)
                                        {
                                            // no found cells, use adjacent cells
                                            foreach (var _cell in _cube.AllCellLocations()
                                                .Where(_c => IsCellInCellZone(_c)))
                                            {
                                                yield return FindOrCreateChoice(
                                                    previous,
                                                    new AttackChoice(AttackTargeting)
                                                    {
                                                        Awareness = _awareness.Awareness,
                                                        TargetCell = _cell
                                                    });
                                            }
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        // none in strike zone, simple awareness
                                        yield return FindOrCreateChoice(
                                            previous,
                                            new AttackChoice(AttackTargeting)
                                            {
                                                TargetCell = null,
                                                TargetRegion = _cube,
                                                Awareness = _awareness.Awareness
                                            });
                                    }
                                }
                                else if (AttackTargeting.AimingMode.UseCellForIndirect)
                                {
                                    #region specified awareness and in range cells
                                    // some in strike zone
                                    var _foundCells = false;
                                    foreach (var _queuedCell in (from _qc in potential.OfType<QueuedCell>()
                                                                 let _c = _qc.Location
                                                                 where _cube.ContainsCell(_c) && IsCellInRange(_c)
                                                                 select _qc).ToList())
                                    {
                                        // use any targetted in range cells
                                        yield return FindOrCreateChoice(
                                            previous,
                                            new AttackChoice(AttackTargeting)
                                            {
                                                Awareness = _awareness.Awareness,
                                                TargetCell = _queuedCell.Location
                                            });

                                        // we don't need to implicit single cells
                                        _foundCells = true;

                                        // and we don't want to visit this cell again
                                        potential.Remove(_queuedCell);
                                    }

                                    if (!_foundCells && (_cube.XLength == 1) && (_cube.YLength == 1) && (_cube.ZHeight == 1))
                                    {
                                        // no found cells, use singleton cell
                                        foreach (var _cell in _cube.AllCellLocations())
                                        {
                                            yield return FindOrCreateChoice(
                                                previous,
                                                new AttackChoice(AttackTargeting)
                                                {
                                                    Awareness = _awareness.Awareness,
                                                    TargetCell = _cell
                                                });
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    // simple awareness
                                    yield return FindOrCreateChoice(
                                        previous,
                                        new AttackChoice(AttackTargeting)
                                        {
                                            TargetCell = null,
                                            TargetRegion = _cube,
                                            Awareness = _awareness.Awareness
                                        });
                                }
                            }
                        }
                    }

                    // remove queued awareness so we don't get stuck on it or re-process it
                    potential.Remove(_awareness);
                }
                else if (potential.Any())
                {
                    // all in range cells
                    foreach (var _cell in potential.OfType<QueuedCell>()
                        .Where(_qc => IsCellInRange(_qc.Location)))
                    {
                        yield return FindOrCreateChoice(
                            previous,
                            new AttackChoice(AttackTargeting)
                            {
                                Awareness = null,
                                TargetCell = _cell.Location
                            });
                    }

                    // clear the list if we get this far, no more to do...
                    potential.Clear();
                }
            }
            yield break;
        }
        #endregion

        #region protected IEnumerable<AttackChoice> GetAllChoices(List<AttackChoice> previous,List<QueuedTargetItem> potential)
        protected IEnumerable<AttackChoice> GetAllChoices(List<AttackChoice> previous, List<QueuedTargetItem> potential)
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

        #region protected bool IsCellInCellZone(ICellLocation location)
        /// <summary>
        /// true if cell is in the zone
        /// </summary>
        protected bool IsCellInCellZone(ICellLocation location)
        {
            /// must have a sourceCell and MaxCellZone must be a number
            if ((SourceCell != null) && (_MaxCellZone != null))
            {
                var _max = _MaxCellZone.Value;
                return Math.Abs(SourceCell.Z - location.Z) <= _max
                    && Math.Abs(SourceCell.Y - location.Y) <= _max
                    && Math.Abs(SourceCell.X - location.X) <= _max;
            }
            return false;
        }
        #endregion

        #region protected bool IsCellInRange(ICellLocation location)
        /// <summary>
        /// Melee and strike zone use cell indexer counts from SourceCell
        /// Ranged uses minimum distance from SourcePoint.
        /// </summary>
        protected bool IsCellInRange(ICellLocation location)
        {
            var _range = AttackTargeting.AimingMode.RangeInfo;
            if (_range is MeleeRangeInfo)
            {
                // none must go beyond the reach
                var _melee = _range as MeleeRangeInfo;
                var _offset = location.Subtract(new CellPosition(SourceCell));
                return ((Math.Abs(_offset.Z) <= _melee.ReachSquares)
                    && (Math.Abs(_offset.Y) <= _melee.ReachSquares)
                    && (Math.Abs(_offset.X) <= _melee.ReachSquares));
            }
            else if (_range is StrikeZoneRangeInfo)
            {
                // none can go beyond max reach, but only one needs to extend to or beyond minimum reach
                var _strike = _range as StrikeZoneRangeInfo;
                var _offset = location.Subtract(new CellPosition(SourceCell));
                var _z = Math.Abs(_offset.Z);
                var _y = Math.Abs(_offset.Y);
                var _x = Math.Abs(_offset.X);
                return (_z <= _strike.MaximumReach)
                    && (_y <= _strike.MaximumReach)
                    && (_x <= _strike.MaximumReach)
                    && ((_z >= _strike.MinimumReach)
                      || (_y >= _strike.MinimumReach)
                      || (_x >= _strike.MinimumReach));
            }
            else
            {
                // in max range
                return (new CellPosition(location)).NearDistance(SourcePoint ?? new Point3D()) <= _range.Value;
            }
        }
        #endregion

        #region protected CubicInfo GetCubicForAwareness(AwarenessInfo awareness)
        /// <summary>
        /// Gets the cubic information for the awareness from the presentation
        /// </summary>
        protected CubicInfo GetCubicForAwareness(AwarenessInfo awareness)
        {
            if (AttackTargeting != null)
            {
                if (AttackTargeting.Builder.ActivityBuilderActor is IActivityBuilderTacticalActor _tactical)
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
