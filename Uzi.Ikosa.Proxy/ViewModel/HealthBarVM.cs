using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class HealthBarVM : ViewModelBase
    {
        #region ctor()
        public HealthBarVM(HealthPointInfo health)
        {
            _Health = health;
        }
        #endregion

        #region data
        private HealthPointInfo _Health;
        #endregion

        public double Minimum
            => _Health.Current >= 0
            ? 0
            : _Health.DeadValue;

        public double Maximum
            => _Health.Current >= 0
            ? _Health.Maximum
            : 0;

        public double Current
            => _Health.Current;

        public Brush Background
            => _Health.Current >= 0
            ? Brushes.Gray
            : Brushes.Red;

        public Brush Foreground
            => (_Health.Current >= (_Health.Maximum / 2))
            ? Brushes.Green
            : _Health.Current >= 0
            ? Brushes.Yellow
            : Brushes.DarkGray;

        public void Conformulate(HealthPointInfo health)
        {
            var _updt = (_Health.Maximum != health.Maximum)
                || (_Health.Current != health.Current)
                || (_Health.DeadValue != health.DeadValue);

            _Health = health;

            if (_updt)
            {
                DoPropertyChanged(nameof(Minimum));
                DoPropertyChanged(nameof(Maximum));
                DoPropertyChanged(nameof(Current));
                DoPropertyChanged(nameof(Background));
                DoPropertyChanged(nameof(Foreground));
            }
        }
    }
}
