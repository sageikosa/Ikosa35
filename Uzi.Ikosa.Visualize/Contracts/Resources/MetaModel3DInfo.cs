using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Uzi.Visualize.Packaging;

namespace Uzi.Visualize.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class MetaModel3DInfo : Model3DInfo
    {
        public MetaModel3DInfo()
            : base()
        {
        }

        public MetaModel3DInfo(MetaModel metaModel)
            : base(metaModel)
        {
            Fragments = metaModel.Fragments.Select(_f => _f.Key).ToArray();
            State = metaModel.State;
        }

        [DataMember]
        public string[] Fragments { get; set; }

        [DataMember]
        public MetaModelState State { get; set; }
    }
}
