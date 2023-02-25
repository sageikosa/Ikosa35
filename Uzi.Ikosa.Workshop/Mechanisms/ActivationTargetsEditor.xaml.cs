using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
    /// Interaction logic for ActivationTargetsEditor.xaml
    /// </summary>
    public partial class ActivationTargetsEditor : UserControl
    {
        public ActivationTargetsEditor()
        {
            InitializeComponent();
            DataContext = this;

            _AddCommand = new RelayCommand(
                () =>
                {
                    ((PositionedObject)lstAvailable.SelectedItem).CoreObject.AddAdjunct(
                        new ActivationTarget(ActivationMechanism.ActivationMaster.ActivationGroup));
                    RefreshLists();
                },
                () => lstAvailable?.SelectedItem != null);
            _RemoveCommand = new RelayCommand(
                () =>
                {
                    var _select = (PositionedObject)lstCurrent.SelectedItem;
                    ActivationMechanism.ActivationMaster.ActivationGroup.Targets
                        .FirstOrDefault(_at => _at.ActivatableObject == _select.CoreObject)
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
            lstCurrent.ItemsSource = CurrentActivatables.ToList();
            lstAvailable.SelectedItem = null;
            lstAvailable.ItemsSource = AvailableActivatables.ToList();
        }

        #region public IEnumerable<PositionedObject> AvailableActivatables { get; }
        public IEnumerable<PositionedObject> AvailableActivatables
        {
            get
            {
                if (ActivationMechanism != null)
                {
                    var _current = ActivationMechanism.ActivatableObjects.ToList();
                    var _loc = ActivationMechanism.GetLocated().Locator;
                    var _rgn = _loc.GeometricRegion;

                    // activatables on own top-level and connected objects
                    foreach (var _obj in _loc.AllConnectedOf<IActivatableObject>())
                    {
                        if ((_obj != ActivationMechanism) && !_current.Contains(_obj))
                        {
                            yield return new PositionedObject
                            {
                                CoreObject = _obj
                            };
                        }
                    }

                    // activatables on all other top-level objects
                    foreach (var _posObj in (from _l in (ActivationMechanism.Setting as LocalMap).MapContext.AllTokensOf<Locator>()
                                             where _l != _loc
                                             from _o in _l.AllConnectedOf<IActivatableObject>()
                                             where !(_o is IItemBase) && !_current.Contains(_o)
                                             let _lRgn = _l.GeometricRegion
                                             select new PositionedObject
                                             {
                                                 CoreObject = _o,
                                                 Distance = Math.Round(_rgn.NearDistance(_lRgn), 2),
                                                 ZOffset = _rgn.LowerZ - _lRgn.LowerZ,
                                                 YOffset = _rgn.LowerY - _lRgn.LowerY,
                                                 XOffset = _rgn.LowerX - _lRgn.LowerX
                                             }).OrderBy(_po => _po.Distance))
                    {
                        yield return _posObj;
                    }
                }
                yield break;
            }
        }
        #endregion

        #region public IEnumerable<PositionedObject> CurrentActivatables { get; }
        public IEnumerable<PositionedObject> CurrentActivatables
        {
            get
            {
                if (ActivationMechanism != null)
                {
                    var _current = ActivationMechanism.ActivatableObjects.ToList();
                    var _loc = ActivationMechanism.GetLocated().Locator;
                    var _rgn = _loc.GeometricRegion;

                    // activatables on all other top-level objects
                    foreach (var _posObj in (from _act in _current
                                             let _lRgn = _act.GetLocated().Locator.GeometricRegion
                                             select new PositionedObject
                                             {
                                                 CoreObject = _act,
                                                 Distance = Math.Round(_rgn.NearDistance(_lRgn), 2),
                                                 ZOffset = _rgn.LowerZ - _lRgn.LowerZ,
                                                 YOffset = _rgn.LowerY - _lRgn.LowerY,
                                                 XOffset = _rgn.LowerX - _lRgn.LowerX
                                             }).OrderBy(_po => _po.Distance))
                    {
                        yield return _posObj;
                    }
                }
                yield break;
            }
        }
        #endregion

        public ActivationMechanism ActivationMechanism
        {
            get => GetValue(ActivationMechanismProperty) as ActivationMechanism;
            set => SetValue(ActivationMechanismProperty, value);
        }

        // Using a DependencyProperty as the backing store for RelockingOpener.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActivationMechanismProperty =
            DependencyProperty.Register(nameof(ActivationMechanism), typeof(ActivationMechanism), typeof(ActivationTargetsEditor),
            new UIPropertyMetadata(null));
    }
}
