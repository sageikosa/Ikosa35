using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uzi.Core
{
    [Serializable]
    public abstract class Pathed : Adjunct
    {
        protected Pathed(object source) : base(source) { }
        public abstract IAdjunctable GetPathParent();
        public abstract string GetPathPartString();

        public override Guid? MergeID => null;
        public abstract void UnPath();

        /// <summary>
        /// Called when path established, or on demand if path significantly changed in some meaningful way
        /// </summary>
        public void RefreshPath()
        {
            // inform object that path changed
            Anchor?.Setting?.ContextSet.PathChanged(Anchor);

            // anchored, let our new adjuncts know it
            if (Anchor is ICoreObject _coreObj)
            {
                foreach (var _adj in _coreObj.GetAllConnectedAdjuncts<IPathDependent>().ToList())
                {
                    _adj.PathChanged(this);
                }

                if (_coreObj.Setting != null)
                {
                    // inform AllConnected objects that path changed
                    foreach (var _o in _coreObj.AllConnected(null).ToList())
                    {
                        _coreObj.Setting.ContextSet.PathChanged(_o);
                    }
                }
            }
            else
                foreach (var _adj in Anchor.Adjuncts.OfType<IPathDependent>().ToList())
                {
                    _adj.PathChanged(this);
                }
        }

        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            if ((Anchor == null) && (oldAnchor != null))
            {
                // inform object that path changed
                oldSetting?.ContextSet.PathChanged(oldAnchor);

                // unanchored, let our old adjuncts know it
                if (oldAnchor is ICoreObject _coreObj)
                {
                    foreach (var _adj in _coreObj.GetAllConnectedAdjuncts<IPathDependent>().ToList())
                    {
                        _adj.PathChanged(this);
                    }

                    if (oldSetting != null)
                    {
                        // inform AllConnected objects that path changed
                        foreach (var _o in _coreObj.AllConnected(null).ToList())
                        {
                            oldSetting.ContextSet.PathChanged(_o);
                        }
                    }
                }
                else
                    foreach (var _adj in oldAnchor.Adjuncts.OfType<IPathDependent>().ToList())
                    {
                        _adj.PathChanged(this);
                    }
            }
            else
            {
                RefreshPath();
            }
            base.OnAnchorSet(oldAnchor, oldSetting);
        }
    }

    public static class PathHelper
    {
        public static Pathed GetPathed(this IAdjunctSet self)
            => self.Adjuncts.OfType<Pathed>().FirstOrDefault(_p => _p.IsActive);

        #region public static string GetPath(this IAdjunctSet self)
        /// <summary>Provides string representation of containment path</summary>
        public static string GetPath(this IAdjunctSet self)
        {
            var _pathStack = new Stack<string>();

            IAdjunctSet _next = self;
            while (_next != null)
            {
                // if next has no path, exit loop
                var _path = _next.GetPathed();
                if (_path == null)
                    break;

                // otherwise, push part and get next parent
                _pathStack.Push(_path.GetPathPartString());
                _next = _path.GetPathParent();
            }

            // return string
            var _final = new StringBuilder(@"/");
            while (_pathStack.Count > 0)
            {
                var _pop = _pathStack.Pop();
                if (!string.IsNullOrEmpty(_pop))
                    _final.AppendFormat(@"/{0}", _pop);
            }
            return _final.ToString();
        }
        #endregion

        public static bool DoesPathContain(this IAdjunctSet self, IAdjunctSet target)
            => (self == target)
            || (self.GetPathed()?.GetPathParent()?.DoesPathContain(target) ?? false);

        /// <summary>True if the adjunctable is currently has some adjunct-association path (bound, held, located, contained, slotted, etc.)</summary>
        public static bool IsPathed(this IAdjunctSet self)
            => self.HasActiveAdjunct<Pathed>();

        /// <summary>Any path to setting is removed, effectively removing the entity from game-play.</summary>
        public static void UnPath(this IAdjunctSet self)
        {
            foreach (var _path in self.Adjuncts.OfType<Pathed>().ToList())
            {
                _path.UnPath();
            }
        }

        /// <summary>True if the adjunct is active on self or up the path to the root</summary>
        public static bool PathHasActiveAdjunct<Adj>(this IAdjunctSet self, Adj exclude = null)
            where Adj : Adjunct
            => self.HasActiveAdjunct<Adj>(exclude)
            || self.ParentPathHasActiveAdjunct<Adj>();

        /// <summary>True if the adjunct is active somewhere up the path to the root</summary>
        private static bool ParentPathHasActiveAdjunct<Adj>(this IAdjunctSet self, Adj exclude = null)
            where Adj : Adjunct
            => self.GetPathed()?.GetPathParent()?.PathHasActiveAdjunct<Adj>(exclude) ?? false;
    }
}