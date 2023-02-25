using System;
using System.Runtime.Serialization;
using Uzi.Visualize;
using Uzi.Visualize.Contracts;

namespace Uzi.Ikosa.Contracts
{
    // NOTE: to be used mostly for communication from client back to server
    [DataContract(Namespace = Statics.Namespace)]
    public class CubicBuilderInfo : BuilderInfo
    {
        public CubicBuilderInfo()
        {
        }

        public CubicBuilderInfo(CubicBuilder builder)
        {
            ZeroCell = new CellInfo(builder.ZeroCell);
            ZExtent = Convert.ToInt64(builder.Size.ZExtent);
            YExtent = Convert.ToInt64(builder.Size.YExtent);
            XExtent = Convert.ToInt64(builder.Size.XExtent);
        }

        [DataMember]
        public CellInfo ZeroCell { get; set; }
        [DataMember]
        public long ZExtent { get; set; }
        [DataMember]
        public long YExtent { get; set; }
        [DataMember]
        public long XExtent { get; set; }

        public override Visualize.IGeometryBuilder GetBuilder()
        {
            var _gSize = new GeometricSize(ZExtent, YExtent, XExtent);
            if (ZeroCell != null)
            {
                return new CubicBuilder(_gSize, ZeroCell);
            }
            return new CubicBuilder(_gSize);
        }
    }
}
