using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Uzi.Visualize.Contracts;

namespace Uzi.Visualize
{
    [DataContract(Namespace = Statics.Namespace)]
    [Serializable]
    public enum OptionalAnchorFace
    {
        None, XLow, XHigh, YLow, YHigh, ZLow, ZHigh
    }

    public static class OptionalAnchorFaceHelper
    {
        public static AnchorFace ToAnchorFace(this OptionalAnchorFace self)
        {
            return (AnchorFace)((int)self - 1);
        }
    }
}
