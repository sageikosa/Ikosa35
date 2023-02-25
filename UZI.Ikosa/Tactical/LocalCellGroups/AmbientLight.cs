using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.ComponentModel;
using Uzi.Packaging;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    /// <summary>Ambient light source for background cell groups</summary>
    public class AmbientLight : ICorePart, INotifyPropertyChanged
    {
        // TODO: NotifyLights local cell groups if properties change

        #region construction
        public AmbientLight(LocalMap map, string name, Vector3D vector, LightRange effectiveLevel,
            double linkVeryBright, double linkBright, double linkNear, double linkFar)
        {
            _Map = map;
            _Name = name;
            _Vector = vector;
            _Level = effectiveLevel;
            _LinkVeryBright = linkVeryBright;
            _LinkBright = linkBright;
            _LinkNearShadow = linkNear;
            _LinkFarShadow = linkFar;
        }
        #endregion

        #region private data
        private LocalMap _Map;
        private string _Name;
        private Vector3D _Vector;
        private LightRange _Level;
        private double _LinkVeryBright;
        private double _LinkBright;
        private double _LinkNearShadow;
        private double _LinkFarShadow;
        #endregion

        #region ICorePart Members

        #region public string Name { get; set; }
        public string Name
        {
            get { return _Name; }
            set
            {
                _Map.AmbientLights.Remove(_Name);
                _Name = value;
                _Map.AmbientLights.Add(_Name, this);
                DoPropertyChanged(@"Name");
            }
        }
        #endregion

        public IEnumerable<ICorePart> Relationships { get { yield break; } }
        public string TypeName
            => GetType().FullName;

        #endregion

        #region INotifyPropertyChanged Members
        protected void DoPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public LocalMap Map => _Map;

        public bool IsSolar
        {
            get { return AmbientLevel >= LightRange.Solar; }
            set
            {
                if (AmbientLevel >= LightRange.VeryBright)
                {
                    AmbientLevel = value ? LightRange.Solar : LightRange.VeryBright;
                    DoPropertyChanged(@"AmbientLevel");
                }
            }
        }

        #region public double ZOffset { get; set; }
        public double ZOffset
        {
            get { return _Vector.Z; }
            set
            {
                _Vector.Z = value;
                DoPropertyChanged(@"ZOffset");
                DoPropertyChanged(@"Vector3D");
            }
        }
        #endregion

        #region public double YOffset { get; set; }
        public double YOffset
        {
            get { return _Vector.Y; }
            set
            {
                _Vector.Y = value;
                DoPropertyChanged(@"YOffset");
                DoPropertyChanged(@"Vector3D");
            }
        }
        #endregion

        #region public double XOffset { get; set; }
        public double XOffset
        {
            get { return _Vector.X; }
            set
            {
                _Vector.X = value;
                DoPropertyChanged(@"XOffset");
                DoPropertyChanged(@"Vector3D");
            }
        }
        #endregion

        #region public Vector3D Vector3D { get; set; }
        public Vector3D Vector3D
        {
            get { return _Vector; }
            set
            {
                _Vector = value;
                DoPropertyChanged(@"ZOffset");
                DoPropertyChanged(@"YOffset");
                DoPropertyChanged(@"XOffset");
                DoPropertyChanged(@"Vector3D");
            }
        }
        #endregion

        #region public LightRange AmbientLevel { get; set; }
        public LightRange AmbientLevel
        {
            get { return _Level; }
            set
            {
                _Level = value;
                DoPropertyChanged(@"AmbientLevel");
            }
        }
        #endregion

        #region public double VeryBright { get; set; }
        public double VeryBright
        {
            get { return _LinkVeryBright; }
            set
            {
                _LinkVeryBright = value;
                DoPropertyChanged(@"VeryBright");
            }
        }
        #endregion

        #region public double Bright { get; set; }
        public double Bright
        {
            get { return _LinkBright; }
            set
            {
                _LinkBright = value;
                DoPropertyChanged(@"Bright");
            }
        }
        #endregion

        #region public double NearShadow { get; set; }
        public double NearShadow
        {
            get { return _LinkNearShadow; }
            set
            {
                _LinkNearShadow = value;
                DoPropertyChanged(@"NearShadow");
            }
        }
        #endregion

        #region public double FarShadow { get; set; }
        public double FarShadow
        {
            get { return _LinkFarShadow; }
            set
            {
                _LinkFarShadow = value;
                DoPropertyChanged(@"FarShadow");
            }
        }
        #endregion
    }
}
