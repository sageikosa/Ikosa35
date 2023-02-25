using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class ContentStack
    {
        public ContentStack(Orientation orientation, params object[] contents)
        {
            _Contents = contents.ToList();
            _Orientation = orientation;
        }

        #region data
        private List<object> _Contents;
        private Orientation _Orientation;
        #endregion

        public IEnumerable<object> Contents => _Contents.Select(_i => _i);
        public Orientation Orientation => _Orientation;
    }
}
