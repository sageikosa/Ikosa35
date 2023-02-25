using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Proxy.ViewModel;
using Uzi.Ikosa.Proxy.VisualizationSvc;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Client.UI
{
    public abstract class BaseViewPoint : INotifyPropertyChanged
    {
        #region data
        protected SensorHostInfo _SensorHost;
        protected uint _State;
        #endregion

        public abstract Camera ViewPointCamera { get; }

        public abstract Point3D ViewPosition { get; }

        public virtual uint ViewPointState
        {
            get => _State;
            set => _State = value;
        }

        public virtual SensorHostInfo SensorHost
        {
            get { return _SensorHost; }
            set { _SensorHost = value; }
        }

        public virtual bool IsSelfVisible { get { return true; } }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void DoPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public static BaseViewPoint CreateViewPoint(ViewPointType viewPointType)
        {
            switch (viewPointType)
            {
                case ViewPointType.GameBoard:
                    return new PerspectiveGameBoardViewPoint();
                case ViewPointType.FirstPerson:
                    return new FirstPersonViewPoint();
                case ViewPointType.AimPoint:
                    return new AimPointViewPoint();
                case ViewPointType.ThirdPerson:
                default:
                    return new ThirdPersonViewPoint();
            }
        }
    }
}
