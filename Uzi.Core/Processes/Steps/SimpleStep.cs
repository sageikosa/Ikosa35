using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Core
{
    /// <summary>
    /// Meant to wrap ISimpleStep; no prerequisites
    /// </summary>
    [Serializable]
    public class SimpleStep : CoreStep
    {
        /// <summary>
        /// Meant to wrap ISimpleStep; no prerequisites
        /// </summary>
        public SimpleStep(CoreProcess process, ISimpleStep simpleStep)
            : base(process)
        {
            _SimpleStep = simpleStep;
        }

        #region state
        private ISimpleStep _SimpleStep;
        #endregion

        public override bool IsDispensingPrerequisites => false;
        protected override StepPrerequisite OnNextPrerequisite() => null;

        protected override bool OnDoStep()
            => _SimpleStep?.DoStep(this) ?? true;
    }
}
