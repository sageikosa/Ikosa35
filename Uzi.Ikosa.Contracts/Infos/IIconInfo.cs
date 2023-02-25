using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Visualize;

namespace Uzi.Ikosa.Contracts.Infos
{
    public interface IIconInfo
    {
        ImageryInfo Icon { get; set; }

        /// <summary>
        /// Added by client-side View Model, tracked here for convenience in data-binding of client controls
        /// </summary>
        IResolveIcon IconResolver { get; set; }
    }
}
