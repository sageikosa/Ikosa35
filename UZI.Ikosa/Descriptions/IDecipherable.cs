using System;
using Uzi.Core;

namespace Uzi.Ikosa.Descriptions
{
    public interface IDecipherable
    {
        CoreStep Decipher(CoreActivity process);
        bool HasDeciphered(Guid guid);
    }
}
