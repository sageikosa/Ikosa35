using Uzi.Core;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Actions
{
    public delegate bool DeliverBurstFilter(Locator loc, ICore core);
    public delegate bool ApplyBurstFilter(CoreStep step);

    public interface IPowerActionDef : IPowerDef
    {
        /// <summary>Action time for the power</summary>
        ActionTime ActionTime { get; }
    }
}
