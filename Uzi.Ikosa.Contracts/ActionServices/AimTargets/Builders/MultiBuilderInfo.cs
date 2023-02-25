using System;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Visualize;

namespace Uzi.Ikosa.Contracts
{
    // NOTE: to be used mostly for communication from client back to server
    [DataContract(Namespace = Statics.Namespace)]
    public class MultiBuilderInfo : BuilderInfo
    {
        public MultiBuilderInfo()
        {
        }

        public MultiBuilderInfo(MultiBuilder builder, Func<IGeometryBuilder, BuilderInfo> converter)
        {
            Includes = (from _inc in builder.Includes
                        let _rslt = converter(_inc)
                        where _rslt != null
                        select _rslt).ToArray();
            Excludes = (from _excl in builder.Excludes
                        let _rslt = converter(_excl)
                        where _rslt != null
                        select _rslt).ToArray();
        }

        [DataMember]
        public BuilderInfo[] Includes { get; set; }
        [DataMember]
        public BuilderInfo[] Excludes { get; set; }

        public override IGeometryBuilder GetBuilder()
        {
            if ((Excludes != null) && (Excludes.Length > 0))
            {
                return new MultiBuilder(
                    Includes.Select(_i => _i.GetBuilder()),
                    Excludes.Select(_i => _i.GetBuilder()));
            }
            return new MultiBuilder(Includes.Select(_i => _i.GetBuilder()));
        }
    }
}
