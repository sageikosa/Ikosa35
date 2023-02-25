using System;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class SuperNaturalPowerDef : PowerDef<SuperNaturalPowerSource>, ISuperNaturalPowerDef
    {
        public SuperNaturalPowerDef(string name, string description, MagicStyle style)
        {
            _Style = style;
            _Name = name;
            _Description = description;
        }

        #region private data
        private MagicStyle _Style;
        private string _Name;
        private string _Description;
        #endregion

        public MagicStyle MagicStyle { get { return _Style; } }
        public override string DisplayName { get { return _Name; } }
        public override string Description { get { return _Description; } }

        public MagicPowerDefInfo ToMagicPowerDefInfo()
            => this.GetMagicPowerDefInfo();

        public override PowerDefInfo ToPowerDefInfo()
            => ToMagicPowerDefInfo();
    }
}