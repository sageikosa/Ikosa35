using System;
using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa
{
    /// <summary>Manages weight factors for size, and carrying capacity of container objects.</summary>
    [Serializable]
    public class ObjectSizer : Sizer
    {
        #region construction
        public ObjectSizer(Size size, ICoreObject coreObj)
            : base(size)
        {
            _Core = coreObj;
            _Containers = new List<IObjectContainer>();
        }

        public ObjectSizer(Size size, ICoreObject coreObj, IObjectContainer container)
            : base(size)
        {
            _Core = coreObj;
            _Containers = new List<IObjectContainer>
            {
                container
            };
        }
        #endregion

        #region data
        protected ICoreObject _Core;
        private List<IObjectContainer> _Containers;
        #endregion

        /// <summary>If not empty, the sizer will also adjust the maximum load weight of the specified container during offset resizing</summary>
        public List<IObjectContainer> Containers => _Containers;

        protected override void OnSizeChange()
        {
            // NOTE: use simple x2, /2 to keep altered weight in line with strength capacity for size changes
            // NOTE: only applies when the item size isn't the natural size (under a sizing effect)
            var _change = Size.Order - NaturalSize.Order;
            if (_change != 0)
            {
                var _factor = Math.Pow(2, _change);
                _Core.Weight *= _factor;
                foreach (var _c in Containers)
                {
                    _c.MaximumLoadWeight *= _factor;
                    _c.TareWeight *= _factor;
                }
                if (_Core is IOpenable)
                {
                    var _open = _Core as IOpenable;
                    _open.OpenWeight *= _factor;
                }
            }
        }
    }
}
