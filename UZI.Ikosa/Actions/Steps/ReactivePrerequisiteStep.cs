using Newtonsoft.Json;
using System;
using Uzi.Core;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Creates a prerequisite step that automatically enqueues itself in the supplied process's reactive steps.</summary>
    [Serializable]
    public class ReactivePrerequisiteStep : PreReqListStepBase, IControlChange<bool>
    {
        /// <summary>
        /// Creates a prerequisite step that automatically enqueues itself in the supplied process's reactive steps.
        /// </summary>
        public ReactivePrerequisiteStep(CoreProcess process, params StepPrerequisite[] preReqs)
            : base(process)
        {
            foreach (var _pre in preReqs)
            {
                _PendingPreRequisites.Enqueue(_pre);
            }

            process.AppendPreEmption(this);
            _SuccessCtrl = new ChangeController<bool>(this, true);
        }

        protected override bool OnDoStep()
        {
            StepPrerequisite _fail = FailingPrerequisite;
            if (_fail != null)
            {
                _SuccessCtrl.DoValueChanged(false);
            }
            else
            {
                _SuccessCtrl.DoValueChanged(true);
            }

            // DONE
            return true;
        }

        #region IControlChange<bool> Members
        private ChangeController<bool> _SuccessCtrl;
        public void AddChangeMonitor(IMonitorChange<bool> monitor)
        {
            _SuccessCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<bool> monitor)
        {
            _SuccessCtrl.RemoveChangeMonitor(monitor);
        }
        #endregion

        #region INotifyPropertyChanged Members
        [field:NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
