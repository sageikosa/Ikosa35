using System;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>Used to show what choices actions have been bound to values</summary>
    [Serializable]
    public class ChoiceBinder
    {
        /// <summary>Used to show what choices actions have been bound to values</summary>
        public ChoiceBinder(ActionBase action, CoreActor actor, bool isExternal, OptionAimOption option)
            : base()
        {
            Action = action;
            Actor = actor;
            IsExternal = isExternal;
            Option = option;
        }

        // everthing needed to make an ActionInfo
        public ActionBase Action { get; set; }
        public CoreActor Actor { get; set; }
        public bool IsExternal { get; set; }

        // choice value
        public OptionAimOption Option { get; set; }
    }
}