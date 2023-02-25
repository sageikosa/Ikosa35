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
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.UI;
using Uzi.Visualize;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for MechanismMountEditor.xaml
    /// </summary>
    public partial class MechanismMountEditor : TabItem, IPackageItem, IHostedTabItem, INotifyPropertyChanged
    {
        #region ctor()
        public MechanismMountEditor(PresentableMechanismMountVM mount, IHostTabControl host)
        {
            InitializeComponent();

            // control will be the view-model
            DataContext = this;
            _Mount = mount;

            // mechanism management
            _NewMechanism = new RelayCommand<string>((p) => NewMechanismExecute(p), (p) => NewMechanismCanExecute(p));
            _RemoveMechanism = new RelayCommand<object>(
                (o) =>
                {
                    if (o is ICoreObject _obj)
                    {
                        _obj.UnbindFromObject(Mount.Thing);
                    }
                },
                (o) => (o != null));

            // dimension changes
            _DecrDimension = new RelayCommand<string>(
                (d) =>
                {
                    switch (d)
                    {
                        case @"Z":
                            BindableZHeight--;
                            break;
                        case @"Y":
                            BindableYLength--;
                            break;
                        case @"X":
                            BindableXLength--;
                            break;
                        case @"P":
                            {
                                var _m = Mount.Thing;
                                _m.Pivot--;
                            }
                            break;
                    }
                },
                (d) =>
                {
                    switch (d)
                    {
                        case @"Z":
                            return BindableZHeight > 1;
                        case @"Y":
                            return BindableYLength > 1;
                        case @"X":
                            return BindableXLength > 1;
                        default:
                            return true;
                    }
                });
            _IncrDimension = new RelayCommand<string>(
                (d) =>
                {
                    switch (d)
                    {
                        case @"Z":
                            BindableZHeight++;
                            break;
                        case @"Y":
                            BindableYLength++;
                            break;
                        case @"X":
                            BindableXLength++;
                            break;
                        case @"P":
                            {
                                var _m = Mount.Thing;
                                _m.Pivot++;
                            }
                            break;
                    }
                },
                (d) =>
                {
                    switch (d)
                    {
                        case @"Z":
                            return BindableZHeight < 2;
                        case @"Y":
                            return BindableYLength < 2;
                        case @"X":
                            return BindableXLength < 2;
                        default:
                            return true;
                    }
                });

            _Host = host;
        }
        #endregion

        #region data
        private readonly IHostTabControl _Host;
        private readonly PresentableMechanismMountVM _Mount;
        private readonly RelayCommand<string> _NewMechanism;
        private readonly RelayCommand<object> _RemoveMechanism;
        private readonly RelayCommand<string> _DecrDimension;
        private readonly RelayCommand<string> _IncrDimension;
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public PresentableMechanismMountVM Mount => _Mount;
        public object PackageItem => _Mount;
        public RelayCommand<string> NewMechanism => _NewMechanism;
        public RelayCommand<object> RemoveMechanism => _RemoveMechanism;
        public RelayCommand<string> DecrDimension => _DecrDimension;
        public RelayCommand<string> IncrDimension => _IncrDimension;

        #region Bindable Dimensions
        /// <summary>Intended for editor use</summary>
        public long BindableZHeight
        {
            get => Mount.Thing.GeometricSize.ZHeight;
            set
            {
                if (value != BindableZHeight)
                {
                    Mount.Thing.SetMountSize(new GeometricSize(value, BindableYLength, BindableXLength));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BindableZHeight)));
                }
            }
        }

        /// <summary>Intended for editor use</summary>
        public long BindableYLength
        {
            get => Mount.Thing.GeometricSize.YLength;
            set
            {
                if (value != BindableYLength)
                {
                    Mount.Thing.SetMountSize(new GeometricSize(BindableZHeight, value, BindableXLength));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BindableYLength)));
                }
            }
        }

        /// <summary>Intended for editor use</summary>
        public long BindableXLength
        {
            get => Mount.Thing.GeometricSize.XLength;
            set
            {
                if (value != BindableXLength)
                {
                    Mount.Thing.SetMountSize(new GeometricSize(BindableZHeight, BindableYLength, value));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BindableXLength)));
                }
            }
        }
        #endregion

        #region private void NewMechanismExecute(string param)
        private void NewMechanismExecute(string param)
        {
            switch (param)
            {
                case @"RelockingOpener":
                    {
                        var _relock = new RelockingOpener(@"Relock", SteelMaterial.Static, 20);
                        _relock.BindToObject(Mount.Thing);
                    }
                    break;

                case @"LockKnob":
                    {
                        var _lockKnob = new LockKnob(@"Knob", SteelMaterial.Static, 20);
                        _lockKnob.BindToObject(Mount.Thing);
                    }
                    break;

                case @"Keyhole":
                    {
                        var _guid = Guid.NewGuid();
                        Mount.Thing.GetLocated().Locator.Map.NamedKeyGuids.Add(_guid, @"New Key");

                        var _keyHole = new Keyhole(@"Keyhole", SteelMaterial.Static, 20, 20, new Guid[] { _guid });
                        _keyHole.BindToObject(Mount.Thing);
                    }
                    break;

                case @"LockActivation":
                    {
                        var _guid = Guid.NewGuid();
                        Mount.Thing.GetLocated().Locator.Map.NamedKeyGuids.Add(_guid, @"New Key");

                        var _lockAct = new LockActivationMechanism(@"Lock Activation", SteelMaterial.Static, 20, 20,
                            new Guid[] { _guid }, ActivationMechanismStyle.Circuit);
                        _lockAct.BindToObject(Mount.Thing);
                    }
                    break;

                case @"SwitchActivation":
                    {
                        var _switchAct = new SwitchActivationMechanism(@"Switch Activation", SteelMaterial.Static, 20,
                            ActivationMechanismStyle.Circuit);
                        _switchAct.BindToObject(Mount.Thing);
                    }
                    break;

                case @"OpenerCloser":
                    {
                        var _opener = new OpenerCloser(@"Opener/Closer", SteelMaterial.Static, 20);
                        _opener.BindToObject(Mount.Thing);
                    }
                    break;

                case @"SurfaceTrigger":
                    {
                        var _surf = new SurfaceTriggerMechanism(@"Surface Trigger", SteelMaterial.Static, 20,
                            PostTriggerState.DeActivate, AnchorFaceList.ZLow, 20d, Mount.Thing);
                        _surf.BindToObject(Mount.Thing);
                    }
                    break;

                case @"ProximityTrigger":
                    {
                        var _prox = new ProximityTriggerMechanism(@"Proximity Trigger", SteelMaterial.Static, 20,
                            PostTriggerState.DeActivate, Size.Small, 30d, Mount.Thing);
                        _prox.BindToObject(Mount.Thing);
                    }
                    break;

                case @"MeleeTriggerable":
                    {
                        var _melee = new MeleeTriggerable(@"Melee Trap", SteelMaterial.Static, 20);
                        _melee.BindToObject(Mount.Thing);
                    }
                    break;

                case @"RangedTriggerable":
                    {
                        var _melee = new RangedTriggerable(@"Ranged Trap", SteelMaterial.Static, 20);
                        _melee.BindToObject(Mount.Thing);
                    }
                    break;

                case @"MagicTorchMechanism":
                    {
                        var _torch = new MagicTorchMechanism(@"Torch", WoodMaterial.Static, 20);
                        _torch.BindToObject(Mount.Thing);
                    }
                    break;
            }
        }

        private bool NewMechanismCanExecute(string name)
        {
            return true;
        }
        #endregion

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
        }

        #region IHostedTabItem Members
        public void CloseTabItem() { }
        #endregion

        #region private void lstMechanism_SelectionChanged(object sender, SelectionChangedEventArgs e)
        private void lstMechanism_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstMechanism.SelectedItem is PresentableThingVM<RelockingOpener> _relock)
            {
                ccMechanism.Content = new RelockingOpenerEditor { RelockingOpener = _relock.Thing };
            }
            else if (lstMechanism.SelectedItem is PresentableThingVM<OpenerCloser> _opener)
            {
                ccMechanism.Content = new OpenerCloserEditor { OpenerCloser = _opener.Thing };
            }
            else if (lstMechanism.SelectedItem is PresentableThingVM<ThrowBolt> _bolt)
            {
                ccMechanism.Content = new ThrowBoltEditor(_bolt.Thing);
            }
            else if (lstMechanism.SelectedItem is PresentableThingVM<LockKnob> _knob)
            {
                ccMechanism.Content = new LockKnobEditor(_knob.Thing);
            }
            else if (lstMechanism.SelectedItem is PresentableThingVM<Keyhole> _keyHole)
            {
                ccMechanism.Content = new KeyholeEditor(_keyHole.Thing);
            }
            else if (lstMechanism.SelectedItem is PresentableThingVM<LockActivationMechanism> _lockAct)
            {
                ccMechanism.Content = new LockActivationMechanismEditor { LockActivationMechanism = _lockAct.Thing };
            }
            else if (lstMechanism.SelectedItem is PresentableThingVM<SwitchActivationMechanism> _switchAct)
            {
                ccMechanism.Content = new SwitchActivationMechanismEditor { SwitchActivationMechanism = _switchAct.Thing };
            }
            else if (lstMechanism.SelectedItem is PresentableThingVM<SurfaceTriggerMechanism> _surfTrigger)
            {
                ccMechanism.Content = new SurfaceTriggerMechanismEditor { SurfaceTriggerMechanism = _surfTrigger.Thing };
            }
            else if (lstMechanism.SelectedItem is PresentableThingVM<ProximityTriggerMechanism> _proxTrigger)
            {
                ccMechanism.Content = new ProximityTriggerMechanismEditor { ProximityTriggerMechanism = _proxTrigger.Thing };
            }
            else if (lstMechanism.SelectedItem is PresentableThingVM<MeleeTriggerable> _meleeResponse)
            {
                ccMechanism.Content = new MeleeTriggerableEditor { MeleeTriggerable = _meleeResponse };
            }
            else if (lstMechanism.SelectedItem is PresentableThingVM<RangedTriggerable> _rangedResponse)
            {
                ccMechanism.Content = new RangedTriggerableEditor { RangedTriggerable = _rangedResponse };
            }
            else
            {
                ccMechanism.Content = null;
            }
        }
        #endregion
    }
}
