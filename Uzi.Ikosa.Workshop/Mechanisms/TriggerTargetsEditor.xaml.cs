using System;
using System.Collections.Generic;
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
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.UI;
using Uzi.Visualize;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for TriggerTargetsEditor.xaml
    /// </summary>
    public partial class TriggerTargetsEditor : UserControl
    {
        public TriggerTargetsEditor()
        {
            InitializeComponent();
            DataContext = this;

            _AddCommand = new RelayCommand(
                () =>
                {
                    ((PositionedObject)lstAvailable.SelectedItem).CoreObject.AddAdjunct(
                        new TriggerTarget(TriggerMechanism.TriggerMaster.TriggerGroup));
                    RefreshLists();
                },
                () => lstAvailable?.SelectedItem != null);
            _RemoveCommand = new RelayCommand(
                () =>
                {
                    var _select = (PositionedObject)lstCurrent.SelectedItem;
                    TriggerMechanism.TriggerMaster.TriggerGroup.Targets
                        .FirstOrDefault(_tt => _tt.Triggerable == _select.CoreObject)
                        ?.Eject();
                    RefreshLists();
                },
                () => lstCurrent?.SelectedItem != null);
        }

        #region data
        private RelayCommand _AddCommand;
        private RelayCommand _RemoveCommand;
        #endregion

        public RelayCommand AddCommand => _AddCommand;
        public RelayCommand RemoveCommand => _RemoveCommand;

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == ActivationMechanismProperty)
            {
                RefreshLists();
            }
            base.OnPropertyChanged(e);
        }

        private void RefreshLists()
        {
            lstCurrent.SelectedItem = null;
            lstCurrent.ItemsSource = CurrentTriggerables.ToList();
            lstAvailable.SelectedItem = null;
            lstAvailable.ItemsSource = AvailableTriggerables.ToList();
        }

        #region public IEnumerable<PositionedObject> AvailableTriggerables { get; }
        public IEnumerable<PositionedObject> AvailableTriggerables
        {
            get
            {
                if (TriggerMechanism != null)
                {
                    var _current = TriggerMechanism.Triggerables.ToList();
                    var _loc = TriggerMechanism.GetLocated().Locator;
                    var _rgn = _loc.GeometricRegion;

                    // activatables on own top-level and connected objects
                    foreach (var _obj in _loc.AllConnectedOf<ITriggerable>())
                    {
                        if ((_obj != TriggerMechanism) && !_current.Contains(_obj))
                        {
                            yield return new PositionedObject
                            {
                                CoreObject = _obj
                            };
                        }
                    }

                    // activatables on all other top-level objects
                    foreach (var _posObj in (from _l in (TriggerMechanism.Setting as LocalMap).MapContext.AllTokensOf<Locator>()
                                             where _l != _loc
                                             from _o in _l.AllConnectedOf<ITriggerable>()
                                             where !(_o is IItemBase) && !_current.Contains(_o)
                                             let _lRgn = _l.GeometricRegion
                                             select new PositionedObject
                                             {
                                                 CoreObject = _o,
                                                 Distance = Math.Round(_rgn.NearDistance(_lRgn), 2),
                                                 ZOffset = _lRgn.LowerZ - _rgn.LowerZ,
                                                 YOffset = _lRgn.LowerY - _rgn.LowerY,
                                                 XOffset = _lRgn.LowerX - _rgn.LowerX
                                             }).OrderBy(_po => _po.Distance))
                    {
                        yield return _posObj;
                    }
                }
                yield break;
            }
        }
        #endregion

        #region public IEnumerable<PositionedObject> CurrentTriggerables { get; }
        public IEnumerable<PositionedObject> CurrentTriggerables
        {
            get
            {
                if (TriggerMechanism != null)
                {
                    var _current = TriggerMechanism.Triggerables.ToList();
                    var _loc = TriggerMechanism.GetLocated().Locator;
                    var _rgn = _loc.GeometricRegion;

                    // activatables on all other top-level objects
                    foreach (var _posObj in (from _act in _current
                                             let _lRgn = _act.GetLocated().Locator.GeometricRegion
                                             select new PositionedObject
                                             {
                                                 CoreObject = _act,
                                                 Distance = Math.Round(_rgn.NearDistance(_lRgn), 2),
                                                 ZOffset = _lRgn.LowerZ - _rgn.LowerZ,
                                                 YOffset = _lRgn.LowerY - _rgn.LowerY,
                                                 XOffset = _lRgn.LowerX - _rgn.LowerX
                                             }).OrderBy(_po => _po.Distance))
                    {
                        yield return _posObj;
                    }
                }
                yield break;
            }
        }
        #endregion

        public TriggerMechanism TriggerMechanism
        {
            get => GetValue(ActivationMechanismProperty) as TriggerMechanism;
            set => SetValue(ActivationMechanismProperty, value);
        }

        // Using a DependencyProperty as the backing store for RelockingOpener.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActivationMechanismProperty =
            DependencyProperty.Register(nameof(TriggerMechanism), typeof(TriggerMechanism), typeof(TriggerTargetsEditor),
            new UIPropertyMetadata(null));
    }
}
