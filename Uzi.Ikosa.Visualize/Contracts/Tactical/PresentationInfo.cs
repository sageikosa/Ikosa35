using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Media.Media3D;
using Uzi.Visualize;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.TacticalNamespace)]
    public class PresentationInfo : CubicInfo, IPresentation
    {
        #region construction
        public PresentationInfo()
        {
        }

        public PresentationInfo(Presentation presentation, IEnumerable<Guid> presentedIDs)
        {
            PresentingIDs = presentedIDs.ToArray();

            // visual effects
            VisualEffectBytes = new Dictionary<string, byte>(presentation.VisualEffects.Count);
            foreach (var _kvp in presentation.VisualEffects)
            {
                VisualEffectBytes.Add(_kvp.Type.AssemblyQualifiedName, (byte)_kvp.Effect);
            }

            // external values
            ExternalValues = new Dictionary<string, int>();
            foreach (var _kvp in presentation.ExternalValues)
            {
                ExternalValues.Add(_kvp.Key, _kvp.Value);
            }

            // presentation cube
            Z = presentation.Z;
            Y = presentation.Y;
            X = presentation.X;
            ZTop = (int)(presentation.Z + presentation.ZHeight - 1);
            YTop = (int)(presentation.Y + presentation.YLength - 1);
            XTop = (int)(presentation.X + presentation.XLength - 1);
            BaseFaceValue = (int)presentation.BaseFace;
            MoveFrom = presentation.MoveFrom;
            SerialState = presentation.SerialState;
        }
        #endregion

        [DataMember]
        public Dictionary<string, byte> VisualEffectBytes { get; set; }
        [DataMember]
        public Guid[] PresentingIDs { get; set; }
        [DataMember]
        public int BaseFaceValue { get; set; }
        [DataMember]
        public Vector3D MoveFrom { get; set; }
        [DataMember]
        public ulong SerialState { get; set; }
        [DataMember]
        public Dictionary<string, int> ExternalValues { get; set; }

        // TODO: animation mode...(smooth, jump)

        private List<VisualEffectValue> _Effects = null;

        #region public List<VisualEffectValue> VisualEffects { get; }
        public List<VisualEffectValue> VisualEffects
        {
            get
            {
                if (_Effects == null)
                {
                    _Effects = new List<VisualEffectValue>();
                    foreach (var _kvp in VisualEffectBytes)
                    {
                        try
                        {
                            var _type = Type.GetType(_kvp.Key);
                            _Effects.Add(new VisualEffectValue(_type, (VisualEffect)_kvp.Value));
                        }
                        catch
                        {
                            _Effects.Add(new VisualEffectValue(typeof(SenseEffectExtension), VisualEffect.Skip));
                        }
                    }
                }
                return _Effects;
            }
        }
        #endregion

        public AnchorFace BaseFace
            => (AnchorFace)BaseFaceValue;

        public virtual Presentable GetPresentable(IResolveIcon iconResolver, IResolveModel3D modelResolver, IEnumerable<PresentationInfo> selected,
            Point3D sourcePosition, int heading, IZoomIcons zoomIcons)
            => null;

        public long GetAxialLength(Axis axis) =>
            axis == Axis.Z ? ZHeight :
            axis == Axis.Y ? YLength :
            XLength;
    }
}
