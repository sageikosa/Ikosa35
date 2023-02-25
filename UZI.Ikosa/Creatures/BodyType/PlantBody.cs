using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Creatures.BodyType
{
    [SourceInfo(@"Plant")]
    [Serializable]
    public class PlantBody : Body
    {
        #region ctor
        public PlantBody(Material bodyMaterial, Size size, int reach)
            : base(size, bodyMaterial, false, reach, false)
        {
        }
        #endregion

        protected override Body InternalClone(Material material)
        {
            var _body = new PlantBody(material, Sizer.BaseSize, ReachSquares.BaseValue);
            _body.Sizer.NaturalSize = Sizer.NaturalSize;
            return _body;
        }

        public override bool HasAnatomy => false;
        public override bool HasBones => false;
    }
}
