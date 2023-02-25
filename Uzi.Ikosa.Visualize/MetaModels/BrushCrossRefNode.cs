using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;
using Uzi.Visualize.Contracts;

namespace Uzi.Visualize
{
    [DataContract(Namespace = Statics.Namespace)]
    public class BrushCrossRefNode
    {
        public BrushCrossRefNode()
        {
        }

        public BrushCrossRefNode(BrushCrossRefNode source)
        {
            ReferenceKey = source.ReferenceKey;
            BrushKey = source.BrushKey;
        }

        [DataMember]
        /// <summary>Key referenced in XAML</summary>
        public string ReferenceKey { get; set; }

        [DataMember]
        /// <summary>Key defined in BrushCollection</summary>
        public string BrushKey { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsExpanded { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSelected { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsActive { get; set; }
    }
}
