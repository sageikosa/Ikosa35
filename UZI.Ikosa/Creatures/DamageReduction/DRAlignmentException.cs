using System;

namespace Uzi.Ikosa
{
    [Serializable]
    public class DRAlignmentException : DRException
    {
        public DRAlignmentException(Alignment align)
            : base()
        {
            this.Alignment = align;
        }

        public Alignment Alignment { get; private set; }

        public override string Name
        {
            get
            {
                return this.Alignment.NoNeutralString();
            }
        }

        public override bool DoesWeaponIgnoreReduction(Items.Weapons.IWeaponHead weaponHead)
        {
            return weaponHead.Alignment.Equals(this.Alignment);
        }
    }
}
