using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Uzi.Visualize.Packaging;

namespace Uzi.Visualize.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class BrushCollectionInfo
    {
        #region construction
        public BrushCollectionInfo()
        {
        }

        public BrushCollectionInfo(BrushCollectionPart brushPart)
        {
            Name = brushPart.Name;
            Bytes = brushPart.StreamBytes;
            BitmapImages = brushPart.ResolvableImages.Where(_ri => _ri.IsLocal).Select(_ri => _ri.BitmapImagePart.Name).ToArray();
        }
        #endregion

        [DataMember]
        public string ModelName { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public byte[] Bytes { get; set; }

        [DataMember]
        public string[] BitmapImages { get; set; }
    }
}
