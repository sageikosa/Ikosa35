using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class GetLocatorsResult
    {
        #region data
        private Locator _Locator;
        private double _Cost;
        private bool _Extra;
        #endregion

        public Locator Locator { get => _Locator; set => _Locator = value; }
        public double MoveCost { get => _Cost; set => _Cost = value; }
        public bool IsExtraWeight { get => _Extra; set => _Extra = value; }
    }
}
