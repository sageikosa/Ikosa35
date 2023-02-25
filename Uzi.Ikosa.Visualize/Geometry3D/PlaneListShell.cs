using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uzi.Visualize
{
    public class PlaneListShell : PlanarShell
    {
        public PlaneListShell()
            : base()
        {
        }

        public void Add(PlanarPoints plane)
        {
            _Faces.Add(plane);
        }

        public void Remove(PlanarPoints plane)
        {
            if (_Faces.Contains(plane))
                _Faces.Remove(plane);
        }

        public void Add(PlanarShell shell)
        {
            foreach (var _f in shell.Faces)
                _Faces.Add(_f);
        }
    }
}
