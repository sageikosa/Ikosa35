using Uzi.Core;

namespace Uzi.Ikosa.Objects
{
    public interface ILockMechanism : ICore
    {
        LockGroup LockGroup { get; set; }
    }
}
