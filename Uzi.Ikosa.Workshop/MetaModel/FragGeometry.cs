using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Uzi.Visualize;
using System.Windows.Media.Media3D;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// ViewModel for Fragment Geometry Properties
    /// </summary>
    public class FragGeometry : INotifyPropertyChanged
    {
        public FragGeometry(MetaModelFragmentNode node)
        {
            _Node = node;
        }

        private MetaModelFragmentNode _Node;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void DoPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        #endregion

        #region public bool NoseUp { get; set; }
        public bool NoseUp
        {
            get { return _Node.NoseUp; }
            set
            {
                _Node.NoseUp = value;
                DoPropertyChanged(@"NoseUp");
            }
        }
        #endregion
        #region public double Roll { get; set; }
        public double Roll
        {
            get { return _Node.Roll ?? 0; }
            set
            {
                _Node.Roll = value == 0 ? (double?)null : value;
                DoPropertyChanged(@"Roll");
            }
        }
        #endregion
        #region public double Pitch { get; set; }
        public double Pitch
        {
            get { return _Node.Pitch ?? 0; }
            set
            {
                _Node.Pitch = value == 0 ? (double?)null : value;
                DoPropertyChanged(@"Pitch");
            }
        }
        #endregion
        #region public double Yaw { get; set; }
        public double Yaw
        {
            get { return _Node.Yaw ?? 0; }
            set
            {
                _Node.Yaw = value == 0 ? (double?)null : value;
                DoPropertyChanged(@"Yaw");
            }
        }
        #endregion

        #region Scale: ScaleX, ScaleY, ScaleZ
        private Vector3D Scale
        {
            get { return _Node.Scale ?? new Vector3D(1, 1, 1); } // TODO: ???
            set
            {
                if ((value.X == 1) && (value.Y == 1) && (value.Z == 1))
                {
                    _Node.Scale = null;
                }
                else
                {
                    _Node.Scale = value;
                }
            }
        }

        public double ScaleX
        {
            get { return Scale.X; }
            set
            {
                var _scale = Scale;
                _scale.X = value;
                Scale = _scale;
                DoPropertyChanged(@"ScaleX");
            }
        }
        public double ScaleY
        {
            get { return Scale.Y; }
            set
            {
                var _scale = Scale;
                _scale.Y = value;
                Scale = _scale;
                DoPropertyChanged(@"ScaleY");
            }
        }
        public double ScaleZ
        {
            get { return Scale.Z; }
            set
            {
                var _scale = Scale;
                _scale.Z = value;
                Scale = _scale;
                DoPropertyChanged(@"ScaleZ");
            }
        }
        #endregion

        #region Offset: OffsetX, OffsetY, OffsetZ
        private Vector3D Offset
        {
            get { return _Node.Offset ?? new Vector3D(); }
            set
            {
                if (value.LengthSquared == 0)
                {
                    _Node.Offset = null;
                }
                else
                {
                    _Node.Offset = value;
                }
            }
        }

        public double OffsetX
        {
            get { return Offset.X; }
            set
            {
                var _off = Offset;
                _off.X = value;
                Offset = _off;
                DoPropertyChanged(@"OffsetX");
            }
        }
        public double OffsetY
        {
            get { return Offset.Y; }
            set
            {
                var _off = Offset;
                _off.Y = value;
                Offset = _off;
                DoPropertyChanged(@"OffsetY");
            }
        }
        public double OffsetZ
        {
            get { return Offset.Z; }
            set
            {
                var _off = Offset;
                _off.Z = value;
                Offset = _off;
                DoPropertyChanged(@"OffsetZ");
            }
        }
        #endregion

        #region OriginOffset: OriginOffsetX, OriginOffsetY, OriginOffsetZ
        private Vector3D OriginOffset
        {
            get { return _Node.OriginOffset ?? new Vector3D(); }
            set
            {
                if (value.LengthSquared == 0)
                {
                    _Node.OriginOffset = null;
                }
                else
                {
                    _Node.OriginOffset = value;
                }
            }
        }

        public double OriginOffsetX
        {
            get { return OriginOffset.X; }
            set
            {
                var _off = OriginOffset;
                _off.X = value;
                OriginOffset = _off;
                DoPropertyChanged(@"OriginOffsetX");
            }
        }
        public double OriginOffsetY
        {
            get { return OriginOffset.Y; }
            set
            {
                var _off = OriginOffset;
                _off.Y = value;
                OriginOffset = _off;
                DoPropertyChanged(@"OriginOffsetY");
            }
        }
        public double OriginOffsetZ
        {
            get { return OriginOffset.Z; }
            set
            {
                var _off = OriginOffset;
                _off.Z = value;
                OriginOffset = _off;
                DoPropertyChanged(@"OriginOffsetZ");
            }
        }
        #endregion
    }
}
