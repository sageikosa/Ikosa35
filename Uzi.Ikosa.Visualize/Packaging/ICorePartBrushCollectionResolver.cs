using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uzi.Packaging;

namespace Uzi.Visualize.Packaging
{
    /// <summary>Resolves brush collections related under an ICorePart</summary>
    public class ICorePartBrushCollectionResolver : IResolveBrushCollection
    {
        /// <summary>Resolves brush collections related under an ICorePart</summary>
        public ICorePartBrushCollectionResolver(ICorePart part)
        {
            _Part = part;
        }

        private ICorePart _Part;

        public ICorePart Part { get { return _Part; } }

        #region IResolveBrushCollection Members

        public BrushCollection GetBrushCollection(object key)
        {
            if ((_Part != null) && (key != null))
            {
                var _bcPart = _Part.FindBasePart(key.ToString()) as BrushCollectionPart;
                if (_bcPart != null)
                    return _bcPart.BrushDefinitions;
            }
            return null;
        }

        public IResolveBrushCollection IResolveBrushCollectionParent { get { return null; } }

        public IEnumerable<BrushCollectionListItem> ResolvableBrushCollections
        {
            get
            {
                if (_Part != null)
                    return _Part.Relationships.OfType<BrushCollectionPart>()
                        .Select(_bc => new BrushCollectionListItem
                        {
                            BrushCollectionPart = _bc,
                            IsLocal = true
                        });

                // empty list
                return new BrushCollectionListItem[] { };
            }
        }

        #endregion
    }
}
