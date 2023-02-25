using System;

namespace Uzi.Ikosa.Senses
{
    public interface IActionAwareProvider
    {
        /// <summary>True if this provider indicates that the given guid should be treated as action aware</summary>
        bool? IsActionAware(Guid guid);
    }
}
