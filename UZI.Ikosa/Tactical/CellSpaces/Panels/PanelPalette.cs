using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class PanelPalette
    {
        public PanelPalette()
        {

            _Panels = [];
        }

        private Dictionary<string, BasePanel> _Panels;

        public bool ContainsKey(string key)
        {
            return _Panels.ContainsKey(key);
        }

        public bool Add(BasePanel panel)
        {
            if (!_Panels.ContainsKey(panel.Name))
            {
                _Panels.Add(panel.Name, panel);
                return true;
            }
            return false;
        }

        public bool Remove(BasePanel panel)
        {
            if (_Panels.ContainsKey(panel.Name))
            {
                return _Panels.Remove(panel.Name);
            }

            return false;
        }

        public BasePanel this[string key]
        {
            get
            {
                if (_Panels.ContainsKey(key))
                {
                    return _Panels[key];
                }

                return null;
            }
            set
            {
                _Panels[key] = value;
            }
        }

        public int Count { get { return _Panels.Count; } }

        public IEnumerable<BasePanel> All()
        {
            return _Panels.OrderBy(_kvp =>_kvp.Key).Select(_kvp => _kvp.Value);
        }
    }
}
