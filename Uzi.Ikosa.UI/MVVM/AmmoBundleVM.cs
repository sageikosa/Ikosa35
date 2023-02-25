using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Ranged;

namespace Uzi.Ikosa.UI
{
    public class AmmoBundleVM
    {
        public AmmoBundleVM(IAmmunitionBundle bundle, PresentableCreatureVM possessor)
        {
            _Bundle = bundle;
            _Possessor = possessor;
            _AddCmd = new RelayCommand(
                () => Bundle?.MergeAmmo((Bundle?.CreateAmmo(), 1)),
                () => (Bundle?.Capacity ?? int.MaxValue) > Bundle?.Count);

            _RemoveCmd = new RelayCommand<AmmoEditSet>(
                (ammoSet) => Bundle?.ExtractAmmo(ammoSet.Ammunition),
                (ammoSet) => ammoSet != null);
            _ExtractCmd = new RelayCommand<AmmoEditSet>(
                (ammoSet) =>
                {
                    var _extract = Bundle?.ExtractAmmo(ammoSet.Ammunition);
                    if (_extract?.ammo != null)
                    {
                        var _bundle = _extract?.ammo.ToAmmunitionBundle($@"{_extract?.ammo.Name} Bundle");
                        _bundle.SetCount(_extract?.ammo, _extract?.count ?? 1);
                        _bundle.Possessor = Bundle.Possessor;
                        _Possessor?.DoChangedPossessions();
                    }
                },
                (ammoSet) => ammoSet != null);
            _IdentityCmd = new RelayCommand<AmmoEditSet>(
                (ammoSet) => 
                {
                    var _dlg = new Window
                    {
                        //Owner = Window.GetWindow(this),
                        Content = new ObjectIdentityEditor(ammoSet.Ammunition as WeaponHead, Bundle.Possessor),
                        WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
                        WindowStyle = WindowStyle.ToolWindow,
                        Title = ammoSet.Ammunition.Name,
                        SizeToContent = SizeToContent.WidthAndHeight
                    };
                    _dlg.ShowDialog();
                },
                (ammoSet) => ammoSet != null);
        }

        #region data
        private readonly IAmmunitionBundle _Bundle;
        private readonly PresentableCreatureVM _Possessor;
        private readonly RelayCommand _AddCmd;
        private readonly RelayCommand<AmmoEditSet> _RemoveCmd;
        private readonly RelayCommand<AmmoEditSet> _ExtractCmd;
        private readonly RelayCommand<AmmoEditSet> _IdentityCmd;
        #endregion

        public IAmmunitionBundle Bundle => _Bundle;
        public RelayCommand AddCmd => _AddCmd;
        public RelayCommand<AmmoEditSet> RemoveCmd => _RemoveCmd;
        public RelayCommand<AmmoEditSet> ExtractCmd => _ExtractCmd;
        public RelayCommand<AmmoEditSet> IdentityCmd => _IdentityCmd;
    }
}
