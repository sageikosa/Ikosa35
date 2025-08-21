using System.Linq;
using System.Runtime.Serialization;
using Uzi.Visualize.Packaging;

namespace Uzi.Visualize.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class Model3DInfo
    {
        #region construction
        public Model3DInfo()
        {
        }

        public Model3DInfo(Model3DPart modelPart)
        {
            Name = modelPart.Name;
            Bytes = modelPart.StreamBytes;
            BitmapImages = modelPart.ResolvableImages.Where(_ri => _ri.IsLocal).Select(_ri => _ri.BitmapImagePart.Name).ToArray();
            Brushes = new BrushCollectionInfo(modelPart.Brushes) { ModelName = modelPart.Name };
        }
        #endregion

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public byte[] Bytes { get; set; }

        [DataMember]
        public string[] BitmapImages { get; set; }

        [DataMember]
        public BrushCollectionInfo Brushes { get; set; }
    }
}
