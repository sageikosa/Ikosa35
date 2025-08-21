using System.Runtime.Serialization;
using Uzi.Visualize.Contracts;

namespace Uzi.Visualize
{
    [DataContract(Namespace = Statics.Namespace)]
    public class MetaModelRootNode : MetaModelFragmentNode
    {
        public MetaModelRootNode()
            : base()
        {
        }

        public MetaModelRootNode(MetaModelRootNode source)
            : base(source)
        {
        }
    }
}
