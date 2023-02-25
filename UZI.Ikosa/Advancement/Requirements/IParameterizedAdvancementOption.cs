using System.Collections.Generic;

namespace Uzi.Ikosa
{
    public interface IParameterizedAdvancementOption : IAdvancementOption
    {
        IAdvancementOption ParameterValue { get; set; }
        IEnumerable<IAdvancementOption> AvailableParameters { get; }
    }
}
