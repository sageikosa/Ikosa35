using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class PromptTurnTrackerPrerequisiteModel : PrerequisiteModel
    {
        public PromptTurnTrackerPrerequisiteInfo PromptTurnTrackerPrerequisiteInfo
            => Prerequisite as PromptTurnTrackerPrerequisiteInfo;
    }
}
