namespace Uzi.Core
{
    /// <summary>Provides access to capabilities that may be replaceable</summary>
    public interface ICapabilityRoot
    {
        /// <summary>
        /// Gets an ICapability interface from the Root
        /// </summary>
        /// <typeparam name="Capability">type of interface to fetch</typeparam>
        /// <returns></returns>
        Capability GetCapability<Capability>() where Capability : class, ICapability;
    }
}
