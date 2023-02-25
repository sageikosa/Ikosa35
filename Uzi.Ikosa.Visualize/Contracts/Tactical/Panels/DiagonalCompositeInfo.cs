using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.Namespace)]
    public class DiagonalCompositeInfo : BasePanelInfo
    {
        public DiagonalCompositeInfo()
            : base()
        {
        }

        public DiagonalCompositeInfo(IBasePanel diagonalComposite)
            : base(diagonalComposite)
        {
        }
    }
}
