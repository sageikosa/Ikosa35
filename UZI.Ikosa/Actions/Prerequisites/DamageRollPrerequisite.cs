using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class DamageRollPrerequisite : RollPrerequisite
    {
        #region ctor()
        public DamageRollPrerequisite(object source, Interaction workSet, string key, string name, Roller roller, 
            bool serial, bool nonLethal, string extra, int minGroup)
            : base(source, workSet, workSet?.Actor, key, name, roller, serial)
        {
            _NonLethal = nonLethal;
            _Extra = extra;
            _MinGroup = minGroup;
        }
        #endregion

        #region data
        private bool _NonLethal;
        private string _Extra;
        private int _MinGroup;
        #endregion

        public bool IsNonLethal => _NonLethal;
        public string Extra => _Extra;
        public int MinGroup => _MinGroup;

        public virtual IEnumerable<DamageData> GetDamageData()
        {
            yield return new DamageData(RollValue, IsNonLethal, Extra, MinGroup);
            yield break;
        }
    }
}
