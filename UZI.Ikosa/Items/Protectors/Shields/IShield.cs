using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Shields
{
    [SourceInfo(@"Shield")]
    public interface IShield : IProtectorItem
    {
        bool Tower { get; }
        bool UseHandToCarry { get; }
        int OpposedDelta { get; }
    }
}
