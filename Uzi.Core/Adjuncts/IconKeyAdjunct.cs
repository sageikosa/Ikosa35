using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    /// <summary>Provides an alternate IconKey for an Item</summary>
    [Serializable]
    public class IconKeyAdjunct : Adjunct
    {
        #region ctor(...)
        /// <summary>Provides an alternate IconKey for an Item</summary>
        public IconKeyAdjunct(string key, int order)
            : base(typeof(IconKeyAdjunct))
        {
            _Key = key;
            _Order = order;
        }
        #endregion

        #region state
        private readonly string _Key;
        private int _Order;
        #endregion

        public string Key => _Key;
        public int Order { get => _Order; set => _Order = value; }

        public static IEnumerable<string> GetIconKeys(IAdjunctable adjunctable)
            => adjunctable?.Adjuncts.OfType<IconKeyAdjunct>()
            .OrderBy(_ik => _ik.Order)
            .Select(_ik => _ik.Key);

        public override object Clone()
            => new IconKeyAdjunct(Key, Order);
    }
}
