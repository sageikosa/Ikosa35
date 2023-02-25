using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    public class ListableList<X>
    {
        public ListableList(List<X> x)
        {
            _List = x;
        }

        private List<X> _List;

        public IEnumerable<X> All { get { return _List.AsEnumerable(); } }
    }
}
