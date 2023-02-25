namespace Uzi.Core
{
    public interface IActionSource : ICore
    {
        IVolatileValue ActionClassLevel { get; }
    }
}
