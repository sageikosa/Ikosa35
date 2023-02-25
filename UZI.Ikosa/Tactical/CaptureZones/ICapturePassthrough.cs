using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Tactical
{
    public interface ICapturePassthrough : ICore
    {
        /// <summary>ICore that make up this ICapturePassthrough</summary>
        IEnumerable<ICore> Contents { get; }
    }
}
