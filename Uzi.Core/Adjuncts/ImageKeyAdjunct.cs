using System;

namespace Uzi.Core
{
    /// <summary>Provides an ImageKey for an Item</summary>
    [Serializable]
    public class ImageKeyAdjunct : Adjunct
    {
        #region construction
        /// <summary>Provides an ImageKey for an Item</summary>
        public ImageKeyAdjunct(string key, int order)
            : base(typeof(ImageKeyAdjunct))
        {
            _Key = key;
            _Order = order;
        }
        #endregion

        #region private data
        private string _Key;
        private int _Order;
        #endregion

        public string Key { get { return _Key; } }
        public int Order { get { return _Order; } set { _Order = value; } }

        public override object Clone()
        {
            return new ImageKeyAdjunct(Key, Order);
        }
    }
}
