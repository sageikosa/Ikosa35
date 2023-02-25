using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Uzi.Visualize;

namespace Uzi.Visualize.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class CellInfo : ICellLocation
    {
        public CellInfo()
        {
        }

        public CellInfo(ICellLocation cellLocation)
        {
            Z = cellLocation.Z;
            Y = cellLocation.Y;
            X = cellLocation.X;
        }

        [DataMember]
        public int Z { get; set; }
        [DataMember]
        public int Y { get; set; }
        [DataMember]
        public int X { get; set; }

        public CellPosition ToCellPosition() => new CellPosition(this);

    }
}
