using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.TacticalNamespace)]
    public class IconPresentationInfo : PresentationInfo
    {
        #region construction
        public IconPresentationInfo()
            : base()
        {
        }

        public IconPresentationInfo(IconPresentation presentation, IEnumerable<Guid> presentedIDs)
            : base(presentation, presentedIDs)
        {
            IconRefs = presentation.IconRefs;
        }
        #endregion

        [DataMember]
        public List<IconReferenceInfo> IconRefs { get; set; }

        #region public override Presentable GetPresentable(...)
        public override Presentable GetPresentable(IResolveIcon iconResolver, IResolveModel3D modelResolver, IEnumerable<PresentationInfo> selected,
            Point3D sourcePosition, int heading, IZoomIcons zoomIcons)
        {
            // icon bill-board presentation
            var _model = ModelGenerator.GenerateMarker(iconResolver, VisualEffects,
                IconRefs, sourcePosition, GetPoint3D(), BaseFace, heading);//,
                //PresentingIDs.Contains(zoomIcons.ZoomedIcon) ? zoomIcons.ZoomLevel : zoomIcons.UnZoomLevel);

            return new Presentable
            {
                Model3D = _model,
                Presentations = new[] { this },
                Presenter = this,
                MoveFrom = MoveFrom,
                SerialState = SerialState
            };
        }
        #endregion
    }
}