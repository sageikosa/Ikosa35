using System;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa
{
    [Serializable]
    public class DRMaterialException:DRException
    {
        public DRMaterialException(Material material)
            : base()
        {
            _Material = material;
        }

        private Material _Material;
        public Material Material
        {
            get
            {
                return _Material;
            }
        }

        public override bool DoesWeaponIgnoreReduction(Items.Weapons.IWeaponHead weaponHead)
        {
            // this is just in case someone doesn't use the static constructor
            return (weaponHead.HeadMaterial.Name.Equals(_Material.Name)&&
                    weaponHead.HeadMaterial.GetType().Equals(_Material.GetType()));
        }

        public override string Name
        {
            get
            {
                return _Material.Name;
            }
        }
    }
}
